using Delly.DBunny.Sql.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.MySql
{
    /// <summary>
    /// MySql Sql 提供程序
    /// </summary>
    public class MySqlSqlProvider : ISqlProvider
    {
        /// <summary>
        /// 是否有 Schema
        /// </summary>
        public bool HasSchema => true;

        /// <summary>
        /// 获取特有名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetSpecialName(string name)
        {
            return "`" + name + "`";
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
                    return "TINYINT(1)";
                case TypeCode.Byte:
                case TypeCode.SByte:
                    return "TINYINT";
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return "SMALLINT";
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return "INT";
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "BIGINT";
                case TypeCode.Single:
                    return "FLOAT";
                case TypeCode.Double:
                    return "DOUBLE";
                case TypeCode.Decimal:
                    if (length > 0 && precision > 0) { return $"DECIMAL({length},{precision})"; }
                    if (length > 0) { return $"DECIMAL({length},4)"; }
                    if (precision > 0) { return $"DECIMAL(18,{precision})"; }
                    return "DECIMAL(18,4)";
                case TypeCode.DateTime:
                    return "DATETIME";
                case TypeCode.String:
                    if (length > 0 && length <= 255) { return $"VARCHAR({length})"; }
                    if (length > 255) { return $"TEXT"; }
                    return "VARCHAR(255)";
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
                    if (length > 0 && precision > 0) { return $"DECIMAL({length},{precision})"; }
                    if (length > 0) { return $"DECIMAL({length},4)"; }
                    if (precision > 0) { return $"DECIMAL(18,{precision})"; }
                    return "DECIMAL(18,4)";
                case DbColumnType.TINY:
                    return "TINYINT";
                case DbColumnType.INTEGER:
                    return "INT";
                case DbColumnType.LONG:
                    return "BIGINT";
                case DbColumnType.TIME:
                    return "DATETIME";
                case DbColumnType.VARCHAR:
                    if (length > 0) { return $"VARCHAR({length})"; }
                    return "VARCHAR(255)";
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
        public Sqled GetSchemas()
        {
            return "SHOW DATABASES";
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Sqled CreateSchema(string schema)
        {
            return $"CREATE DATABASE {GetSpecialName(schema)} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Sqled GetTables(string schema)
        {
            return $"SHOW TABLES FROM {GetSpecialName(schema)}";
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
                return $"{GetSpecialName(column)} {columnType} NOT NULL AUTO_INCREMENT PRIMARY KEY";
            }
            return $"{GetSpecialName(column)} {columnType}{(nullable ? " NULL" : " NOT NULL")}";
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="columnDefines"></param>
        /// <returns></returns>
        public Sqled CreateTable(string schema, string table, IList<Sqled> columnDefines)
        {
            var sql = new Sqled();
            sql.Builder.AppendLine($"CREATE TABLE {GetSpecialName(schema)}.{GetSpecialName(table)}(");
            for (int i = 0; i < columnDefines.Count; i++)
            {
                var column = columnDefines[i];
                sql.Builder.Append(new string(' ', 4));
                sql.Builder.Append(column.Sql);
                sql.Set(column.Parameters);
                if (i < columnDefines.Count - 1) { sql.Append(','); }
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
            return $"SHOW COLUMNS FROM {GetSpecialName(schema)}.{GetSpecialName(table)}";
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="typeName"></param>
        /// <param name="primaryKey"></param>
        /// <param name="nullable"></param>
        /// <returns></returns>
        public Sqled CreateColumn(string schema, string table, string column, string typeName, bool primaryKey, bool nullable)
        {
            var nullableStr = nullable ? "NULL" : "NOT NULL";
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} ADD COLUMN {GetSpecialName(column)} {typeName} {nullableStr};";
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
            return $"UPDATE {GetSpecialName(schema)}.{GetSpecialName(table)} SET {GetSpecialName(columnTarget)} = CAST({GetSpecialName(column)} AS {columnType});";
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
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} RENAME COLUMN {GetSpecialName(column)} TO {GetSpecialName(columnTarget)};";
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
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} DROP COLUMN {GetSpecialName(column)};";
        }

        /// <summary>
        /// 获取表的所有索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Sqled GetIndexes(string schema, string table)
        {
            return $"SHOW INDEX FROM {GetSpecialName(schema)}.{GetSpecialName(table)}";
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="unique"></param>
        /// <returns></returns>
        public Sqled CreateIndex(string schema, string table, string column, bool unique)
        {
            var schemaName = GetSpecialName(schema);
            var tableName = GetSpecialName(table);
            var columnName = GetSpecialName(column);
            if (unique) { return $"CREATE UNIQUE INDEX {table}_{column}_IDX ON {schemaName}.{tableName} ({columnName});"; }
            return $"CREATE INDEX {table}_{column}_IDX ON {schemaName}.{tableName} ({columnName});";
        }

    }
}