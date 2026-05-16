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

        /// <summary>
        /// 是否有 Schema
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
        Sqled CreateSchema(string schema);

        /// <summary>
        /// 获取 Schema 所有表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        Sqled GetTables(string schema);

        /// <summary>
        /// 获取字段定义
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        Sqled ColumnDefine(string column, string columnType, bool primaryKey, bool nullable);

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        Sqled CreateTable(string schema, string table, IList<Sqled> columnDefines);

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
        Sqled CreateColumn(string schema, string table, string column, string columnType, bool primaryKey, bool nullable);

        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled RenameColumn(string schema, string table, string column, string columnTarget);

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

        /// <summary>
        /// 获取表中所有索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled GetIndexes(string schema, string table);

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        Sqled CreateIndex(string schema, string table, string column, bool unique);
    }
}
