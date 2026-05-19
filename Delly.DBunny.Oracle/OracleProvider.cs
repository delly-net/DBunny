using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using Delly.DBunny.Core.Providing.Extension;
using Delly.DBunny.Core;

namespace Delly.DBunny.Oracle
{
    /// <summary>
    /// Oracle 提供程序
    /// </summary>
    public sealed class OracleProvider : IDbProvider
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType => OracleConnectionDefine.DATABASE_TYPE;

        /// <summary>
        /// SQL 提供程序
        /// </summary>
        public ISqlProvider SqlProvider { get; } = new OracleSqlProvider();

        /// <summary>
        /// 获取 DataSet
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <returns>数据集</returns>
        public DataSet GetDataSet(DbCommand command)
        {
            var sqlDataAdapter = new OracleDataAdapter { SelectCommand = (OracleCommand)command };
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
            var sqlCommand = new OracleCommand
            {
                CommandTimeout = 600,
                Connection = (OracleConnection)connection
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
            return new OracleConnection(connectionString);
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
                command.Parameters.Add(new OracleParameter(":" + param.Key, param.Value));
            }
        }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>Schema 名称列表（Oracle 中即用户名）</returns>
        public async Task<IReadOnlyList<string>> GetSchemasAsync(DbConnection connection)
        {
            var schemas = new List<string>();
            var sql = SqlProvider.GetSchemas();
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    schemas.Add(reader.GetString(0));
                }
            });
            return schemas;
        }

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称</param>
        /// <returns>Schema 名称，若不存在则返回 null (仅 .NET 5.0+)</returns>
#if NETSTANDARD2_0
        public async Task<string> GetSchemaAsync(DbConnection connection, string schema)
#else
        public async Task<string?> GetSchemaAsync(DbConnection connection, string schema)
#endif
        {
            var schemas = new List<string>();
            var sql = SqlProvider.GetSchemas(schema);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    schemas.Add(reader.GetString(0));
                }
            });
            return schemas.Count > 0 ? schemas[0] : null;
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称（Oracle 中即用户名）</param>
        /// <returns>表描述符列表</returns>
        public async Task<IReadOnlyList<DbTableDesciptor>> GetTablesAsync(DbConnection connection, string schema)
        {
            var tables = new List<DbTableDesciptor>();
            var sql = SqlProvider.GetTables(schema);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(new DbTableDesciptor()
                    {
                        SchemaName = schema,
                        TableName = reader.GetString(0)
                    });
                }
            });
            return tables;
        }

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>表描述符，若不存在则返回 null</returns>
#if NETSTANDARD2_0
        public async Task<DbTableDesciptor> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
#else
        public async Task<DbTableDesciptor?> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
#endif
        {
            var tables = new List<DbTableDesciptor>();
            var sql = SqlProvider.GetTable(tableDesciptor);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(new DbTableDesciptor()
                    {
                        SchemaName = tableDesciptor.SchemaName,
                        TableName = reader.GetString(0)
                    });
                }
            });
            return tables.Count > 0 ? tables[0] : null;
        }

        /// <summary>
        /// 获取表中的所有列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>列描述符列表</returns>
        public async Task<IReadOnlyList<DbColumnDesciptor>> GetColumnsAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
        {
            var columns = new List<DbColumnDesciptor>();
            var sql = SqlProvider.GetColumns(tableDesciptor);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    var columnName = reader.IsDBNull(reader.GetOrdinal("column_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("column_name"));
                    var dataType = reader.IsDBNull(reader.GetOrdinal("data_type")) ? string.Empty : reader.GetString(reader.GetOrdinal("data_type"));
                    var nullable = reader.IsDBNull(reader.GetOrdinal("nullable")) ? "N" : reader.GetString(reader.GetOrdinal("nullable"));
                    var columnKey = reader.IsDBNull(reader.GetOrdinal("column_key")) ? string.Empty : reader.GetString(reader.GetOrdinal("column_key"));

                    columns.Add(new DbColumnDesciptor()
                    {
                        SchemaName = tableDesciptor.SchemaName,
                        TableName = tableDesciptor.TableName,
                        ColumnName = columnName,
                        ColumnType = dataType,
                        NullableFlag = nullable == "Y",
                        PrimaryKeyFlag = columnKey == "PRI",
                    });
                }
            });
            return columns;
        }

        /// <summary>
        /// 获取单个列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名称</param>
        /// <returns>列描述符，若不存在则返回 null</returns>
#if NETSTANDARD2_0
        public async Task<DbColumnDesciptor> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column)
#else
        public async Task<DbColumnDesciptor?> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column)
#endif
        {
            var columns = new List<DbColumnDesciptor>();
            var sql = SqlProvider.GetColumn(tableDesciptor, column);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    var columnName = reader.IsDBNull(reader.GetOrdinal("column_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("column_name"));
                    var dataType = reader.IsDBNull(reader.GetOrdinal("data_type")) ? string.Empty : reader.GetString(reader.GetOrdinal("data_type"));
                    var nullable = reader.IsDBNull(reader.GetOrdinal("nullable")) ? "N" : reader.GetString(reader.GetOrdinal("nullable"));
                    var columnKey = reader.IsDBNull(reader.GetOrdinal("column_key")) ? string.Empty : reader.GetString(reader.GetOrdinal("column_key"));

                    columns.Add(new DbColumnDesciptor()
                    {
                        SchemaName = tableDesciptor.SchemaName,
                        TableName = tableDesciptor.TableName,
                        ColumnName = columnName,
                        ColumnType = dataType,
                        NullableFlag = nullable == "Y",
                        PrimaryKeyFlag = columnKey == "PRI",
                    });
                }
            });
            return columns.Count > 0 ? columns[0] : null;
        }

        /// <summary>
        /// 获取表中的所有索引
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>索引描述符列表</returns>
        public async Task<IReadOnlyList<DbIndexDesciptor>> GetIndexesAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
        {
            var indexes = new List<DbIndexDesciptor>();
            var sql = SqlProvider.GetIndexes(tableDesciptor);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    var indexName = reader.GetString(reader.GetOrdinal("index_name"));
                    // Skip PRIMARY key index
                    if (!indexName.StartsWith("SYS_C", StringComparison.OrdinalIgnoreCase) &&
                        !indexName.Equals(tableDesciptor.TableName.ToUpper() + "_PK", StringComparison.OrdinalIgnoreCase))
                    {
                        indexes.Add(new DbIndexDesciptor()
                        {
                            SchemaName = tableDesciptor.SchemaName,
                            TableName = tableDesciptor.TableName,
                            IndexName = indexName,
                            UniqueFlag = reader.GetString(reader.GetOrdinal("uniqueness")) == "UNIQUE",
                            ColumnName = reader.GetString(reader.GetOrdinal("column_name")),
                        });
                    }
                }
            });
            return indexes;
        }

        /// <summary>
        /// 获取单个索引信息
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>索引描述符，若不存在则返回 null</returns>
#if NETSTANDARD2_0
        public async Task<DbIndexDesciptor> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor)
#else
        public async Task<DbIndexDesciptor?> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor)
#endif
        {
            var indexes = new List<DbIndexDesciptor>();
            var sql = SqlProvider.GetIndex(indexDesciptor);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    var indexName = reader.GetString(reader.GetOrdinal("index_name"));
                    if (indexName == indexDesciptor.IndexName)
                    {
                        indexes.Add(new DbIndexDesciptor()
                        {
                            SchemaName = indexDesciptor.SchemaName,
                            TableName = indexDesciptor.TableName,
                            IndexName = indexName,
                            UniqueFlag = reader.GetString(reader.GetOrdinal("uniqueness")) == "UNIQUE",
                            ColumnName = reader.GetString(reader.GetOrdinal("column_name")),
                        });
                    }
                }
            });
            return indexes.Count > 0 ? indexes[0] : null;
        }
    }
}