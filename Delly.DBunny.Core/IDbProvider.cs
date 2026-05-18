using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny.Core
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
        /// <param name="connectionString">连接字符串</param>
        /// <returns>数据库连接</returns>
        DbConnection GetDbConnection(string connectionString);

        /// <summary>
        /// 获取数据库命令管理器
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>数据库命令</returns>
        DbCommand GetDbCommand(DbConnection connection);

        /// <summary>
        /// 设置参数集
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <param name="parameters">参数集合</param>
        void SetParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> parameters);

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <returns>数据集</returns>
        DataSet GetDataSet(DbCommand command);

        /// <summary>
        /// Sql提供程序
        /// </summary>
        ISqlProvider SqlProvider { get; }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>Schema 名称列表</returns>
        Task<IReadOnlyList<string>> GetSchemas(DbConnection connection);

        /// <summary>
        /// 获取 Schema 中的所有表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称</param>
        /// <returns>表描述符列表</returns>
        Task<IReadOnlyList<DbTableDesciptor>> GetTables(DbConnection connection, string schema);

        /// <summary>
        /// 获取表中的所有列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <returns>列描述符列表</returns>
        Task<IReadOnlyList<DbColumnDesciptor>> GetColumns(DbConnection connection, string schema, string table);

        /// <summary>
        /// 获取表中的所有索引
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <returns>索引描述符列表</returns>
        Task<IReadOnlyList<DbIndexDesciptor>> GetIndexes(DbConnection connection, string schema, string table);
    }
}


