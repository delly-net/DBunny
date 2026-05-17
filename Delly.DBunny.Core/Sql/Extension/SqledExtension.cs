using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Sql.Extension
{
    /// <summary>
    /// Sql集 扩展
    /// </summary>
    public static class SqlSetExtension
    {
        /// <summary>
        /// 获取数据库连接描述器
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="name"></param>
        /// <param name="define"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static Sqled Append(
            this Sqled sqlSet,
#if NETSTANDARD2_0
            object value
#else
            object? value
#endif
            )
        {
            sqlSet.Builder.Append(value);
            return sqlSet;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="sqlSet"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Sqled Set(this Sqled sql, string name, object value)
        {
            sql.Parameters[name] = value;
            return sql;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="sqlSet"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Sqled Set(this Sqled sql, KeyValuePair<string, object> pair)
        {
            sql.Parameters[pair.Key] = pair.Value;
            return sql;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="sqlSet"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Sqled Set(this Sqled sql, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            foreach (var pair in parameters)
            {
                sql.Parameters[pair.Key] = pair.Value;
            }
            return sql;
        }
    }
}
