using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Core
{
    /// <summary>
    /// Sql 提供程序
    /// </summary>
    public interface ISqlProvider
    {

        /// <summary>
        /// 获取特有名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>特殊格式名称</returns>
        string GetSpecialName(string name);

        /// <summary>
        /// 获取特有类型
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>数据库特定类型名称</returns>
        string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0);

        /// <summary>
        /// 获取特有类型
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>数据库特定类型名称</returns>
        string GetSpecialTypeName(DbColumnType columnType, int length = 0, int precision = 0);

        #region 数据库

        /// <summary>
        /// 是否有 数据库 层
        /// </summary>
        bool HasDatabase { get; }

        /// <summary>
        /// 获取所有 数据库
        /// </summary>
        /// <returns>获取数据库的 SQL 命令</returns>
        Sqled GetDatabases();

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <param name="options">配置选项</param>
        /// <returns>创建数据库的 SQL 命令</returns>
        Sqled CreateDatabase(string database, IDictionary<string, object> options);

        /// <summary>
        /// 删除 数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <returns>删除数据库的 SQL 命令</returns>
        Sqled DropDatabase(string database);

        #endregion

        #region Schema

        /// <summary>
        /// 是否有 Schema 层
        /// </summary>
        bool HasSchema { get; }

        /// <summary>
        /// 获取所有 Schema
        /// </summary>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        Sqled GetSchemas();

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="options">配置选项</param>
        /// <returns>创建 Schema 的 SQL 命令</returns>
        Sqled CreateSchema(string schema, IDictionary<string, object> options);

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>删除 Schema 的 SQL 命令</returns>
        Sqled DropSchema(string schema);

        #endregion

        #region 数据表

        /// <summary>
        /// 获取 Schema 所有表
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取表的 SQL 命令</returns>
        Sqled GetTables(string schema);

        /// <summary>
        /// 获取创建表时的字段定义
        /// </summary>
        /// <param name="column">列名称</param>
        /// <param name="columnType">列类型</param>
        /// <param name="primaryKey">是否为主键</param>
        /// <param name="nullable">是否可空</param>
        /// <returns>字段定义 SQL</returns>
        Sqled CreateTableColumnDefine(string column, string columnType, bool primaryKey, bool nullable);

        /// <summary>
        /// 创建 表
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <param name="columnDesciptors">列描述符集合</param>
        /// <returns>创建表的 SQL 命令</returns>
        Sqled CreateTable(string schema, string table, IList<DbColumnDesciptor> columnDesciptors);

        /// <summary>
        /// 删除 表
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <returns>删除表的 SQL 命令</returns>
        Sqled DropTable(string schema, string table);

        #endregion

        #region 数据列

        /// <summary>
        /// 获取表中所有列
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <returns>获取列的 SQL 命令</returns>
        Sqled GetColumns(string schema, string table);

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnDesciptor">列描述符</param>
        /// <returns>创建列的 SQL 命令</returns>
        Sqled CreateColumn(DbColumnDesciptor columnDesciptor);

        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">新列名</param>
        /// <returns>重命名列的 SQL 命令</returns>
        Sqled RenameColumn(string schema, string table, string column, string columnTarget);

        /// <summary>
        /// 修改列
        /// </summary>
        /// <param name="column">原列描述符</param>
        /// <param name="columnTarget">目标列描述符</param>
        /// <returns>修改列的 SQL 命令</returns>
        Sqled ModifyColumn(DbColumnDesciptor column, DbColumnDesciptor columnTarget);

        /// <summary>
        /// 复制列
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">目标列名</param>
        /// <param name="columnType">列类型</param>
        /// <returns>复制列的 SQL 命令</returns>
        Sqled CopyColumn(string schema, string table, string column, string columnTarget, string columnType);

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <param name="column">列名</param>
        /// <returns>删除列的 SQL 命令</returns>
        Sqled DropColumn(string schema, string table, string column);

        #endregion

        #region 索引

        /// <summary>
        /// 获取表中所有索引
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <returns>获取索引的 SQL 命令</returns>
        Sqled GetIndexes(string schema, string table);

        /// <summary>
        /// 创建 索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>创建索引的 SQL 命令</returns>
        Sqled CreateIndex(DbIndexDesciptor indexDesciptor);

        /// <summary>
        /// 删除 索引
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <param name="table">表名称</param>
        /// <param name="column">列名</param>
        /// <returns>删除索引的 SQL 命令</returns>
        Sqled DropIndex(string schema, string table, string column);

        #endregion
    }
}
