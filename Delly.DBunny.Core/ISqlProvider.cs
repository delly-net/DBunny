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

        #region 名称

        /// <summary>
        /// 获取特有名称
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>特殊格式名称</returns>
        string GetSpecialName(string name);

        #endregion

        #region 参数

        /// <summary>
        /// 创建参数Sql对象
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value"></param>
        /// <returns>特殊格式名称</returns>
        Sqled GetParamterSqled(string name, object value);

        /// <summary>
        /// 创建时间类型参数Sql对象
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value"></param>
        /// <returns>特殊格式名称</returns>
        Sqled GetTimeParamterSqled(string name, object value);

        #endregion

        #region 游标

        /// <summary>
        /// 附加游标
        /// </summary>
        /// <returns>特殊格式名称</returns>
        Sqled AppendOffset(Sqled sqlSet, int? take, int? skip);

        #endregion

        #region 类型

        /// <summary>
        /// 获取数据库特定类型名称（包含自增长标识）
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="typeCode">类型代码</param>
        /// <param name="autoIncrementFlag">自增长标识</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>数据库特定类型名称</returns>
        string GetSpecialTypeName(DbColumnType columnType, TypeCode typeCode, bool autoIncrementFlag, int length = 0, int precision = 0);

        /// <summary>
        /// 获取数据库特定类型名称（根据 .NET 类型代码）
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>数据库特定类型名称</returns>
        string GetSpecialTypeName(TypeCode typeCode, int length = 0, int precision = 0);

        /// <summary>
        /// 获取数据库特定类型名称（根据列类型）
        /// </summary>
        /// <param name="columnType">列类型</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <returns>数据库特定类型名称</returns>
        string GetSpecialTypeName(DbColumnType columnType, int length = 0, int precision = 0);

        #endregion

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
        /// 创建数据库
        /// </summary>
        /// <param name="database">数据库名称</param>
        /// <param name="options">配置选项</param>
        /// <returns>创建数据库的 SQL 命令</returns>
        Sqled CreateDatabase(string database, IDictionary<string, object> options);

        /// <summary>
        /// 删除数据库
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
        /// 获取单个 Schema
        /// </summary>
        /// <param name="schema">Schema 名称</param>
        /// <returns>获取 Schema 的 SQL 命令</returns>
        Sqled GetSchemas(string schema);

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
        /// 获取单个表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取表的 SQL 命令</returns>
        Sqled GetTable(DbTableDesciptor tableDesciptor);

        /// <summary>
        /// 获取创建表时的字段定义
        /// </summary>
        /// <param name="columnDesciptor">列描述符</param>
        /// <returns>字段定义 SQL</returns>
        Sqled CreateTableColumnDefine(DbColumnDesciptor columnDesciptor);

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="columnDesciptors">列描述符集合</param>
        /// <returns>创建表的 SQL 命令</returns>
        Sqled CreateTable(DbTableDesciptor tableDesciptor, IList<DbColumnDesciptor> columnDesciptors);

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>删除表的 SQL 命令</returns>
        Sqled DropTable(DbTableDesciptor tableDesciptor);

        #endregion

        #region 数据列

        /// <summary>
        /// 获取表中所有列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取列的 SQL 命令</returns>
        Sqled GetColumns(DbTableDesciptor tableDesciptor);

        /// <summary>
        /// 获取单个列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名称</param>
        /// <returns>获取列的 SQL 命令</returns>
        Sqled GetColumn(DbTableDesciptor tableDesciptor, string column);

        /// <summary>
        /// 创建列
        /// </summary>
        /// <param name="columnDesciptor">列描述符</param>
        /// <returns>创建列的 SQL 命令</returns>
        Sqled CreateColumn(DbColumnDesciptor columnDesciptor);

        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">新列名</param>
        /// <returns>重命名列的 SQL 命令</returns>
        Sqled RenameColumn(DbTableDesciptor tableDesciptor, string column, string columnTarget);

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
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">原列名</param>
        /// <param name="columnTarget">目标列名</param>
        /// <param name="columnType">列类型</param>
        /// <returns>复制列的 SQL 命令</returns>
        Sqled CopyColumn(DbTableDesciptor tableDesciptor, string column, string columnTarget, string columnType);

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <param name="column">列名</param>
        /// <returns>删除列的 SQL 命令</returns>
        Sqled DropColumn(DbTableDesciptor tableDesciptor, string column);

        #endregion

        #region 索引

        /// <summary>
        /// 获取表中所有索引
        /// </summary>
        /// <param name="tableDesciptor">表描述符</param>
        /// <returns>获取索引的 SQL 命令</returns>
        Sqled GetIndexes(DbTableDesciptor tableDesciptor);

        /// <summary>
        /// 获取单个索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>获取索引的 SQL 命令</returns>
        Sqled GetIndex(DbIndexDesciptor indexDesciptor);

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>创建索引的 SQL 命令</returns>
        Sqled CreateIndex(DbIndexDesciptor indexDesciptor);

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="indexDesciptor">索引描述符</param>
        /// <returns>删除索引的 SQL 命令</returns>
        Sqled DropIndex(DbIndexDesciptor indexDesciptor);

        #endregion
    }
}
