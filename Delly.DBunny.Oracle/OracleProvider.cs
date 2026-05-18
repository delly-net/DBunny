using Delly.DBunny.Providing.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;

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
        /// Sql 提供程序
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
        public async Task<IReadOnlyList<string>> GetSchemas(DbConnection connection)
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
        /// 获取所有表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称（Oracle 中即用户名）</param>
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
                        SchemaName = schema,
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
        /// <param name="schema">Schema 名称（Oracle 中即用户名）</param>
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
                    var columnName = reader.IsDBNull(reader.GetOrdinal("column_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("column_name"));
                    var dataType = reader.IsDBNull(reader.GetOrdinal("data_type")) ? string.Empty : reader.GetString(reader.GetOrdinal("data_type"));
                    var nullable = reader.IsDBNull(reader.GetOrdinal("nullable")) ? "N" : reader.GetString(reader.GetOrdinal("nullable"));
                    var columnKey = reader.IsDBNull(reader.GetOrdinal("column_key")) ? string.Empty : reader.GetString(reader.GetOrdinal("column_key"));

                    columns.Add(new DbColumnDesciptor()
                    {
                        SchemaName = schema,
                        TableName = table,
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
        /// 获取表中的所有索引
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称（Oracle 中即用户名）</param>
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
                    var indexName = reader.GetString(reader.GetOrdinal("index_name"));
                    // Skip PRIMARY key index
                    if (!indexName.StartsWith("SYS_C", StringComparison.OrdinalIgnoreCase) &&
                        !indexName.Equals(table.ToUpper() + "_PK", StringComparison.OrdinalIgnoreCase))
                    {
                        indexes.Add(new DbIndexDesciptor()
                        {
                            SchemaName = schema,
                            TableName = table,
                            IndexName = indexName,
                            UniqueFlag = reader.GetString(reader.GetOrdinal("uniqueness")) == "UNIQUE",
                            ColumnName = reader.GetString(reader.GetOrdinal("column_name")),
                        });
                    }
                }
            });
            return indexes;
        }
    }
}