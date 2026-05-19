using Delly.DBunny.Core;
using Delly.DBunny.Core.Providing.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
#if !NETSTANDARD2_0
using System.Runtime.Versioning;
#endif
using System.Threading.Tasks;

namespace Delly.DBunny.MsAccess
{
    /// <summary>
    /// Microsoft Access 提供程序
    /// </summary>
#if !NETSTANDARD2_0
    [SupportedOSPlatform("windows")]
#endif
    public sealed class MsAccessProvider : IDbProvider
    {
        private static readonly Dictionary<int, string> _oleDbTypeMap = new Dictionary<int, string>()
        {
            { 2, "SMALLINT" },   // SmallInt
            { 3, "INTEGER" },    // Integer
            { 4, "SINGLE" },     // Single
            { 5, "DOUBLE" },     // Double
            { 6, "CURRENCY" },   // Currency
            { 7, "DATETIME" },   // Date
            { 11, "BIT" },       // Boolean
            { 14, "DECIMAL" },   // Decimal
            { 16, "TINYINT" },   // TinyInt
            { 17, "UNSIGNEDTINYINT" }, // UnsignedTinyInt
            { 18, "BIGINT" },    // BigInt
            { 19, "UNSIGNEDSMALLINT" }, // UnsignedSmallInt
            { 20, "UNSIGNEDINT" }, // UnsignedInt
            { 21, "UNSIGNEDBIGINT" }, // UnsignedBigInt
            { 72, "GUID" },      // Guid
            { 128, "BINARY" },   // Binary
            { 129, "CHAR" },     // Char
            { 130, "WCHAR" },    // WChar
            { 131, "NUMERIC" },  // Numeric
            { 133, "USERDEFINED" }, // UserDefined
            { 134, "VARBINARY" }, // VarBinary
            { 135, "VARCHAR" },  // VarChar
            { 139, "ROWVERSION" }, // RowVersion
            { 200, "NVARCHAR" }, // VarWChar
            { 201, "NTEXT" },    // LongVarWChar
            { 202, "VARBINARY" }, // VarBinary
            { 203, "IMAGE" },    // LongVarBinary
            { 204, "TEXT" },     // LongVarChar
        };

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType => MsAccessConnectionDefine.DATABASE_TYPE;

        /// <summary>
        /// Sql 提供程序
        /// </summary>
        public ISqlProvider SqlProvider { get; } = new MsAccessSqlProvider();

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public DataSet GetDataSet(DbCommand command)
        {
            var oleDbDataAdapter = new OleDbDataAdapter { SelectCommand = (OleDbCommand)command };
            var dataSet = new DataSet();
            oleDbDataAdapter.Fill(dataSet);
            return dataSet;
        }

        /// <summary>
        /// 获取数据库命令管理器
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public DbCommand GetDbCommand(DbConnection connection)
        {
            var sqlCommand = new OleDbCommand
            {
                CommandTimeout = 600,
                Connection = (OleDbConnection)connection
            };
            return sqlCommand;
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public DbConnection GetDbConnection(string connectionString)
        {
            return new OleDbConnection(connectionString);
        }

        /// <summary>
        /// 设置参数集
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public void SetParameters(DbCommand command, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            command.Parameters.Clear();
            int paramIndex = 0;
            foreach (var param in parameters)
            {
                // OleDb 使用位置参数 (?) 而不是命名参数
                var oleDbParam = new OleDbParameter("?" + paramIndex, param.Value ?? DBNull.Value);
                command.Parameters.Add(oleDbParam);
                paramIndex++;
            }
        }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>Schema 名称列表</returns>
        public Task<IReadOnlyList<string>> GetSchemasAsync(DbConnection connection)
        {
            return Task.FromResult<IReadOnlyList<string>>(new List<string> { "" });
        }

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称</param>
        /// <returns>Schema 名称，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        public Task<string> GetSchemaAsync(DbConnection connection, string schema)
#else
        public Task<string?> GetSchemaAsync(DbConnection connection, string schema)
#endif
        {
            return Task.FromResult(string.IsNullOrEmpty(schema) ? schema : null);
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DbTableDesciptor>> GetTablesAsync(DbConnection connection, string schema)
        {
            var tables = new List<DbTableDesciptor>();

            // 使用 GetSchema 获取表列表，避免 MSysObjects 权限问题
            var oleDbConnection = (OleDbConnection)connection;
            var dataTable = oleDbConnection.GetSchema("Tables");

            foreach (DataRow row in dataTable.Rows)
            {
                var tableType = row["TABLE_TYPE"]?.ToString();
                var tableName = row["TABLE_NAME"]?.ToString();

                // 只返回 TABLE 类型的表，跳过系统表和视图
                if (tableType != null && tableType.Equals("TABLE", StringComparison.OrdinalIgnoreCase) &&
                    tableName != null && !tableName.StartsWith("MSys", StringComparison.OrdinalIgnoreCase) &&
                    !tableName.StartsWith("~", StringComparison.OrdinalIgnoreCase))
                {
                    tables.Add(new DbTableDesciptor
                    {
                        TableName = tableName
                    });
                }
            }

            return tables;
        }

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>表描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        public async Task<DbTableDesciptor> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
#else
        public async Task<DbTableDesciptor?> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
#endif
        {
            var oleDbConnection = (OleDbConnection)connection;
            var dataTable = oleDbConnection.GetSchema("Tables");

            foreach (DataRow row in dataTable.Rows)
            {
                var tableName = row["TABLE_NAME"]?.ToString();
                if (tableName == tableDesciptor.TableName)
                {
                    return tableDesciptor;
                }
            }

#if NETSTANDARD2_0
            return null;
#else
            return await Task.FromResult<DbTableDesciptor?>(null);
#endif
        }

        /// <summary>
        /// 获取表中的所有列
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DbColumnDesciptor>> GetColumnsAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
        {
            var columns = new List<DbColumnDesciptor>();
            var table = tableDesciptor.TableName;

            // 使用 GetSchema 获取列信息
            var oleDbConnection = (OleDbConnection)connection;
#if NETSTANDARD2_0
            string[] restrictions = new string[] { null, null, table };
#else
            string?[] restrictions = new string?[] { null, null, table };
#endif
            var dataTable = oleDbConnection.GetSchema("Columns", restrictions);

            foreach (DataRow row in dataTable.Rows)
            {
                // DATA_TYPE 返回的是整数类型代码，需要映射为类型名称
#if NETSTANDARD2_0
                string columnType = "UNKNOWN";
#else
                string? columnType = "UNKNOWN";
#endif
                if (row["DATA_TYPE"] != null && int.TryParse(row["DATA_TYPE"].ToString(), out int dataType))
                {
                    _oleDbTypeMap.TryGetValue(dataType, out columnType);
                    if (columnType is null) { columnType = "UNKNOWN"; }
                }

                columns.Add(new DbColumnDesciptor
                {
                    TableName = table,
                    ColumnName = row["COLUMN_NAME"]?.ToString() ?? string.Empty,
                    ColumnType = columnType,
                    NullableFlag = row["IS_NULLABLE"] != null &&
                                   ($"{row["IS_NULLABLE"]}".Equals("YES", StringComparison.OrdinalIgnoreCase) ||
                                    $"{row["IS_NULLABLE"]}".Equals("true", StringComparison.OrdinalIgnoreCase)),
                    PrimaryKeyFlag = false // 需要额外查询主键信息
                });
            }

            return columns;
        }

        /// <summary>
        /// 获取单个列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名称</param>
        /// <returns>列描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        public async Task<DbColumnDesciptor> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column)
#else
        public async Task<DbColumnDesciptor?> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column)
#endif
        {
            var columns = await GetColumnsAsync(connection, tableDesciptor);
            var foundColumn = columns.FirstOrDefault(c => c.ColumnName == column);
#if NETSTANDARD2_0
            return foundColumn;
#else
            return await Task.FromResult(foundColumn);
#endif
        }

        /// <summary>
        /// 获取表中的所有索引
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DbIndexDesciptor>> GetIndexesAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
        {
            var indexes = new List<DbIndexDesciptor>();
            var table = tableDesciptor.TableName;

            // 使用 GetSchema 获取索引信息
            var oleDbConnection = (OleDbConnection)connection;
#if NETSTANDARD2_0
            string[] restrictions = new string[] { null, null, null, null, table };
#else
            string?[] restrictions = new string?[] { null, null, null, null, table };
#endif
            var dataTable = oleDbConnection.GetSchema("Indexes", restrictions);

            foreach (DataRow row in dataTable.Rows)
            {
                var indexName = row["INDEX_NAME"]?.ToString() ?? string.Empty;

                indexes.Add(new DbIndexDesciptor
                {
                    TableName = table,
                    IndexName = indexName,
                    UniqueFlag = row["UNIQUE"]?.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true
                });
            }

            return indexes;
        }

        /// <summary>
        /// 获取单个索引信息
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>索引描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        public async Task<DbIndexDesciptor> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor)
#else
        public async Task<DbIndexDesciptor?> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor)
#endif
        {
            var indexes = await GetIndexesAsync(connection, indexDesciptor);
            var foundIndex = indexes.FirstOrDefault(i => i.IndexName == indexDesciptor.IndexName);
#if NETSTANDARD2_0
            return foundIndex;
#else
            return await Task.FromResult(foundIndex);
#endif
        }
    }
}