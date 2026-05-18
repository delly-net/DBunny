using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Core.Sql.Extension
{
    /// <summary>
    /// Sql集 扩展
    /// </summary>
    public static class SqlSetExtension
    {
        /// <summary>
        /// 附加内容到 SQL
        /// </summary>
        /// <param name="sqlSet">SQL 集合</param>
        /// <param name="value">要附加的值</param>
        /// <returns>Sqled 实例</returns>
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
        /// <param name="sql">SQL 集合</param>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>Sqled 实例</returns>
        public static Sqled Set(this Sqled sql, string name, object value)
        {
            sql.Parameters[name] = value;
            return sql;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="sql">SQL 集合</param>
        /// <param name="pair">参数键值对</param>
        /// <returns>Sqled 实例</returns>
        public static Sqled Set(this Sqled sql, KeyValuePair<string, object> pair)
        {
            sql.Parameters[pair.Key] = pair.Value;
            return sql;
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="sql">SQL 集合</param>
        /// <param name="parameters">参数集合</param>
        /// <returns>Sqled 实例</returns>
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
