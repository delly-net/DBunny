using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Sql.Extension
{
    /// <summary>
    /// 字符串 扩展
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 转为Sql对象
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static Sqled ToSql(this string sql)
        {
            return new Sqled(sql);
        }
    }
}
