using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny
{
    /// <summary>
    /// Sql 提供程序
    /// </summary>
    public interface ISqlProvider
    {

        /// <summary>
        /// 获取特有名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetSpecialName(string name);

        /// <summary>
        /// 获取特有类型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0);

        /// <summary>
        /// 获取特有类型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetSpecialTypeName(DbColumnType columnType, int length = 0, int precision = 0);

        #region 数据库

        /// <summary>
        /// 是否有 数据库 层
        /// </summary>
        bool HasDatabase { get; }

        /// <summary>
        /// 获取所有 数据库
        /// </summary>
        /// <returns></returns>
        Sqled GetDatabases();

        /// <summary>
        /// 创建 数据库
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        Sqled CreateDatabase(string database, IDictionary<string, object> options);

        /// <summary>
        /// 删除 数据库
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        Sqled GetSchemas();

        /// <summary>
        /// 创建 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        Sqled CreateSchema(string schema, IDictionary<string, object> options);

        /// <summary>
        /// 删除 Schema
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled DropSchema(string schema);

        #endregion

        #region 数据表

        /// <summary>
        /// 获取 Schema 所有表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        Sqled GetTables(string schema);

        /// <summary>
        /// 获取创建表时的字段定义
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        Sqled CreateTableColumnDefine(string column, string columnType, bool primaryKey, bool nullable);

        /// <summary>
        /// 创建 表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        Sqled CreateTable(string schema, string table, IList<DbColumnDesciptor> columnDesciptors);

        /// <summary>
        /// 删除 表
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled DropTable(string schema, string table);

        #endregion

        #region 数据列

        /// <summary>
        /// 获取表中所有列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled GetColumns(string schema, string table);

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled CreateColumn(DbColumnDesciptor columnDesciptor);

        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled RenameColumn(string schema, string table, string column, string columnTarget);

        /// <summary>
        /// 修改列
        /// </summary>
        /// <param name="column"></param>
        /// <param name="columnTarget"></param>
        /// <returns></returns>
        Sqled ModifyColumn(DbColumnDesciptor column, DbColumnDesciptor columnTarget);

        /// <summary>
        /// 复制列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled CopyColumn(string schema, string table, string column, string columnTarget, string columnType);

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled DropColumn(string schema, string table, string column);

        #endregion

        #region 索引

        /// <summary>
        /// 获取表中所有索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled GetIndexes(string schema, string table);

        /// <summary>
        /// 创建 索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled CreateIndex(DbIndexDesciptor indexDesciptor);

        /// <summary>
        /// 删除 索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled DropIndex(string schema, string table, string column);

        #endregion
    }
}
