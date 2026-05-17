using Delly.DBunny.Providing.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MySqlConnector;
using System.Threading.Tasks;

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
        /// Sql 提供程序
        /// </summary>
        public ISqlProvider SqlProvider { get; } = new MySqlSqlProvider();

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
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
        /// <param name="connection"></param>
        /// <returns></returns>
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
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public DbConnection GetDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        /// <summary>
        /// 设置参数集
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
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
        /// <param name="connection"></param>
        /// <returns></returns>
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
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
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
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
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
                        SchemaName = schema,
                        TableName = table,
                        ColumnName = reader.GetString(reader.GetOrdinal("COLUMN_NAME")),
                        ColumnType = reader.GetString(reader.GetOrdinal("COLUMN_TYPE")),
                        NullableFlag = reader.GetString(reader.GetOrdinal("IS_NULLABLE")) == "YES",
                        PrimaryKeyFlag = reader.GetString(reader.GetOrdinal("COLUMN_KEY")) == "PRI",
                    });
                }
            });
            return columns;
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
            var sql = SqlProvider.GetIndexes(schema, table);
            await this.ReadAsync(connection, sql, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    var indexName = reader.GetString(reader.GetOrdinal("INDEX_NAME"));
                    // Skip PRIMARY key index
                    if (indexName != "PRIMARY")
                    {
                        indexes.Add(new DbIndexDesciptor()
                        {
                            TableName = table,
                            IndexName = indexName,
                            UniqueFlag = reader.GetBoolean(reader.GetOrdinal("NON_UNIQUE")) == false,
                        });
                    }
                }
            });
            return indexes;
        }

    }
}