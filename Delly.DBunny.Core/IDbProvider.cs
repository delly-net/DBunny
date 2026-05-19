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
        /// SQL 提供程序
        /// </summary>
        ISqlProvider SqlProvider { get; }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>Schema 名称列表</returns>
        Task<IReadOnlyList<string>> GetSchemasAsync(DbConnection connection);

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称</param>
        /// <returns>Schema 名称，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        Task<string> GetSchemaAsync(DbConnection connection, string schema);
#else
        Task<string?> GetSchemaAsync(DbConnection connection, string schema);
#endif

        /// <summary>
        /// 获取 Schema 中的所有表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称</param>
        /// <returns>表描述符列表</returns>
        Task<IReadOnlyList<DbTableDesciptor>> GetTablesAsync(DbConnection connection, string schema);

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>表描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        Task<DbTableDesciptor> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor);
#else
        Task<DbTableDesciptor?> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor);
#endif

        /// <summary>
        /// 获取表中的所有列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>列描述符列表</returns>
        Task<IReadOnlyList<DbColumnDesciptor>> GetColumnsAsync(DbConnection connection, DbTableDesciptor tableDesciptor);

        /// <summary>
        /// 获取单个列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名称</param>
        /// <returns>列描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        Task<DbColumnDesciptor> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column);
#else
        Task<DbColumnDesciptor?> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column);
#endif

        /// <summary>
        /// 获取表中的所有索引
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>索引描述符列表</returns>
        Task<IReadOnlyList<DbIndexDesciptor>> GetIndexesAsync(DbConnection connection, DbTableDesciptor tableDesciptor);

        /// <summary>
        /// 获取单个索引信息
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>索引描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        Task<DbIndexDesciptor> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor);
#else
        Task<DbIndexDesciptor?> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor);
#endif
    }
}


