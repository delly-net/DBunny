using Delly.DBunny.Core;
using Delly.DBunny.Core.Sql.Extension;
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
        /// <returns>SQLite 类型名称</returns>
        /// <exception cref="NotSupportedException"></exception>
        public string GetSpecialTypeName(DbColumnType columnType, TypeCode typeCode, bool autoIncrementFlag, int length = 0, int precision = 0)
        {
            // SQLite uses INTEGER PRIMARY KEY AUTOINCREMENT for auto-increment columns
            return GetSpecialTypeName(typeCode, length, precision);
        }

        /// <summary>
        /// 获取数据库特定类型名称（根据 .NET 类型代码）
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>SQLite 类型名称</returns>
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
                default: throw new NotSupportedException($"Type code '{typeCode}' not supported.");
            }
        }

        /// <summary>
        /// 获取数据库特定类型名称（根据列类型）
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>SQLite 类型名称</returns>
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
                default: throw new NotSupportedException($"Column type '{columnType}' not supported.");
            }
        }

        #region 数据库

        /// <summary>
        /// 获取所有 数据库
        /// </summary>
        /// <returns>获取数据库的 SQL 命令</returns>
        public Sqled GetDatabases()
        {
            throw new NotSupportedException("SQLite does not support multiple databases. HasDatabase is false.");
        }

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <param name="options">配置选项</param>
        /// <returns>创建数据库的 SQL 命令</returns>
        public Sqled CreateDatabase(string database, IDictionary<string, object> options)
        {
            throw new NotSupportedException("SQLite creates databases by opening a connection to a file, not via SQL commands. HasDatabase is false.");
        }

        /// <summary>
        /// 删除 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <returns>删除数据库的 SQL 命令</returns>
        public Sqled DropDatabase(string database)
        {
            throw new NotSupportedException("SQLite does not support dropping databases. HasDatabase is false.");
        }

        #endregion

        #region Schema

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        public Sqled GetSchemas()
        {
            throw new NotSupportedException("SQLite does not support schemas. HasSchema is false.");
        }

        /// <summary>
        /// 获取单个 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        public Sqled GetSchemas(string schema)
        {
            throw new NotSupportedException("SQLite does not support schemas. HasSchema is false.");
        }

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="options">配置选项</param>
        /// <returns>创建 Schema 的 SQL 命令</returns>
        public Sqled CreateSchema(string schema, IDictionary<string, object> options)
        {
            throw new NotSupportedException("SQLite does not support schemas. HasSchema is false.");
        }

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>删除 Schema 的 SQL 命令</returns>
        public Sqled DropSchema(string schema)
        {
            throw new NotSupportedException("SQLite does not support schemas. HasSchema is false.");
        }

        #endregion

        #region 数据表

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="schema">Schema 名称（SQLite 中忽略）</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTables(string schema)
        {
            return $"SELECT name FROM sqlite_master WHERE type='table'";
        }

        /// <summary>
        /// 获取单个表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取表的 SQL 命令</returns>
        public Sqled GetTable(DbTableDesciptor tableDesciptor)
        {
            return $"SELECT name FROM sqlite_master WHERE type='table' AND name={GetSpecialName(tableDesciptor.TableName)}";
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
        /// 创建表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="columnDesciptors">列描述符集合</param>
        /// <returns>创建表的 SQL 命令</returns>
        public Sqled CreateTable(DbTableDesciptor tableDesciptor, IList<DbColumnDesciptor> columnDesciptors)
        {
            var table = tableDesciptor.TableName;
            var sql = new Sqled();
            sql.Builder.AppendLine($"CREATE TABLE {GetSpecialName(table)}(");
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
        /// 删除表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>删除表的 SQL 命令</returns>
        public Sqled DropTable(DbTableDesciptor tableDesciptor)
        {
            return $"DROP TABLE IF EXISTS {GetSpecialName(tableDesciptor.TableName)};";
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
            return $"PRAGMA table_info({GetSpecialName(tableDesciptor.TableName)})";
        }

        /// <summary>
        /// 获取单个列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名称</param>
        /// <returns>获取列的 SQL 命令</returns>
        public Sqled GetColumn(DbTableDesciptor tableDesciptor, string column)
        {
            return $"PRAGMA table_info({GetSpecialName(tableDesciptor.TableName)})";
        }

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnDesciptor">列描述符</param>
        /// <returns>创建列的 SQL 命令</returns>
        public Sqled CreateColumn(DbColumnDesciptor columnDesciptor)
        {
            var table = columnDesciptor.TableName;
            var column = columnDesciptor.ColumnName;
            var columnType = columnDesciptor.ColumnType;
            var nullable = columnDesciptor.NullableFlag;

            return $"ALTER TABLE {GetSpecialName(table)} ADD COLUMN {GetSpecialName(column)} {columnType}{(nullable ? " NULL" : " NOT NULL")};";
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
            return $"ALTER TABLE {GetSpecialName(tableDesciptor.TableName)} RENAME COLUMN {GetSpecialName(column)} TO {GetSpecialName(columnTarget)};";
        }

        /// <summary>
        /// 修改列
        /// </summary>
        /// <param name="column">原列描述符</param>
        /// <param name="columnTarget">目标列描述符</param>
        /// <returns>修改列的 SQL 命令</returns>
        public Sqled ModifyColumn(DbColumnDesciptor column, DbColumnDesciptor columnTarget)
        {
            throw new NotSupportedException("SQLite does not support modifying column types directly. Recreate the table with the new schema instead.");
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
            return $"UPDATE {GetSpecialName(tableDesciptor.TableName)} SET {GetSpecialName(columnTarget)} = CAST({GetSpecialName(column)} AS {columnType});";
        }

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名</param>
        /// <returns>删除列的 SQL 命令</returns>
        public Sqled DropColumn(DbTableDesciptor tableDesciptor, string column)
        {
            return $"ALTER TABLE {GetSpecialName(tableDesciptor.TableName)} DROP COLUMN {GetSpecialName(column)};";
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
            return $"PRAGMA index_list({GetSpecialName(tableDesciptor.TableName)});";
        }

        /// <summary>
        /// 获取单个索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>获取索引的 SQL 命令</returns>
        public Sqled GetIndex(DbIndexDesciptor indexDesciptor)
        {
            return $"PRAGMA index_info({GetSpecialName(indexDesciptor.IndexName)});";
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>创建索引的 SQL 命令</returns>
        public Sqled CreateIndex(DbIndexDesciptor indexDesciptor)
        {
            var table = indexDesciptor.TableName;
            var column = indexDesciptor.ColumnName;
            var unique = indexDesciptor.UniqueFlag;

            var tableName = GetSpecialName(table);
            var columnName = GetSpecialName(column);
            if (unique) { return $"CREATE UNIQUE INDEX {table}_{column}_IDX ON {tableName} ({columnName});"; }
            return $"CREATE INDEX {table}_{column}_IDX ON {tableName} ({columnName});";
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>删除索引的 SQL 命令</returns>
        public Sqled DropIndex(DbIndexDesciptor indexDesciptor)
        {
            return $"DROP INDEX IF EXISTS {indexDesciptor.TableName}_{indexDesciptor.ColumnName}_IDX;";
        }

        #endregion
    }
}