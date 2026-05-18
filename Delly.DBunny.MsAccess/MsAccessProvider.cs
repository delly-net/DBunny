using Delly.DBunny.Providing.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace Delly.DBunny.MsAccess
{
    /// <summary>
    /// Microsoft Access 提供程序
    /// </summary>
    public sealed class MsAccessProvider : IDbProvider
    {
        private static readonly Dictionary<int, string> OleDbTypeMap = new Dictionary<int, string>()
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
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<IReadOnlyList<string>> GetSchemas(DbConnection connection)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DbTableDesciptor>> GetTables(DbConnection connection, string schema)
        {
            var tables = new List<DbTableDesciptor>();

            // 使用 GetSchema 获取表列表，避免 MSysObjects 权限问题
            var oleDbConnection = (OleDbConnection)connection;
            var dataTable = oleDbConnection.GetSchema("Tables");

            foreach (DataRow row in dataTable.Rows)
            {
                var tableType = row["TABLE_TYPE"].ToString();
                var tableName = row["TABLE_NAME"].ToString();

                // 只返回 TABLE 类型的表，跳过系统表和视图
                if (tableType.Equals("TABLE", StringComparison.OrdinalIgnoreCase) &&
                    !tableName.StartsWith("MSys", StringComparison.OrdinalIgnoreCase) &&
                    !tableName.StartsWith("~", StringComparison.OrdinalIgnoreCase))
                {
                    tables.Add(new DbTableDesciptor
                    {
                        TableName = tableName
                    });
                }
            }

            return await Task.FromResult<IReadOnlyList<DbTableDesciptor>>(tables);
        }

        /// <summary>
        /// 获取表中的所有列
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DbColumnDesciptor>> GetColumns(DbConnection connection, string schema, string table)
        {
            var columns = new List<DbColumnDesciptor>();

            // 使用 GetSchema 获取列信息
            var oleDbConnection = (OleDbConnection)connection;
            string[] restrictions = new string[] { null, null, table };
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
                    OleDbTypeMap.TryGetValue(dataType, out columnType);
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

            return await Task.FromResult<IReadOnlyList<DbColumnDesciptor>>(columns);
        }

        /// <summary>
        /// 获取表中的所有索引
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DbIndexDesciptor>> GetIndexes(DbConnection connection, string schema, string table)
        {
            var indexes = new List<DbIndexDesciptor>();

            // 使用 GetSchema 获取索引信息
            var oleDbConnection = (OleDbConnection)connection;
#if NETSTANDARD2_0
            string[] restrictions = new string[] { null, null, null, null, table };
#else
            var restrictions = new string?[] { null, null, null, null, table };
#endif
            var dataTable = oleDbConnection.GetSchema("Indexes", restrictions);

            foreach (DataRow row in dataTable.Rows)
            {
                var indexName = row["INDEX_NAME"]?.ToString() ?? string.Empty;

                indexes.Add(new DbIndexDesciptor
                {
                    TableName = table,
                    IndexName = indexName,
                    UniqueFlag = row["UNIQUE"] != null && row["UNIQUE"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase)
                });
            }

            return await Task.FromResult<IReadOnlyList<DbIndexDesciptor>>(indexes);
        }
    }
}