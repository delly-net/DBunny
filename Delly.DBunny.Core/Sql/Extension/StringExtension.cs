using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Core.Sql.Extension
{
    /// <summary>
    /// 字符串 扩展
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 转为 Sql 对象
        /// </summary>
        /// <param name="sql">SQL 脚本字符串</param>
        /// <returns>Sqled 实例</returns>
        public static Sqled ToSql(this string sql)
        {
            return new Sqled(sql);
        }
    }
}
