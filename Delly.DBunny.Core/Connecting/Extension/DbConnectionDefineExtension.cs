using Delly.DBunny.Core;
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
        /// <param name="define">连接定义</param>
        /// <param name="databaseType">数据库类型</param>
        /// <param name="connectionName">连接名称</param>
        /// <returns>连接描述符</returns>
        public static DbConnectionDescriptor GetDbConnectionDescriptor(this IDbConnectionDefine define, string databaseType, string connectionName)
        {
            return new DbConnectionDescriptor(connectionName, databaseType, define.ConnectionString);
        }
    }
}
