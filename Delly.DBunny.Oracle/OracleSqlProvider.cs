using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
using Delly.Modeling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Oracle
{
    /// <summary>
    /// Oracle Sql 提供程序
    /// </summary>
    public class OracleSqlProvider : ISqlProvider
    {
        /// <summary>
        /// 是否有 数据库 层
        /// </summary>
        public bool HasDatabase => false;

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
        /// <returns>Oracle 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(ColumnType columnType, TypeCode typeCode, bool autoIncrementFlag, int length = 0, int precision = 0)
        {
            // Oracle uses SEQUENCE for auto-increment, not column types
            return GetSpecialTypeName(typeCode, length, precision);
        }

        /// <summary>
        /// 获取数据库特定类型名称（根据 .NET 类型代码）
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>Oracle 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return "NUMBER(1)";
                case TypeCode.Byte:
                case TypeCode.SByte:
                    return "NUMBER(3)";
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return "NUMBER(5)";
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return "NUMBER(10)";
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "NUMBER(19)";
                case TypeCode.Single:
                    return "BINARY_FLOAT";
                case TypeCode.Double:
                    return "BINARY_DOUBLE";
                case TypeCode.Decimal:
                    if (length > 0 && precision > 0) { return $"NUMBER({length},{precision})"; }
                    if (length > 0) { return $"NUMBER({length})"; }
                    if (precision > 0) { return $"NUMBER(18,{precision})"; }
                    return "NUMBER(18,4)";
                case TypeCode.DateTime:
                    return "TIMESTAMP";
                case TypeCode.String:
                    if (length > 0 && length <= 4000) { return $"VARCHAR2({length})"; }
                    if (length > 4000) { return $"CLOB"; }
                    return "VARCHAR2(4000)";
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
        /// <returns>Oracle 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(ColumnType columnType, int length = 0, int precision = 0)
        {
            // 兼容列类型特性定义
            switch (columnType)
            {
                case ColumnType.DECIMAL:
                    if (length > 0 && precision > 0) { return $"NUMBER({length},{precision})"; }
                    if (length > 0) { return $"NUMBER({length})"; }
                    if (precision > 0) { return $"NUMBER(18,{precision})"; }
                    return "NUMBER(18,4)";
                case ColumnType.BOOL:
                    return "NUMBER(3)";
                case ColumnType.INTEGER:
                    return "NUMBER(10)";
                case ColumnType.LONG:
                    return "NUMBER(19)";
                case ColumnType.TIME:
                    return "TIMESTAMP";
                case ColumnType.VARCHAR:
                    if (length > 0 && length <= 4000) { return $"VARCHAR2({length})"; }
                    if (length > 4000) { return $"CLOB"; }
                    return "VARCHAR2(255)";
                case ColumnType.TEXT:
                    return "CLOB";
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
            return $":{name}".ToSql().Set(name, value);
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
            sb.Append($" OFFSET {skip ?? 0} ROWS FETCH NEXT {take.Value} ROWS ONLY");
            return sqled;
        }

        #region 数据库

        /// <summary>
        /// 获取所有 数据库
        /// </summary>
        /// <returns>获取数据库的 SQL 命令</returns>
        public Sqled GetDatabases()
        {
            throw new NotSupportedException("Oracle does not support multiple databases at SQL level. Databases are created at the instance level. HasDatabase is false.");
        }

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <param name="options">配置选项</param>
        /// <returns>创建数据库的 SQL 命令</returns>
        public Sqled CreateDatabase(string database, IDictionary<string, object> options)
        {
            throw new NotSupportedException("Oracle databases are created at the instance level, not via SQL commands. HasDatabase is false.");
        }

        /// <summary>
        /// 删除 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <returns>删除数据库的 SQL 命令</returns>
        public Sqled DropDatabase(string database)
        {
            throw new NotSupportedException("Oracle does not support dropping databases at SQL level. HasDatabase is false.");
        }

        #endregion

        #region Schema

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns>获取 Schema（用户）的 SQL 命令</returns>
        public Sqled GetSchemas()
        {
            return "SELECT username FROM all_users WHERE username NOT IN ('SYS', 'SYSTEM', 'OUTLN', 'DBSNMP', 'APPQOSSYS', 'DBSFWUSER', 'GSMADMIN_INTERNAL', 'ORDDATA', 'ORACLE_OCM', 'XS$NULL', 'MDSYS', 'OLAPSYS', 'OWBSYS', 'EXFSYS', 'CTXSYS', 'XDB', 'ANONYMOUS', 'ORDPLUGINS', 'SI_INFORMTN_SCHEMA', 'ORDDATA', 'ORDSYS') ORDER BY username";
        }

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        public Sqled GetSchemas(string schema)
        {
            return $"SELECT username FROM all_users WHERE username = '{schema}'";
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema">Schema 名称（Oracle 中即用户名）</param>
        /// <param name="options">配置选项（password, tablespace, temp_tablespace）</param>
        /// <returns>创建 Schema 的 SQL 命令</returns>
        public Sqled CreateSchema(string schema, IDictionary<string, object> options)
        {
            var password = "password";
            var tablespace = "USERS";
            var tempTablespace = "TEMP";

            if (options != null)
            {
                if (options.TryGetValue("password", out var pwd) && pwd != null)
                {
                    password = pwd.ToString() ?? "password";
                }
                if (options.TryGetValue("tablespace", out var ts) && ts != null)
                {
                    tablespace = ts.ToString() ?? "USERS";
                }
                if (options.TryGetValue("temp_tablespace", out var tts) && tts != null)
                {
                    tempTablespace = tts.ToString() ?? "TEMP";
                }
            }

            return $"CREATE USER {GetSpecialName(schema)} IDENTIFIED BY {GetSpecialName(password)} DEFAULT TABLESPACE {tablespace} TEMPORARY TABLESPACE {tempTablespace}";
        }

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema">Schema 名称（Oracle 中即用户名）</param>
        /// <returns>删除 Schema 的 SQL 命令</returns>
        public Sqled DropSchema(string schema)
        {
            return $"DROP USER {GetSpecialName(schema)} CASCADE;";
        }

        #endregion

        #region 数据表

        /// <summary>
        /// 获取 Schema 所有表
        /// </summary>
        /// <param name="schema">Schema 名称（Oracle 中即用户名）</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTables(string schema)
        {
            // Use exact case since schema and table names are quoted in CREATE TABLE
            return $"SELECT table_name FROM all_tables WHERE owner = '{schema}' AND table_name NOT LIKE 'BIN$%' ORDER BY table_name";
        }

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTable(DbTableDesciptor tableDesciptor)
        {
            return $"SELECT table_name FROM all_tables WHERE owner = '{tableDesciptor.SchemaName}' AND table_name = '{tableDesciptor.TableName}'";
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
            sql.Builder.AppendLine(")");
            return sql;
        }

        /// <summary>
        /// 删除 表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>删除表的 SQL 命令</returns>
        public Sqled DropTable(DbTableDesciptor tableDesciptor)
        {
            return $"DROP TABLE {GetSpecialName(tableDesciptor.SchemaName)}.{GetSpecialName(tableDesciptor.TableName)} PURGE;";
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
            // Use case-insensitive comparison for owner, exact for table
            // Owner is case-insensitive, but table name is case-sensitive when quoted
            return $@"
SELECT
    c.column_name,
    c.data_type,
    CASE WHEN c.nullable = 'Y' THEN 'Y' ELSE 'N' END AS nullable,
    CASE
        WHEN pk.column_name IS NOT NULL THEN 'PRI'
        ELSE ''
    END AS column_key
FROM
    all_tab_columns c
LEFT JOIN (
    SELECT
        cc.column_name,
        cc.table_name
    FROM
        all_constraints con
    JOIN
        all_cons_columns cc ON con.constraint_name = cc.constraint_name
    WHERE
        UPPER(con.owner) = UPPER('{schema}')
        AND con.table_name = '{table}'
        AND con.constraint_type = 'P'
) pk ON c.table_name = pk.table_name AND c.column_name = pk.column_name
WHERE
    UPPER(c.owner) = UPPER('{schema}')
    AND c.table_name = '{table}'
ORDER BY
    c.column_id";
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
    c.column_name,
    c.data_type,
    CASE WHEN c.nullable = 'Y' THEN 'Y' ELSE 'N' END AS nullable,
    CASE
        WHEN pk.column_name IS NOT NULL THEN 'PRI'
        ELSE ''
    END AS column_key
FROM
    all_tab_columns c
LEFT JOIN (
    SELECT
        cc.column_name,
        cc.table_name
    FROM
        all_constraints con
    JOIN
        all_cons_columns cc ON con.constraint_name = cc.constraint_name
    WHERE
        UPPER(con.owner) = UPPER('{schema}')
        AND con.table_name = '{table}'
        AND con.constraint_type = 'P'
) pk ON c.table_name = pk.table_name AND c.column_name = pk.column_name
WHERE
    UPPER(c.owner) = UPPER('{schema}')
    AND c.table_name = '{table}'
    AND c.column_name = '{column}'
ORDER BY
    c.column_id";
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
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} ADD ({GetSpecialName(column)} {columnType} {nullableStr});";
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

            var nullableStr = nullable ? "NULL" : "NOT NULL";
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} MODIFY ({GetSpecialName(columnName)} {columnType} {nullableStr});";
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
            // Use all_indexes and all_ind_columns to get index information
            return $@"
SELECT
    i.index_name,
    i.uniqueness,
    c.column_name,
    i.table_name
FROM
    all_indexes i
JOIN
    all_ind_columns c ON i.index_name = c.index_name AND i.table_name = c.table_name
WHERE
    UPPER(i.table_owner) = UPPER('{schema}')
    AND UPPER(i.table_name) = UPPER('{table}')
    AND i.index_name NOT LIKE 'SYS_%'
    AND i.index_name NOT LIKE '%BIN$%'
ORDER BY
    i.index_name, c.column_position";
        }

        /// <summary>
        /// 获取单个索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>获取索引的 SQL 命令</returns>
        public Sqled GetIndex(DbIndexDesciptor indexDesciptor)
        {
            var schema = indexDesciptor.SchemaName;
            var table = indexDesciptor.TableName;
            return $@"
	SELECT
	    i.index_name,
	    i.uniqueness,
	    c.column_name,
	    i.table_name
	FROM
	    all_indexes i
	JOIN
	    all_ind_columns c ON i.index_name = c.index_name AND i.table_name = c.table_name
	WHERE
	    UPPER(i.table_owner) = UPPER('{schema}')
	    AND UPPER(i.table_name) = UPPER('{table}')
	    AND i.index_name = '{indexDesciptor.IndexName}'
	ORDER BY
	    i.index_name, c.column_position";
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
            var indexName = GetSpecialName($"{table}_{column}_IDX");
            if (unique) { return $"CREATE UNIQUE INDEX {indexName} ON {schemaName}.{tableName} ({columnName});"; }
            return $"CREATE INDEX {indexName} ON {schemaName}.{tableName} ({columnName});";
        }

        /// <summary>
        /// 删除 索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>删除索引的 SQL 命令</returns>
        public Sqled DropIndex(DbIndexDesciptor indexDesciptor)
        {
            // Index name format matches CreateIndex: {table}_{column}_IDX with quotes
            var indexName = GetSpecialName($"{indexDesciptor.TableName}_{indexDesciptor.ColumnName}_IDX");
            return $"DROP INDEX {indexName};";
        }

        #endregion
    }
}