using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Providing.Extension
{
    /// <summary>
    /// 数据库提供程序工厂
    /// </summary>
    public static class DbProviderFactoryExtension
    {
        /// <summary>
        /// 获取数据库连接描述器
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="name"></param>
        /// <param name="define"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static DbConnectionDescriptor GetDbConnectionDescriptor(this IDbProviderFactory factory, string databaseType, string connectionName, IDbConnectionDefine define)
        {
            return factory.GetDbConnectionDescriptor(databaseType, connectionName, define.ConnectionString);
        }

        /// <summary>
        /// 获取数据库连接描述器
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="name"></param>
        /// <param name="define"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static DbConnectionDescriptor GetDbConnectionDescriptor(this IDbProviderFactory factory, string databaseType, string connectionName, string connectionString)
        {
            var provider = factory.GetProvider(databaseType)
                ?? throw new NotSupportedException($"Database type '{databaseType}' not supported.");
            return new DbConnectionDescriptor(connectionName, databaseType, connectionString, provider);
        }
    }
}
