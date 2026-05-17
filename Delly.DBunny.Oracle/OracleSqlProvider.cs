using Delly.DBunny.Sql.Extension;
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
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetSpecialName(string name)
        {
            return "\"" + name + "\"";
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
                    if (length > 0 && precision > 0) { return $"NUMBER({length},{precision})"; }
                    if (length > 0) { return $"NUMBER({length})"; }
                    if (precision > 0) { return $"NUMBER(18,{precision})"; }
                    return "NUMBER(18,4)";
                case DbColumnType.TINY:
                    return "NUMBER(3)";
                case DbColumnType.INTEGER:
                    return "NUMBER(10)";
                case DbColumnType.LONG:
                    return "NUMBER(19)";
                case DbColumnType.TIME:
                    return "TIMESTAMP";
                case DbColumnType.VARCHAR:
                    if (length > 0 && length <= 4000) { return $"VARCHAR2({length})"; }
                    if (length > 4000) { return $"CLOB"; }
                    return "VARCHAR2(255)";
                case DbColumnType.TEXT:
                    return "CLOB";
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
            throw new NotSupportedException("Oracle does not support multiple databases at SQL level. Databases are created at the instance level. HasDatabase is false.");
        }

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="database"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Sqled CreateDatabase(string database, IDictionary<string, object> options)
        {
            throw new NotSupportedException("Oracle databases are created at the instance level, not via SQL commands. HasDatabase is false.");
        }

        /// <summary>
        /// 删除 数据库
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public Sqled DropDatabase(string database)
        {
            throw new NotSupportedException("Oracle does not support dropping databases at SQL level. HasDatabase is false.");
        }

        #endregion

        #region Schema

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns></returns>
        public Sqled GetSchemas()
        {
            return "SELECT username FROM all_users WHERE username NOT IN ('SYS', 'SYSTEM', 'OUTLN', 'DBSNMP', 'APPQOSSYS', 'DBSFWUSER', 'GSMADMIN_INTERNAL', 'ORDDATA', 'ORACLE_OCM', 'XS$NULL', 'MDSYS', 'OLAPSYS', 'OWBSYS', 'EXFSYS', 'CTXSYS', 'XDB', 'ANONYMOUS', 'ORDPLUGINS', 'SI_INFORMTN_SCHEMA', 'ORDDATA', 'ORDSYS') ORDER BY username";
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Sqled CreateSchema(string schema, IDictionary<string, object> options)
        {
            var password = "password";
            var tablespace = "USERS";
            var tempTablespace = "TEMP";

            if (options != null)
            {
                if (options.TryGetValue("password", out var pwd))
                {
                    password = pwd.ToString();
                }
                if (options.TryGetValue("tablespace", out var ts))
                {
                    tablespace = ts.ToString();
                }
                if (options.TryGetValue("temp_tablespace", out var tts))
                {
                    tempTablespace = tts.ToString();
                }
            }

            return $"CREATE USER {GetSpecialName(schema)} IDENTIFIED BY {GetSpecialName(password)} DEFAULT TABLESPACE {tablespace} TEMPORARY TABLESPACE {tempTablespace}";
        }

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Sqled DropSchema(string schema)
        {
            return $"DROP USER {GetSpecialName(schema)} CASCADE;";
        }

        #endregion

        #region 数据表

        /// <summary>
        /// 获取 Schema 所有表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public Sqled GetTables(string schema)
        {
            // Use exact case since schema and table names are quoted in CREATE TABLE
            return $"SELECT table_name FROM all_tables WHERE owner = '{schema}' AND table_name NOT LIKE 'BIN$%' ORDER BY table_name";
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
            if (primaryKey)
            {
                return $"{GetSpecialName(column)} {columnType} NOT NULL PRIMARY KEY";
            }
            return $"{GetSpecialName(column)} {columnType}{(nullable ? " NULL" : " NOT NULL")}";
        }

        /// <summary>
        /// 创建 表
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="columnDesciptors"></param>
        /// <returns></returns>
        public Sqled CreateTable(string schema, string table, IList<DbColumnDesciptor> columnDesciptors)
        {
            var sql = new Sqled();
            sql.Builder.AppendLine($"CREATE TABLE {GetSpecialName(schema)}.{GetSpecialName(table)}(");
            for (int i = 0; i < columnDesciptors.Count; i++)
            {
                var column = columnDesciptors[i];
                sql.Builder.Append(new string(' ', 4));
                var columnDefine = CreateTableColumnDefine(column.ColumnName, column.ColumnType, column.PrimaryKeyFlag, column.NullableFlag);
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
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public Sqled DropTable(string schema, string table)
        {
            return $"DROP TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} PURGE;";
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

            var nullableStr = nullable ? "NULL" : "NOT NULL";
            return $"ALTER TABLE {GetSpecialName(schema)}.{GetSpecialName(table)} ADD ({GetSpecialName(column)} {columnType} {nullableStr});";
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
        /// 创建 索引
        /// </summary>
        /// <param name="indexDesciptor"></param>
        /// <returns></returns>
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
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public Sqled DropIndex(string schema, string table, string column)
        {
            // Index name format matches CreateIndex: {table}_{column}_IDX with quotes
            var indexName = GetSpecialName($"{table}_{column}_IDX");
            return $"DROP INDEX {indexName};";
        }

        #endregion
    }
}