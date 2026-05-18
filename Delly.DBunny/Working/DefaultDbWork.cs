using Delly.DBunny;
using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Transactions;

namespace Delly.DBunny.Working
{
    /// <summary>
    /// 简单的数据库作业
    /// </summary>
    public class DefaultDbWork : IDisposable, IDbWork
    {

        private readonly List<Sqled> _sqleds;
        private readonly IDbManager _manager;
        private readonly IDbProvider _provider;
        private readonly DbConnectionDescriptor _connectionDescriptor;
        private readonly IEnumerable<IDbFilter> _filters;

        /// <summary>
        /// 简单的数据库作业
        /// </summary>
        public DefaultDbWork(
            IDbManager manager,
            IDbProvider provider,
            DbConnectionDescriptor connectionDescriptor,
            IEnumerable<IDbFilter> filters
            )
        {
            _sqleds = new List<Sqled>();
            _manager = manager;
            _provider = provider;
            _connectionDescriptor = connectionDescriptor;
            _filters = filters;
        }

        /// <summary>
        /// 工作者管理器
        /// </summary>
        public IDbManager Manager => _manager;

        /// <summary>
        /// 命令集合
        /// </summary>
        public List<Sqled> Sqleds => _sqleds;

        /// <summary>
        /// 提供程序
        /// </summary>
        public IDbProvider Provider => _provider;

        /// <summary>
        /// 连接描述器
        /// </summary>
        public DbConnectionDescriptor ConnectionDescriptor => _connectionDescriptor;

        /// <summary>
        /// 过滤器 集合
        /// </summary>
        public IEnumerable<IDbFilter> Filters => _filters;

        /// <summary>
        /// 获取Sql命令
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public DbCommand GetSqlCommand(DbConnection dbc, Sqled sqled)
        {
            var command = CommandCreating(dbc);
            if (command is null) { command = _provider.GetDbCommand(dbc); }
            // 兼容Sql装载处理
            sqled = SqlLoading(sqled);
            command.CommandText = sqled.Sql;
            _provider.SetParameters(command, sqled.Parameters);
            return CommandExecuting(command);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public DbConnection Connect()
        {
            var dbc = _provider.GetDbConnection(_connectionDescriptor.ConnectionString);
            dbc.Open();
            return dbc;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public async Task<DbConnection> ConnectAsync()
        {
            var dbc = _provider.GetDbConnection(_connectionDescriptor.ConnectionString);
            await dbc.OpenAsync();
            return dbc;
        }


        #region 同步事务

        /// <summary>
        /// 生效事务
        /// </summary>
        public int Commit()
        {
            if (_sqleds.Count <= 0) { return 0; }
            using (var conn = _provider.GetDbConnection(_connectionDescriptor.ConnectionString))
            {
                return CommitConnection(conn);
            }
        }

        // 生效连接事务
        private int CommitConnection(DbConnection conn)
        {
            int result = 0;
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    result = CommitTransaction(conn, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            }
            _sqleds.Clear();
            conn.Close();
            return result;
        }

        // 生效事务
        private int CommitTransaction(DbConnection conn, DbTransaction transaction)
        {
            int result = 0;
            foreach (var sqled in _sqleds)
            {
                using (var sqlCommand = GetExecutingCommand(conn, transaction, sqled))
                {
                    try
                    {
                        result += sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new SqlException(sqled, ex);
                    }
                }
            }
            return result;
        }

        #endregion

        #region 异步事务

        /// <summary>
        /// 生效事务
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        public async Task<int> CommitAsync()
        {
            if (_sqleds.Count <= 0) { return 0; }
            using (var conn = _provider.GetDbConnection(_connectionDescriptor.ConnectionString))
            {
                return await CommitConnectionAsync(conn);
            }
        }

        // 生效连接事务
        private async Task<int> CommitConnectionAsync(DbConnection conn)
        {
            int result = 0;
            await conn.OpenAsync();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    result = await CommitTransactionAsync(conn, transaction);
#if NETSTANDARD2_0
                    transaction.Commit();
#else
                    await transaction.CommitAsync();
#endif
                }
                catch
                {
#if NETSTANDARD2_0
                    transaction.Rollback();
#else
                    await transaction.RollbackAsync();
#endif
                    throw;
                }
            }
            _sqleds.Clear();
#if NETSTANDARD2_0
            conn.Close();
#else
            await conn.CloseAsync();
#endif
            return result;
        }

        // 生效事务
        private async Task<int> CommitTransactionAsync(DbConnection conn, DbTransaction transaction)
        {
            int result = 0;
            foreach (var sqled in _sqleds)
            {
                using (var sqlCommand = GetExecutingCommand(conn, transaction, sqled))
                {
                    try
                    {
                        result += await sqlCommand.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        throw new SqlException(sqled, ex);
                    }
                }
            }
            return result;
        }

        #endregion

        // 获取命令执行
        private DbCommand GetExecutingCommand(DbConnection connection, DbTransaction transaction, Sqled sqled)
        {
            var sqlCommand = CommandCreating(connection);
            if (sqlCommand is null) { sqlCommand = _provider.GetDbCommand(connection); }
            // 兼容Sql装载处理
            sqled = SqlLoading(sqled);
            sqlCommand.Transaction = transaction;
            sqlCommand.CommandText = sqled.Sql;
            _provider.SetParameters(sqlCommand, sqled.Parameters);
            return CommandExecuting(sqlCommand);
        }

        /// <summary>
        /// SqlSet装载
        /// </summary>
        /// <returns></returns>
        private Sqled SqlLoading(Sqled sqled)
        {
            foreach (var filter in _filters)
            {
                sqled = filter.SqlLoading(sqled);
            }
            return sqled;
        }

        /// <summary>
        /// 命令管理器创建
        /// </summary>
        /// <returns></returns>
        private DbCommand CommandCreating(DbConnection connection)
        {
#if NETSTANDARD2_0
            DbCommand command = null;
#else
            DbCommand? command = null;
#endif
            foreach (var filter in _filters)
            {
                command = filter.CommandCreating(command);
            }
            if (command is null) { command = _provider.GetDbCommand(connection); }
            return command;
        }

        /// <summary>
        /// 命令管理器执行
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        private DbCommand CommandExecuting(DbCommand command)
        {
            foreach (var filter in _filters)
            {
                command = filter.CommandExecuting(command);
            }
            return command;
        }

        #region 对象释放

        private bool _disposed = false;

        /// <summary>
        /// 构析函数
        /// </summary>
        ~DefaultDbWork()
        {
            Dispose();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) { return; }
            _disposed = true;
            // 释放工作者
            _manager.ReleaseWork();
            GC.SuppressFinalize(this);
        }

        public DbCommand GetCommand(Sqled sqled)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}


