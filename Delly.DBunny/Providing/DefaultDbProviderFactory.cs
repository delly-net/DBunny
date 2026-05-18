using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny.Providing
{
    /// <summary>
    /// 默认数据库工厂
    /// </summary>
    public sealed class DefaultDbProviderFactory : IDbProviderFactory
    {
        private readonly Dictionary<string, IDbProvider> _providers;

        /// <summary>
        /// 默认数据库工厂
        /// </summary>
        /// <param name="providers">数据库提供程序集合</param>
        public DefaultDbProviderFactory(params IDbProvider[] providers)
        {
            _providers = new Dictionary<string, IDbProvider>();
            Initialize(providers);
        }

        /// <summary>
        /// 附加
        /// </summary>
        /// <param name="provider">数据库提供程序</param>
        public void Append(IDbProvider provider)
        {
            Register(provider);
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _providers.Clear();
        }

        /// <summary>
        /// 获取数据库类型集合
        /// </summary>
        /// <returns>数据库类型集合</returns>
        public IReadOnlyList<string> GetDatabaseTypes()
        {
            return _providers.Keys.ToArray();
        }

        /// <summary>
        /// 获取提供程序
        /// </summary>
        /// <param name="databaseType">数据库类型</param>
        /// <returns>数据库提供程序</returns>
#if NETSTANDARD2_0
        public IDbProvider GetProvider(string databaseType)
#else
        public IDbProvider? GetProvider(string databaseType)
#endif
        {
#if NETSTANDARD2_0
            if (_providers.TryGetValue(databaseType, out IDbProvider provider)) { return provider; }
#else
            if (_providers.TryGetValue(databaseType, out IDbProvider? provider)) { return provider; }
#endif

            return null;
        }

        // 初始化
        private void Initialize(IEnumerable<IDbProvider> providers)
        {
            foreach (var provider in providers)
            {
                Register(provider);
            }
        }

        // 注册
        private void Register(IDbProvider provider)
        {
            _providers[provider.DatabaseType] = provider;
        }
    }
}


