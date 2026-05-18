using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据库 作业
    /// </summary>
    public interface IDbWork : IDisposable
    {
        /// <summary>
        /// 作业管理器
        /// </summary>
        IDbManager Manager { get; }

        /// <summary>
        /// 提供程序
        /// </summary>
        IDbProvider Provider { get; }

        /// <summary>
        /// 连接描述器
        /// </summary>
        DbConnectionDescriptor ConnectionDescriptor { get; }

        /// <summary>
        /// 过滤器 集合
        /// </summary>
        IEnumerable<IDbFilter> Filters { get; }

        /// <summary>
        /// Sql对象集合
        /// </summary>
        List<Sqled> Sqleds { get; }

        /// <summary>
        /// 根据Sql对象获取一个命令对象
        /// </summary>
        /// <returns></returns>
        DbCommand GetSqlCommand(DbConnection dbc, Sqled sqled);

        /// <summary>
        /// 创建一个连接
        /// </summary>
        /// <returns></returns>
        DbConnection Connect();

        /// <summary>
        /// 异步创建一个连接
        /// </summary>
        /// <returns></returns>
        Task<DbConnection> ConnectAsync();

        /// <summary>
        /// 生效事务
        /// </summary>
        int Commit();

        /// <summary>
        /// 生效事务
        /// </summary>
        Task<int> CommitAsync();
    }
}


