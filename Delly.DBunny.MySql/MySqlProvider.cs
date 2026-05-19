using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MySqlConnector;
using System.Threading.Tasks;
using Delly.DBunny.Core.Providing.Extension;
using Delly.DBunny.Core;

namespace Delly.DBunny.MySql
{
    /// <summary>
    /// MySql 提供程序
    /// </summary>
    public sealed class MySqlProvider : IDbProvider
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType => MySqlConnectionDefine.DATABASE_TYPE;

        /// <summary>
        /// SQL 提供程序
        /// </summary>
        public ISqlProvider SqlProvider { get; } = new MySqlSqlProvider();

        /// <summary>
        /// 获取 DataSet
        /// </summary>
        /// <param name="command">数据库命令</param>
        /// <returns>数据集</returns>
        public DataSet GetDataSet(DbCommand command)
        {
            var sqlDataAdapter = new MySqlDataAdapter { SelectCommand = (MySqlCommand)command };
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
            var sqlCommand = new MySqlCommand
            {
                CommandTimeout = 600,
                Connection = (MySqlConnection)connection
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
            return new MySqlConnection(connectionString);
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
                command.Parameters.Add(new MySqlParameter("@" + param.Key, param.Value));
            }
        }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>Schema 名称列表</returns>
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
        /// <exception cref="NotSupportedException"></exception>
#if NETSTANDARD2_0
        public Task<string> GetSchemaAsync(DbConnection connection, string schema)
#else
        public Task<string?> GetSchemaAsync(DbConnection connection, string schema)
#endif
        {
            throw new NotSupportedException("MySQL does not support separate schemas. Database and Schema are the same concept. HasSchema is false.");
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="schema">Schema 名称（MySQL 中即数据库名）</param>
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
        /// <returns>表描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
        /// <exception cref="NotImplementedException"></exception>
#if NETSTANDARD2_0
        public Task<DbTableDesciptor> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
#else
        public Task<DbTableDesciptor?> GetTableAsync(DbConnection connection, DbTableDesciptor tableDesciptor)
#endif
        {
            throw new NotImplementedException();
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
                    columns.Add(new DbColumnDesciptor()
                    {
                        SchemaName = tableDesciptor.SchemaName,
                        TableName = tableDesciptor.TableName,
                        ColumnName = reader.GetString(reader.GetOrdinal("Field")),
                        ColumnType = reader.GetString(reader.GetOrdinal("Type")).ToUpperInvariant(),
                        NullableFlag = reader.GetString(reader.GetOrdinal("Null")) == "YES",
                        PrimaryKeyFlag = reader.GetString(reader.GetOrdinal("Key")) == "PRI",
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
        /// <returns>列描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
        /// <exception cref="NotImplementedException"></exception>
#if NETSTANDARD2_0
        public Task<DbColumnDesciptor> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column)
#else
        public Task<DbColumnDesciptor?> GetColumnAsync(DbConnection connection, DbTableDesciptor tableDesciptor, string column)
#endif
        {
            throw new NotImplementedException();
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
                    var indexName = reader.GetString(reader.GetOrdinal("Key_name"));
                    // Skip PRIMARY key index
                    if (indexName != "PRIMARY")
                    {
                        indexes.Add(new DbIndexDesciptor()
                        {
                            SchemaName = tableDesciptor.SchemaName,
                            TableName = tableDesciptor.TableName,
                            IndexName = indexName,
                            UniqueFlag = reader.GetBoolean(reader.GetOrdinal("Non_unique")) == false,
                            ColumnName = reader.GetString(reader.GetOrdinal("Column_name")),
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
        /// <returns>索引描述符，若不存在则返回 null (仅 .NET 5.0+)</returns>
        /// <exception cref="NotImplementedException"></exception>
#if NETSTANDARD2_0
        public Task<DbIndexDesciptor> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor)
#else
        public Task<DbIndexDesciptor?> GetIndexeAsync(DbConnection connection, DbIndexDesciptor indexDesciptor)
#endif
        {
            throw new NotImplementedException();
        }

    }
}