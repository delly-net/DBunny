using Delly.DBunny.Sql.Extension;
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
        public string GetSpecialTypeName(DbColumnType columnType, int length = 0, int precision = 0)
        {
            switch (columnType)
            {
                case DbColumnType.DECIMAL:
                    return "CURRENCY";
                case DbColumnType.TINY:
                    return "SMALLINT";
                case DbColumnType.INTEGER:
                    return "INTEGER";
                case DbColumnType.LONG:
                    return "LONG";
                case DbColumnType.TIME:
                    return "DATETIME";
                case DbColumnType.VARCHAR:
                    if (length > 0 && length <= 255)
                        return $"VARCHAR({length})";
                    return "LONGTEXT";
                case DbColumnType.TEXT:
                    return "LONGTEXT";
                default:
                    throw new NotSupportedException($"Column type '{columnType}' not supported.");
            }
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
        /// 获取创建表时的字段定义
        /// </summary>
        /// <param name="column"></param>
        /// <param name="columnType"></param>
        /// <param name="primaryKey"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public Sqled CreateTableColumnDefine(string column, string columnType, bool primaryKey, bool nullable)
        {
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
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="columnDesciptors"></param>
        /// <returns></returns>
        public Sqled CreateTable(string schema, string table, IList<DbColumnDesciptor> columnDesciptors)
        {
            var sql = new Sqled();
            sql.Builder.Append($"CREATE TABLE {GetSpecialName(table)}(");
            for (int i = 0; i < columnDesciptors.Count; i++)
            {
                if (i > 0)
                {
                    sql.Builder.Append(", ");
                }
                var column = columnDesciptors[i];
                var columnDefine = CreateTableColumnDefine(column.ColumnName, column.ColumnType, column.PrimaryKeyFlag, column.NullableFlag);
                sql.Builder.Append(columnDefine.Sql);
            }
            sql.Builder.AppendLine(")");
            return sql;
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Sqled DropTable(string schema, string table)
        {
            return $"DROP TABLE {GetSpecialName(table)};";
        }

        #endregion

        #region 数据列

        /// <summary>
        /// 获取表中所有列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Sqled GetColumns(string schema, string table)
        {
            return $"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_FLAGS FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}'";
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnDesciptor"></param>
        /// <returns></returns>
        public Sqled CreateColumn(DbColumnDesciptor columnDesciptor)
        {
            var schema = columnDesciptor.SchemaName;
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
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="columnTarget"></param>
        /// <returns></returns>
        public Sqled RenameColumn(string schema, string table, string column, string columnTarget)
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
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="columnTarget"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public Sqled CopyColumn(string schema, string table, string column, string columnTarget, string columnType)
        {
            return $"UPDATE {GetSpecialName(table)} SET {GetSpecialName(columnTarget)} = {GetSpecialName(column)};";
        }

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public Sqled DropColumn(string schema, string table, string column)
        {
            return $"ALTER TABLE {GetSpecialName(table)} DROP COLUMN {GetSpecialName(column)};";
        }

        #endregion

        #region 索引

        /// <summary>
        /// 获取表的所有索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Sqled GetIndexes(string schema, string table)
        {
            return $"SELECT IndexName, Name, Position FROM MSysIndexColumns INNER JOIN MSysIndexes ON MSysIndexColumns.Id = MSysIndexes.Id WHERE MSysIndexColumns.ObjectId = (SELECT id FROM MSysObjects WHERE name = '{table}' AND type = 1)";
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="indexDesciptor"></param>
        /// <returns></returns>
        public Sqled CreateIndex(DbIndexDesciptor indexDesciptor)
        {
            var schema = indexDesciptor.SchemaName;
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
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public Sqled DropIndex(string schema, string table, string column)
        {
            return $"DROP INDEX {table}_{column}_IDX ON {GetSpecialName(table)};";
        }

        #endregion
    }
}