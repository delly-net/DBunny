using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.SqlServer
{
    /// <summary>
    /// SQL Server Sql 提供程序
    /// </summary>
    public class SqlServerSqlProvider : ISqlProvider
    {
        /// <summary>
        /// 是否有 数据库 层
        /// </summary>
        public bool HasDatabase => true;

        /// <summary>
        /// 是否有 Schema 层
        /// </summary>
        public bool HasSchema => true;

        /// <summary>
        /// 获取特有名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>特殊格式名称（方括号包裹）</returns>
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
        /// <returns>SQL Server 类型名称</returns>
        public string GetSpecialTypeName(DbColumnType columnType, TypeCode typeCode, bool autoIncrementFlag, int length = 0, int precision = 0)
        {
            return GetSpecialTypeName(typeCode, length, precision);
        }

        /// <summary>
        /// 获取特有类型名称
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>SQL Server 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return "BIT";
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
                    return "REAL";
                case TypeCode.Double:
                    return "FLOAT";
                case TypeCode.Decimal:
                    if (length > 0 && precision > 0) { return $"DECIMAL({length},{precision})"; }
                    if (length > 0) { return $"DECIMAL({length},4)"; }
                    if (precision > 0) { return $"DECIMAL(18,{precision})"; }
                    return "DECIMAL(18,4)";
                case TypeCode.DateTime:
                    return "DATETIME";
                case TypeCode.String:
                    if (length > 0 && length <= 4000) { return $"NVARCHAR({length})"; }
                    if (length > 4000) { return $"NVARCHAR(MAX)"; }
                    return "NVARCHAR(255)";
                default:
                    throw new NotSupportedException($"Type code '{typeCode}' not supported.");
            }
        }

        /// <summary>
        /// 获取特有类型名称
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>SQL Server 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(DbColumnType columnType, int length = 0, int precision = 0)
        {
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
                    if (length > 0) { return $"NVARCHAR({length})"; }
                    return "NVARCHAR(255)";
                case DbColumnType.TEXT:
                    return "NVARCHAR(MAX)";
                default:
                    throw new NotSupportedException($"Column type '{columnType}' not supported.");
            }
        }

        #region 数据库

        /// <summary>
        /// 获取所有 数据库
        /// </summary>
        /// <returns>获取数据库的 SQL 命令</returns>
        public Sqled GetDatabases()
        {
            return "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name;";
        }

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <param name="options">配置选项（collation）</param>
        /// <returns>创建数据库的 SQL 命令</returns>
        public Sqled CreateDatabase(string database, IDictionary<string, object> options)
        {
            var sql = $"CREATE DATABASE {GetSpecialName(database)}";
            if (options != null)
            {
                if (options.TryGetValue("collation", out var collation) && collation != null)
                {
                    var collationValue = collation.ToString() ?? string.Empty;
                    sql += $" COLLATE {collationValue}";
                }
            }
            else
            {
                sql += " COLLATE SQL_Latin1_General_CP1_CI_AS";
            }
            return sql + ";";
        }

        /// <summary>
        /// 删除 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <returns>删除数据库的 SQL 命令</returns>
        public Sqled DropDatabase(string database)
        {
            return $"DROP DATABASE IF EXISTS {GetSpecialName(database)};";
        }

        #endregion

        #region Schema

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        public Sqled GetSchemas()
        {
            return "SELECT name FROM sys.schemas WHERE name NOT IN ('dbo', 'guest', 'sys', 'INFORMATION_SCHEMA') ORDER BY name;";
        }

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        public Sqled GetSchemas(string schema)
        {
            return $"SELECT name FROM sys.schemas WHERE name = '{schema}';";
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="options">配置选项（authorization）</param>
        /// <returns>创建 Schema 的 SQL 命令</returns>
        public Sqled CreateSchema(string schema, IDictionary<string, object> options)
        {
            var sql = new Sqled();
            sql.Builder.Append($"CREATE SCHEMA {GetSpecialName(schema)}");
            if (options != null && options.TryGetValue("authorization", out var authorization) && authorization != null)
            {
                var authName = authorization.ToString() ?? string.Empty;
                sql.Builder.Append($" AUTHORIZATION {GetSpecialName(authName)}");
            }
            sql.Builder.Append(";");
            return sql;
        }

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>删除 Schema 的 SQL 命令</returns>
        public Sqled DropSchema(string schema)
        {
            return $"DROP SCHEMA IF EXISTS {GetSpecialName(schema)};";
        }

        #endregion

        #region 数据表

        /// <summary>
        /// 获取 Schema 所有表
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTables(string schema)
        {
            return $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{schema}' AND TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;";
        }

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTable(DbTableDesciptor tableDesciptor)
        {
            return $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{tableDesciptor.SchemaName}' AND TABLE_NAME = '{tableDesciptor.TableName}' AND TABLE_TYPE = 'BASE TABLE';";
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

            if (primaryKey)
            {
                return $"{GetSpecialName(column)} {columnType} IDENTITY(1,1) NOT NULL PRIMARY KEY";
            }
            return $"{GetSpecialName(column)} {columnType}{(nullable ? " NULL" : " NOT NULL")}";
        }

        /// <summary>
        /// 创建 表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="columnDesciptors">列描述符集合</param>
        /// <returns>创建表的 SQL 命令</returns>
        public Sqled CreateTable(DbTableDesciptor tableDesciptor, IList<DbColumnDesciptor> columnDesciptors)
        {
            var schema = tableDesciptor.SchemaName;
            var table = tableDesciptor.TableName;
            var sql = new Sqled();
            sql.Builder.AppendLine($"CREATE TABLE {GetSpecialName(schema)}.{GetSpecialName(table)}(");
            for (int i = 0; i < columnDesciptors.Count; i++)
            {
                var column = columnDesciptors[i];
                sql.Builder.Append(new string(' ', 4));
                var columnDefine = CreateTableColumnDefine(column);
                sql.Builder.Append(columnDefine.Sql);
                if (i < columnDesciptors.Count - 1) { sql.Append(','); }
                sql.Builder.AppendLine();
            }
            sql.Builder.AppendLine(");");
            return sql;
        }

        /// <summary>
        /// 删除 表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>删除表的 SQL 命令</returns>
        public Sqled DropTable(DbTableDesciptor tableDesciptor)
        {
            return $"DROP TABLE IF EXISTS {GetSpecialName(tableDesciptor.SchemaName)}.{GetSpecialName(tableDesciptor.TableName)};";
        }

        #endregion

        #region 数据列

        /// <summary>
        /// 获取表中所有列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取列的 SQL 命令</returns>
        public Sqled GetColumns(DbTableDesciptor tableDesciptor)
        {
            var schema = tableDesciptor.SchemaName;
            var table = tableDesciptor.TableName;
            return $@"
SELECT
    c.name AS column_name,
    t.name AS data_type,
    CAST(c.is_nullable AS BIT) AS is_nullable,
    CAST(CASE WHEN kcu.column_name IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS is_primary_key
FROM sys.columns c
INNER JOIN sys.tables tbl ON c.object_id = tbl.object_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
INNER JOIN sys.schemas s ON tbl.schema_id = s.schema_id
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    ON kcu.TABLE_SCHEMA = s.name
    AND kcu.TABLE_NAME = tbl.name
    AND kcu.COLUMN_NAME = c.name
    AND kcu.CONSTRAINT_NAME LIKE 'PK%'
WHERE s.name = '{schema}'
    AND tbl.name = '{table}'
ORDER BY c.column_id;";
        }

        /// <summary>
        /// 获取单个列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名称</param>
        /// <returns>获取列的 SQL 命令</returns>
        public Sqled GetColumn(DbTableDesciptor tableDesciptor, string column)
        {
            var schema = tableDesciptor.SchemaName;
            var table = tableDesciptor.TableName;
            return $@"
SELECT
    c.name AS column_name,
    t.name AS data_type,
    CAST(c.is_nullable AS BIT) AS is_nullable,
    CAST(CASE WHEN kcu.column_name IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS is_primary_key
FROM sys.columns c
INNER JOIN sys.tables tbl ON c.object_id = tbl.object_id
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
INNER JOIN sys.schemas s ON tbl.schema_id = s.schema_id
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    ON kcu.TABLE_SCHEMA = s.name
    AND kcu.TABLE_NAME = tbl.name
    AND kcu.COLUMN_NAME = c.name
    AND kcu.CONSTRAINT_NAME LIKE 'PK%'
WHERE s.name = '{schema}'
    AND tbl.name = '{table}'
    AND c.name = '{column}'
ORDER BY c.column_id;";
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnDesciptor">列描述符</param>
        /// <returns>创建列的 SQL 命令</returns>
        public Sqled CreateColumn(DbColumnDesciptor columnDesciptor)
        {
            var schema = columnDesciptor.SchemaName;
            var table = columnDesciptor.TableName;
            var column = columnDesciptor.ColumnName;
            var columnType = columnDesciptor.ColumnType;
            var nullable = columnDesciptor.NullableFlag;

            var nullableStr = nullable ? "NULL" : "NOT NULL";
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} ADD {GetSpecialName(column)} {columnType} {nullableStr};";
        }

        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">新列名</param>
        /// <returns>重命名列的 SQL 命令</returns>
        public Sqled RenameColumn(DbTableDesciptor tableDesciptor, string column, string columnTarget)
        {
            return $"EXEC sp_rename '{tableDesciptor.SchemaName}.{tableDesciptor.TableName}.{column}', '{columnTarget}', 'COLUMN';";
        }

        /// <summary>
        /// 修改列
        /// </summary>
        /// <param name="column">原列描述符</param>
        /// <param name="columnTarget">目标列描述符</param>
        /// <returns>修改列的 SQL 命令</returns>
        public Sqled ModifyColumn(DbColumnDesciptor column, DbColumnDesciptor columnTarget)
        {
            var schema = column.SchemaName;
            var table = column.TableName;
            var columnName = column.ColumnName;
            var columnType = columnTarget.ColumnType;
            var nullable = columnTarget.NullableFlag;

            var sql = new Sqled();
            var schemaName = GetSpecialName(schema);
            var tableName = GetSpecialName(table);
            var colName = GetSpecialName(columnName);

            sql.Builder.AppendLine($"ALTER TABLE {schemaName}.{tableName}");
            sql.Builder.AppendLine($"    ALTER COLUMN {colName} {columnType} {(nullable ? "NULL" : "NOT NULL")};");

            return sql;
        }

        /// <summary>
        /// 复制列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">目标列名</param>
        /// <param name="columnType">列类型</param>
        /// <returns>复制列的 SQL 命令</returns>
        public Sqled CopyColumn(DbTableDesciptor tableDesciptor, string column, string columnTarget, string columnType)
        {
            return $"UPDATE {GetSpecialName(tableDesciptor.SchemaName)}.{GetSpecialName(tableDesciptor.TableName)} SET {GetSpecialName(columnTarget)} = CAST({GetSpecialName(column)} AS {columnType});";
        }

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名</param>
        /// <returns>删除列的 SQL 命令</returns>
        public Sqled DropColumn(DbTableDesciptor tableDesciptor, string column)
        {
            return $"ALTER TABLE {GetSpecialName(tableDesciptor.SchemaName)}.{GetSpecialName(tableDesciptor.TableName)} DROP COLUMN {GetSpecialName(column)};";
        }

        #endregion

        #region 索引

        /// <summary>
        /// 获取表的所有索引
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取索引的 SQL 命令</returns>
        public Sqled GetIndexes(DbTableDesciptor tableDesciptor)
        {
            var schema = tableDesciptor.SchemaName;
            var table = tableDesciptor.TableName;
            return $@"
SELECT
    i.name AS index_name,
    i.is_unique AS is_unique,
    c.name AS column_name
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE s.name = '{schema}'
    AND t.name = '{table}'
    AND i.name IS NOT NULL
ORDER BY i.name, ic.key_ordinal;";
        }

        /// <summary>
        /// 获取单个索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>获取索引的 SQL 命令</returns>
        public Sqled GetIndex(DbIndexDesciptor indexDesciptor)
        {
            return $@"
SELECT
    i.name AS index_name,
    i.is_unique AS is_unique,
    c.name AS column_name
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE s.name = '{indexDesciptor.SchemaName}'
    AND t.name = '{indexDesciptor.TableName}'
    AND i.name = '{indexDesciptor.IndexName}'
ORDER BY i.name, ic.key_ordinal;";
        }

        /// <summary>
        /// 创建 索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>创建索引的 SQL 命令</returns>
        public Sqled CreateIndex(DbIndexDesciptor indexDesciptor)
        {
            var schema = indexDesciptor.SchemaName;
            var table = indexDesciptor.TableName;
            var column = indexDesciptor.ColumnName;
            var unique = indexDesciptor.UniqueFlag;

            var schemaName = GetSpecialName(schema);
            var tableName = GetSpecialName(table);
            var columnName = GetSpecialName(column);
            if (unique) { return $"CREATE UNIQUE INDEX {table}_{column}_IDX ON {schemaName}.{tableName} ({columnName});"; }
            return $"CREATE INDEX {table}_{column}_IDX ON {schemaName}.{tableName} ({columnName});";
        }

        /// <summary>
        /// 删除 索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>删除索引的 SQL 命令</returns>
        public Sqled DropIndex(DbIndexDesciptor indexDesciptor)
        {
            var indexName = indexDesciptor.IndexName;
            if (string.IsNullOrEmpty(indexName))
            {
                indexName = $"{indexDesciptor.TableName}_{indexDesciptor.ColumnName}_IDX";
            }
            return $"DROP INDEX IF EXISTS {indexName} ON {GetSpecialName(indexDesciptor.SchemaName)}.{GetSpecialName(indexDesciptor.TableName)};";
        }

        #endregion
    }
}