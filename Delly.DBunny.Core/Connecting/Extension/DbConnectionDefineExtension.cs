using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Connecting.Extension
{
    /// <summary>
    /// 数据库提供程序工厂
    /// </summary>
    public static class DbConnectionDefineExtension
    {
        /// <summary>
        /// 获取数据库连接描述器
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="name"></param>
        /// <param name="define"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static DbConnectionDescriptor GetDbConnectionDescriptor(this IDbConnectionDefine define, string databaseType, string connectionName)
        {
            return new DbConnectionDescriptor(connectionName, databaseType, define.ConnectionString);
        }
    }
}
