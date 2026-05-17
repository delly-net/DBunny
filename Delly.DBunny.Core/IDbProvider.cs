using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据库提供者
    /// </summary>
    public interface IDbProvider
    {

        /// <summary>
        /// 数据库类型
        /// </summary>
        string DatabaseType { get; }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        DbConnection GetDbConnection(string connectionString);

        /// <summary>
        /// 获取数据库命令管理器
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        DbCommand GetDbCommand(DbConnection connection);

        /// <summary>
        /// 设置参数集
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        void SetParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> parameters);

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        DataSet GetDataSet(DbCommand command);

        /// <summary>
        /// Sql提供程序
        /// </summary>
        ISqlProvider SqlProvider { get; }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<string>> GetSchemas(DbConnection connection);

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<DbTableDesciptor>> GetTables(DbConnection connection, string schema);

        /// <summary>
        /// 获取表中的所有列
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<DbColumnDesciptor>> GetColumns(DbConnection connection, string schema, string table);

        /// <summary>
        /// 获取表中的所有索引
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<DbIndexDesciptor>> GetIndexes(DbConnection connection, string schema, string table);
    }
}


