using Delly.DBunny.Sql.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Sqlite
{
    /// <summary>
    /// Sqlite Sql 提供程序
    /// </summary>
    public class SqliteSqlProvider : ISqlProvider
    {
        /// <summary>
        /// 是否有 Schema
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
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "INTEGER";
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return "REAL";
                case TypeCode.DateTime:
                    return "TEXT(32)";
                case TypeCode.String:
                    if (length > 0) { return $"TEXT({length})"; }
                    return "TEXT";
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
            // 兼容列类型特性定义
            switch (columnType)
            {
                case DbColumnType.DECIMAL:
                    return "REAL";
                case DbColumnType.TINY:
                    return "INTEGER";
                case DbColumnType.INTEGER:
                    return "INTEGER";
                case DbColumnType.LONG:
                    return "INTEGER";
                case DbColumnType.TIME:
                    return "TEXT(32)";
                case DbColumnType.VARCHAR:
                    if (length > 0) { return $"TEXT({length})"; }
                    return "TEXT";
                case DbColumnType.TEXT:
                    return "TEXT";
                default:
                    throw new NotSupportedException($"Column type '{columnType}' not supported.");
            }
        }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Sqled GetSchemas()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Sqled CreateSchema(string schema)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Sqled GetTables(string schema)
        {
            return $"SELECT name FROM sqlite_master WHERE type='table'";
        }

        /// <summary>
        /// 列定义
        /// </summary>
        /// <param name="column"></param>
        /// <param name="columnType"></param>
        /// <param name="primaryKey"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public Sqled ColumnDefine(string column, string columnType, bool primaryKey, bool nullable)
        {
            if (primaryKey)
            {
                return $"{GetSpecialName(column)} {columnType} NOT NULL PRIMARY KEY";
            }
            return $"{GetSpecialName(column)} {columnType}{(nullable ? " NULL" : " NOT NULL")}";
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
            sql.Builder.AppendLine($"CREATE TABLE {GetSpecialName(table)}(");
            for (int i = 0; i < columnDesciptors.Count; i++)
            {
                var column = columnDesciptors[i];
                sql.Builder.Append(new string(' ', 4));
                var columnDefine = ColumnDefine(column.ColumnName, column.ColumnType, column.PrimaryKeyFlag, column.NullableFlag);
                sql.Builder.Append(columnDefine.Sql);
                if (i < columnDesciptors.Count - 1) { sql.Append(','); }
                sql.Builder.AppendLine();
            }
            sql.Builder.AppendLine(");");
            return sql;
        }

        /// <summary>
        /// 获取所有列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Sqled GetColumns(string schema, string table)
        {
            return $"PRAGMA table_info({GetSpecialName(table)})";
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

            return $"ALTER TABLE {GetSpecialName(table)} ADD COLUMN {GetSpecialName(column)} {columnType}{(nullable ? " NULL" : " NOT NULL")};";
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
            return $"UPDATE {GetSpecialName(table)} SET {GetSpecialName(columnTarget)} = CAST({GetSpecialName(column)} AS {columnType});";
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
            return $"ALTER TABLE {GetSpecialName(table)} RENAME COLUMN {GetSpecialName(column)} TO {GetSpecialName(columnTarget)};";
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

        /// <summary>
        /// 获取表的所有索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Sqled GetIndexes(string schema, string table)
        {
            return $"PRAGMA index_list({GetSpecialName(table)});";
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
            if (unique) { return $"CREATE UNIQUE INDEX {table}_{column}_IDX ON {tableName} ({columnName});"; }
            return $"CREATE INDEX {table}_{column}_IDX ON {tableName} ({columnName});";
        }

    }
}
