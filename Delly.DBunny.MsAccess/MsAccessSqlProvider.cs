using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using Delly.Modeling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.MsAccess
{
    /// <summary>
    /// Microsoft Access SQL 提供程序
    /// </summary>
    public class MsAccessSqlProvider : ISqlProvider
    {
        /// <summary>
        /// 是否有 数据库 层
        /// </summary>
        public bool HasDatabase => false;

        /// <summary>
        /// 是否有 Schema 层
        /// </summary>
        public bool HasSchema => false;

        /// <summary>
        /// 获取特有名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetSpecialName(string name)
        {
            return "[" + name + "]";
        }

        /// <summary>
        /// 获取数据库特定类型名称（包含自增长标识）
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="typeCode">类型代码</param>
        /// <param name="autoIncrementFlag">自增长标识</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>Access 类型名称</returns>
        public string GetSpecialTypeName(ColumnType columnType, TypeCode typeCode, bool autoIncrementFlag, int length = 0, int precision = 0)
        {
            return GetSpecialTypeName(typeCode, length, precision);
        }

        /// <summary>
        /// 获取特有类型名称
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="length"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return "BIT";
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return "INTEGER";
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "LONG";
                case TypeCode.Single:
                case TypeCode.Double:
                    return "DOUBLE";
                case TypeCode.Decimal:
                    return "CURRENCY";
                case TypeCode.DateTime:
                    return "DATETIME";
                case TypeCode.String:
                    if (length > 0 && length <= 255)
                        return $"VARCHAR({length})";
                    return "LONGTEXT";
                case TypeCode.Char:
                    return "CHAR(1)";
                default:
                    throw new NotSupportedException($"Type code '{typeCode}' not supported.");
            }
        }

        /// <summary>
        /// 获取特有类型名称
        /// </summary>
        /// <param name="columnType"></param>
        /// <param name="length"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(ColumnType columnType, int length = 0, int precision = 0)
        {
            switch (columnType)
            {
                case ColumnType.DECIMAL:
                    return "CURRENCY";
                case ColumnType.BOOL:
                    return "SMALLINT";
                case ColumnType.INTEGER:
                    return "INTEGER";
                case ColumnType.LONG:
                    return "LONG";
                case ColumnType.TIME:
                    return "DATETIME";
                case ColumnType.VARCHAR:
                    if (length > 0 && length <= 255)
                        return $"VARCHAR({length})";
                    return "LONGTEXT";
                case ColumnType.TEXT:
                    return "LONGTEXT";
                default:
                    throw new NotSupportedException($"Column type '{columnType}' not supported.");
            }
        }

        /// <summary>
        /// 获取参数 Sql对象
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>参数 Sql 对象</returns>
        public Sqled GetParamterSqled(string name, object value)
        {
            return $"@{name}".ToSql().Set(name, value);
        }

        /// <summary>
        /// 获取时间参数 Sql对象
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">时间值</param>
        /// <returns>参数 Sql 对象</returns>
        public Sqled GetTimeParamterSqled(string name, object value)
        {
            return GetParamterSqled(name, value);
        }

        /// <summary>
        /// 附加游标
        /// </summary>
        /// <param name="sqled">Sql 对象</param>
        /// <param name="take">取值数量</param>
        /// <param name="skip">跳过数量</param>
        /// <returns>附加游标后的 Sql 对象</returns>
        public Sqled AppendOffset(Sqled sqled, int? take, int? skip)
        {
            if (!take.HasValue) { return sqled; }
            var sb = sqled.Builder;
            sb.Append($" LIMIT {take.Value}");
            return sqled;
        }

        #region 数据库

        /// <summary>
        /// 获取所有 数据库
        /// </summary>
        /// <returns></returns>
        public Sqled GetDatabases()
        {
            throw new NotSupportedException("Access does not support multiple databases. HasDatabase is false.");
        }

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="database"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Sqled CreateDatabase(string database, IDictionary<string, object> options)
        {
            throw new NotSupportedException("Access databases are files, created via ADOX or by creating a new .accdb/.mdb file. HasDatabase is false.");
        }

        /// <summary>
        /// 删除 数据库
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public Sqled DropDatabase(string database)
        {
            throw new NotSupportedException("Access databases are files, deleted by deleting the .accdb/.mdb file. HasDatabase is false.");
        }

        #endregion

        #region Schema

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns></returns>
        public Sqled GetSchemas()
        {
            throw new NotSupportedException("Access does not support schemas. HasSchema is false.");
        }

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        public Sqled GetSchemas(string schema)
        {
            throw new NotSupportedException("Access does not support schemas. HasSchema is false.");
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Sqled CreateSchema(string schema, IDictionary<string, object> options)
        {
            throw new NotSupportedException("Access does not support schemas. HasSchema is false.");
        }

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Sqled DropSchema(string schema)
        {
            throw new NotSupportedException("Access does not support schemas. HasSchema is false.");
        }

        #endregion

        #region 数据表

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Sqled GetTables(string schema)
        {
            return "SELECT name FROM MSysObjects WHERE type = 1 AND flags = 0 ORDER BY name";
        }

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTable(DbTableDesciptor tableDesciptor)
        {
            return $"SELECT name FROM MSysObjects WHERE name = '{tableDesciptor.TableName}' AND type = 1 AND flags = 0";
        }

        /// <summary>
        /// 获取创建表时的字段定义
        /// </summary>
        /// <param name="columnDesciptor">列描述符</param>
        /// <returns>字段定义 SQL</returns>
        public Sqled CreateTableColumnDefine(DbColumnDesciptor columnDesciptor)
        {
            var column = columnDesciptor.ColumnName;
            var columnType = columnDesciptor.ColumnType;
            var primaryKey = columnDesciptor.PrimaryKeyFlag;
            var nullable = columnDesciptor.NullableFlag;

            var sb = new StringBuilder();
            sb.Append(GetSpecialName(column));
            sb.Append(" ");

            if (primaryKey && columnType.Equals("INTEGER", StringComparison.OrdinalIgnoreCase))
            {
                // Access 使用 COUNTER 作为自增主键
                sb.Append("COUNTER");
            }
            else
            {
                sb.Append(columnType);
            }

            if (primaryKey && !columnType.Equals("INTEGER", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(" NOT NULL PRIMARY KEY");
            }
            else if (!nullable)
            {
                sb.Append(" NOT NULL");
            }
            else if (nullable)
            {
                sb.Append(" NULL");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="columnDesciptors"></param>
        /// <returns></returns>
        public Sqled CreateTable(DbTableDesciptor tableDesciptor, IList<DbColumnDesciptor> columnDesciptors)
        {
            var table = tableDesciptor.TableName;
            var sql = new Sqled();
            sql.Builder.Append($"CREATE TABLE {GetSpecialName(table)}(");
            for (int i = 0; i < columnDesciptors.Count; i++)
            {
                if (i > 0)
                {
                    sql.Builder.Append(", ");
                }
                var column = columnDesciptors[i];
                var columnDefine = CreateTableColumnDefine(column);
                sql.Builder.Append(columnDefine.Sql);
            }
            sql.Builder.AppendLine(")");
            return sql;
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns></returns>
        public Sqled DropTable(DbTableDesciptor tableDesciptor)
        {
            return $"DROP TABLE {GetSpecialName(tableDesciptor.TableName)};";
        }

        #endregion

        #region 数据列

        /// <summary>
        /// 获取表中所有列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns></returns>
        public Sqled GetColumns(DbTableDesciptor tableDesciptor)
        {
            var table = tableDesciptor.TableName;
            return $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_FLAGS FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}'";
        }

        /// <summary>
        /// 获取单个列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名称</param>
        /// <returns>获取列的 SQL 命令</returns>
        public Sqled GetColumn(DbTableDesciptor tableDesciptor, string column)
        {
            var table = tableDesciptor.TableName;
            return $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_FLAGS FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}' AND COLUMN_NAME = '{column}'";
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnDesciptor"></param>
        /// <returns></returns>
        public Sqled CreateColumn(DbColumnDesciptor columnDesciptor)
        {
            var table = columnDesciptor.TableName;
            var column = columnDesciptor.ColumnName;
            var columnType = columnDesciptor.ColumnType;
            var nullable = columnDesciptor.NullableFlag;
            var primaryKey = columnDesciptor.PrimaryKeyFlag;

            var sql = new StringBuilder();
            sql.Append("ALTER TABLE ");
            sql.Append(GetSpecialName(table));
            sql.Append(" ADD COLUMN ");
            sql.Append(GetSpecialName(column));
            sql.Append(" ");
            sql.Append(columnType);

            if (primaryKey)
            {
                sql.Append(" NOT NULL PRIMARY KEY AUTOINCREMENT");
            }
            else if (!nullable)
            {
                sql.Append(" NOT NULL");
            }
            else
            {
                sql.Append(" NULL");
            }

            sql.Append(";");
            return sql.ToString();
        }

        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">新列名</param>
        /// <returns></returns>
        public Sqled RenameColumn(DbTableDesciptor tableDesciptor, string column, string columnTarget)
        {
            throw new NotSupportedException("Access does not support renaming columns directly. Use DROP COLUMN and ADD COLUMN instead.");
        }

        /// <summary>
        /// 修改列
        /// </summary>
        /// <param name="column"></param>
        /// <param name="columnTarget"></param>
        /// <returns></returns>
        public Sqled ModifyColumn(DbColumnDesciptor column, DbColumnDesciptor columnTarget)
        {
            throw new NotSupportedException("Access does not support modifying column types directly. Create a new table with the new schema and copy data instead.");
        }

        /// <summary>
        /// 复制列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">目标列名</param>
        /// <param name="columnType">列类型</param>
        /// <returns></returns>
        public Sqled CopyColumn(DbTableDesciptor tableDesciptor, string column, string columnTarget, string columnType)
        {
            var table = tableDesciptor.TableName;
            return $"UPDATE {GetSpecialName(table)} SET {GetSpecialName(columnTarget)} = {GetSpecialName(column)};";
        }

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名</param>
        /// <returns></returns>
        public Sqled DropColumn(DbTableDesciptor tableDesciptor, string column)
        {
            var table = tableDesciptor.TableName;
            return $"ALTER TABLE {GetSpecialName(table)} DROP COLUMN {GetSpecialName(column)};";
        }

        #endregion

        #region 索引

        /// <summary>
        /// 获取表的所有索引
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns></returns>
        public Sqled GetIndexes(DbTableDesciptor tableDesciptor)
        {
            var table = tableDesciptor.TableName;
            return $"SELECT IndexName, Name, Position FROM MSysIndexColumns INNER JOIN MSysIndexes ON MSysIndexColumns.Id = MSysIndexes.Id WHERE MSysIndexColumns.ObjectId = (SELECT id FROM MSysObjects WHERE name = '{table}' AND type = 1)";
        }

        /// <summary>
        /// 获取单个索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>获取索引的 SQL 命令</returns>
        public Sqled GetIndex(DbIndexDesciptor indexDesciptor)
        {
            var indexName = indexDesciptor.IndexName;
            var table = indexDesciptor.TableName;
            return $"SELECT IndexName, Name, Position FROM MSysIndexColumns INNER JOIN MSysIndexes ON MSysIndexColumns.Id = MSysIndexes.Id WHERE MSysIndexColumns.ObjectId = (SELECT id FROM MSysObjects WHERE name = '{table}' AND type = 1) AND IndexName = '{indexName}'";
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="indexDesciptor"></param>
        /// <returns></returns>
        public Sqled CreateIndex(DbIndexDesciptor indexDesciptor)
        {
            var table = indexDesciptor.TableName;
            var column = indexDesciptor.ColumnName;
            var unique = indexDesciptor.UniqueFlag;

            var tableName = GetSpecialName(table);
            var columnName = GetSpecialName(column);

            if (unique)
            {
                return $"CREATE UNIQUE INDEX {table}_{column}_IDX ON {tableName} ({columnName});";
            }
            return $"CREATE INDEX {table}_{column}_IDX ON {tableName} ({columnName});";
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns></returns>
        public Sqled DropIndex(DbIndexDesciptor indexDesciptor)
        {
            var indexName = indexDesciptor.IndexName;
            if (string.IsNullOrEmpty(indexName))
            {
                indexName = $"{indexDesciptor.TableName}_{indexDesciptor.ColumnName}_IDX";
            }
            return $"DROP INDEX {indexName} ON {GetSpecialName(indexDesciptor.TableName)};";
        }

        #endregion
    }
}