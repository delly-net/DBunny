using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.PostgreSql
{
    /// <summary>
    /// PostgreSql Sql 提供程序
    /// </summary>
    public class PostgreSqlSqlProvider : ISqlProvider
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
        /// <returns>特殊格式名称（双引号包裹）</returns>
        public string GetSpecialName(string name)
        {
            return "\"" + name + "\"";
        }

        /// <summary>
        /// 获取数据库特定类型名称（包含自增长标识）
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="typeCode">类型代码</param>
        /// <param name="autoIncrementFlag">自增长标识</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>PostgreSQL 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(DbColumnType columnType, TypeCode typeCode, bool autoIncrementFlag, int length = 0, int precision = 0)
        {
            // PostgreSQL uses SERIAL types for auto-increment
            if (autoIncrementFlag)
            {
                switch (typeCode)
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        return "SMALLSERIAL";
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        return "SERIAL";
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return "BIGSERIAL";
                }
            }
            return GetSpecialTypeName(typeCode, length, precision);
        }

        /// <summary>
        /// 获取数据库特定类型名称（根据 .NET 类型代码）
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>PostgreSQL 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return "BOOLEAN";
                case TypeCode.Byte:
                case TypeCode.SByte:
                    return "SMALLINT";
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return "SMALLINT";
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return "INTEGER";
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "BIGINT";
                case TypeCode.Single:
                    return "REAL";
                case TypeCode.Double:
                    return "DOUBLE PRECISION";
                case TypeCode.Decimal:
                    if (length > 0 && precision > 0) { return $"NUMERIC({length},{precision})"; }
                    if (length > 0) { return $"NUMERIC({length},4)"; }
                    if (precision > 0) { return $"NUMERIC(18,{precision})"; }
                    return "NUMERIC(18,4)";
                case TypeCode.DateTime:
                    return "TIMESTAMP";
                case TypeCode.String:
                    if (length > 0 && length <= 255) { return $"VARCHAR({length})"; }
                    if (length > 255) { return $"TEXT"; }
                    return "VARCHAR(255)";
                default:
                    throw new NotSupportedException($"Type code '{typeCode}' not supported.");
            }
        }

        /// <summary>
        /// 获取数据库特定类型名称（根据列类型）
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>PostgreSQL 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(DbColumnType columnType, int length = 0, int precision = 0)
        {
            // 兼容列类型特性定义
            switch (columnType)
            {
                case DbColumnType.DECIMAL:
                    if (length > 0 && precision > 0) { return $"NUMERIC({length},{precision})"; }
                    if (length > 0) { return $"NUMERIC({length},4)"; }
                    if (precision > 0) { return $"NUMERIC(18,{precision})"; }
                    return "NUMERIC(18,4)";
                case DbColumnType.TINY:
                    return "SMALLINT";
                case DbColumnType.INTEGER:
                    return "INTEGER";
                case DbColumnType.LONG:
                    return "BIGINT";
                case DbColumnType.TIME:
                    return "TIMESTAMP";
                case DbColumnType.VARCHAR:
                    if (length > 0) { return $"VARCHAR({length})"; }
                    return "VARCHAR(255)";
                case DbColumnType.TEXT:
                    return "TEXT";
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
            return "SELECT datname FROM pg_database WHERE datistemplate = false;";
        }

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <param name="options">配置选项（owner, encoding, template）</param>
        /// <returns>创建数据库的 SQL 命令</returns>
        public Sqled CreateDatabase(string database, IDictionary<string, object> options)
        {
            var sql = $"CREATE DATABASE {GetSpecialName(database)}";
            if (options != null)
            {
                if (options.TryGetValue("owner", out var owner) && owner != null)
                {
                    var ownerName = owner.ToString() ?? string.Empty;
                    sql += $" OWNER {GetSpecialName(ownerName)}";
                }
                if (options.TryGetValue("encoding", out var encoding) && encoding != null)
                {
                    sql += $" ENCODING '{encoding}'";
                }
                if (options.TryGetValue("template", out var template) && template != null)
                {
                    var templateName = template.ToString() ?? string.Empty;
                    sql += $" TEMPLATE {GetSpecialName(templateName)}";
                }
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
            return "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog', 'information_schema')";
        }

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        public Sqled GetSchemas(string schema)
        {
            return $"SELECT schema_name FROM information_schema.schemata WHERE schema_name = '{schema}'";
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="options">配置选项（authorization）</param>
        /// <returns>创建 Schema 的 SQL 命令</returns>
        public Sqled CreateSchema(string schema, IDictionary<string, object> options)
        {
            var sql = $"CREATE SCHEMA {GetSpecialName(schema)}";
            if (options != null)
            {
                if (options.TryGetValue("authorization", out var authorization) && authorization != null)
                {
                    var authName = authorization.ToString() ?? string.Empty;
                    sql += $" AUTHORIZATION {GetSpecialName(authName)}";
                }
            }
            return sql + ";";
        }

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>删除 Schema 的 SQL 命令</returns>
        public Sqled DropSchema(string schema)
        {
            return $"DROP SCHEMA IF EXISTS {GetSpecialName(schema)} CASCADE;";
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
            return $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{schema}' AND table_type = 'BASE TABLE'";
        }

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTable(DbTableDesciptor tableDesciptor)
        {
            return $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{tableDesciptor.SchemaName}' AND table_name = '{tableDesciptor.TableName}' AND table_type = 'BASE TABLE'";
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
                return $"{GetSpecialName(column)} {columnType} NOT NULL PRIMARY KEY";
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
            return $"DROP TABLE IF EXISTS {GetSpecialName(tableDesciptor.SchemaName)}.{GetSpecialName(tableDesciptor.TableName)} CASCADE;";
        }

        #endregion

        #region 数据列

        /// <summary>
        /// 获取表中所有列
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <returns>获取列的 SQL 命令</returns>
        public Sqled GetColumns(DbTableDesciptor tableDesciptor)
        {
            var schema = tableDesciptor.SchemaName;
            var table = tableDesciptor.TableName;
            return $@"
SELECT
    c.column_name,
    c.data_type,
    c.is_nullable,
    CASE
        WHEN pk.column_name IS NOT NULL THEN 'PRI'
        ELSE ''
    END AS column_key
FROM
    information_schema.columns c
LEFT JOIN (
    SELECT
        ku.table_schema,
        ku.table_name,
        ku.column_name
    FROM
        information_schema.table_constraints tc
    JOIN
        information_schema.key_column_usage ku
        ON tc.constraint_name = ku.constraint_name
        AND tc.table_schema = ku.table_schema
    WHERE
        tc.constraint_type = 'PRIMARY KEY'
) pk ON c.table_schema = pk.table_schema
    AND c.table_name = pk.table_name
    AND c.column_name = pk.column_name
WHERE
    c.table_schema = '{schema}'
    AND c.table_name = '{table}'
ORDER BY
    c.ordinal_position;";
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
            var primaryKey = columnDesciptor.PrimaryKeyFlag;

            var nullableStr = nullable ? "NULL" : "NOT NULL";
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} ADD COLUMN {GetSpecialName(column)} {columnType} {nullableStr};";
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
            return $"ALTER TABLE {GetSpecialName(tableDesciptor.SchemaName)}.{GetSpecialName(tableDesciptor.TableName)} RENAME COLUMN {GetSpecialName(column)} TO {GetSpecialName(columnTarget)};";
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
            sql.Builder.AppendLine($"    ALTER COLUMN {colName} TYPE {columnType},");
            sql.Builder.AppendLine($"    ALTER COLUMN {colName} {(nullable ? "DROP NOT NULL" : "SET NOT NULL")};");

            return sql;
        }

        /// <summary>
        /// 复制列
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
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
            return $@"
SELECT
    i.relname AS indexname,
    ix.indisunique AS unique,
    a.attname AS column_name
FROM
    pg_class t,
    pg_class i,
    pg_index ix,
    pg_attribute a
WHERE
    t.oid = ix.indrelid
    AND i.oid = ix.indexrelid
    AND a.attrelid = t.oid
    AND a.attnum = ANY(ix.indkey)
    AND t.relkind = 'r'
    AND t.relname = '{tableDesciptor.TableName}'
    AND pg_get_userbyid(t.relowner) = CURRENT_USER
ORDER BY
    i.relname;";
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
	    i.relname AS indexname,
	    ix.indisunique AS unique,
	    a.attname AS column_name
	FROM
	    pg_class t,
	    pg_class i,
	    pg_index ix,
	    pg_attribute a
	WHERE
	    t.oid = ix.indrelid
	    AND i.oid = ix.indexrelid
	    AND a.attrelid = t.oid
	    AND a.attnum = ANY(ix.indkey)
	    AND t.relkind = 'r'
	    AND t.relname = '{indexDesciptor.TableName}'
	    AND i.relname = '{indexDesciptor.IndexName}'
	    AND pg_get_userbyid(t.relowner) = CURRENT_USER
	ORDER BY
	    i.relname;";
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
            return $"DROP INDEX IF EXISTS {GetSpecialName(indexDesciptor.SchemaName)}.{indexDesciptor.TableName}_{indexDesciptor.ColumnName}_IDX CASCADE;";
        }

        #endregion
    }
}