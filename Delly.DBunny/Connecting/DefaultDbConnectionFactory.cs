using Delly.DBunny.Core;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Delly.DBunny.Connecting
{
    /// <summary>
    /// 主机数据库上下文配置工厂
    /// </summary>
    public class DefaultDbConnectionFactory : IDbConnectionFactory
    {

        #region DI注入

        private readonly Dictionary<string, DbConnectionDescriptor> _descriptors;

        /// <summary>
        /// 主机数据库上下文配置工厂
        /// </summary>
        /// <param name="providers">数据库连接描述符集合</param>
        public DefaultDbConnectionFactory(
            params DbConnectionDescriptor[] providers
            )
        {
            _descriptors = providers.ToDictionary(d => d.Name);
        }
        #endregion

        /// <summary>
        /// 获取默认连接
        /// </summary>
        /// <returns>默认连接描述符</returns>
        public DbConnectionDescriptor GetDefaultConnection()
        {
            if (_descriptors.Count <= 0) { throw new KeyNotFoundException($"No database connection."); }
#if NETSTANDARD2_0
            if (_descriptors.TryGetValue("Default", out DbConnectionDescriptor descriptor)) { return descriptor; }
#else
            if (_descriptors.TryGetValue("Default", out DbConnectionDescriptor? descriptor)) { return descriptor; }
#endif
            return _descriptors.First().Value;
        }

        /// <summary>
        /// 获取连接描述
        /// </summary>
        /// <param name="name">连接名称</param>
        /// <returns>连接描述符</returns>
        public DbConnectionDescriptor GetConnection(string name)
        {
#if NETSTANDARD2_0
            if (_descriptors.TryGetValue(name, out DbConnectionDescriptor descriptor)) { return descriptor; }
#else
            if (_descriptors.TryGetValue(name, out DbConnectionDescriptor? descriptor)) { return descriptor; }
#endif
            throw new KeyNotFoundException($"Database connection '{name}' not found.");
        }
    }
}


