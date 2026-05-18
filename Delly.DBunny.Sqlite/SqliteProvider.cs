using Delly.DBunny.Core;
using Delly.DBunny.Core.Providing.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Delly.DBunny.Sqlite
{
    /// <summary>
    /// Sqlite 提供程序
    /// </summary>
    public sealed class SqliteProvider : IDbProvider
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType => SqliteConnectionDefine.DATABASE_TYPE;

        /// <summary>
        /// Sql 提供程序
        /// </summary>
        public ISqlProvider SqlProvider { get; } = new SqliteSqlProvider();

        /// <summary>
        /// 获取 DataSet
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <returns>数据集</returns>
        public DataSet GetDataSet(DbCommand command)
        {
            var sqlDataAdapter = new SQLiteDataAdapter { SelectCommand = (SQLiteCommand)command };
            var dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }

        /// <summary>
        /// 获取数据库命令管理器
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>数据库命令</returns>
        public DbCommand GetDbCommand(DbConnection connection)
        {
            var sqlCommand = new SQLiteCommand
            {
                CommandTimeout = 600,
                Connection = (SQLiteConnection)connection
            };
            return sqlCommand;
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>数据库连接</returns>
        public DbConnection GetDbConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        /// <summary>
        /// 设置参数集
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <param name="parameters">参数集合</param>
        public void SetParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            command.Parameters.Clear();
            foreach (var param in parameters)
            {
                command.Parameters.Add(new SQLiteParameter("@" + param.Key, param.Value));
            }
        }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>Schema 名称列表</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<IReadOnlyList<string>> GetSchemas(DbConnection connection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称（SQLite 中忽略）</param>
        /// <returns>表描述符列表</returns>
        public async Task<IReadOnlyList<DbTableDesciptor>> GetTables(DbConnection connection, string schema)
        {
            var tables = new List<DbTableDesciptor>();
            var sql = SqlProvider.GetTables(schema);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(new DbTableDesciptor()
                    {
                        TableName = reader.GetString(0)
                    });
                }
            });
            return tables;
        }

        /// <summary>
        /// 获取表中的所有列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称（SQLite 中忽略）</param>
        /// <param name="table">表名称</param>
        /// <returns>列描述符列表</returns>
        public async Task<IReadOnlyList<DbColumnDesciptor>> GetColumns(DbConnection connection, string schema, string table)
        {
            var columns = new List<DbColumnDesciptor>();
            var sql = SqlProvider.GetColumns(schema, table);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    columns.Add(new DbColumnDesciptor()
                    {
                        TableName = table,
                        ColumnName = reader.GetString(reader.GetOrdinal("name")),
                        ColumnType = reader.GetString(reader.GetOrdinal("type")),
                        NullableFlag = !reader.GetBoolean(reader.GetOrdinal("notnull")),
                        PrimaryKeyFlag = reader.GetBoolean(reader.GetOrdinal("pk")),
                    });
                }
            });
            return columns;
        }

        /// <summary>
        /// 获取表中的所有索引
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称（SQLite 中忽略）</param>
        /// <param name="table">表名称</param>
        /// <returns>索引描述符列表</returns>
        public async Task<IReadOnlyList<DbIndexDesciptor>> GetIndexes(DbConnection connection, string schema, string table)
        {
            var indexes = new List<DbIndexDesciptor>();
            var sql = SqlProvider.GetIndexes(schema, table);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    indexes.Add(new DbIndexDesciptor()
                    {
                        TableName = table,
                        IndexName = reader.GetString(reader.GetOrdinal("name")),
                        UniqueFlag = reader.GetBoolean(reader.GetOrdinal("unique")),
                    });
                }
            });
            return indexes;
        }

    }
}
