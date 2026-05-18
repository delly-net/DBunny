using Delly.DBunny;
using Delly.DBunny.Core;
using System;
using System.Threading;

namespace Delly.DBunny.Working
{
    /// <summary>
    /// 数据库作业管理器
    /// </summary>
    public sealed class DefaultDbWorkManager : IDbManager
    {
        // 异步对象
        private static readonly AsyncLocal<DbWorkWrapper> _asyncLocal = new AsyncLocal<DbWorkWrapper>();
        // 互斥锁对象
        private static readonly object _lock = new object();

        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbFilterFactory _filterFactory;
        private readonly IDbProviderFactory _providerFactory;

        /// <summary>
        /// 数据库作业管理器
        /// </summary>
        public DefaultDbWorkManager(
            IDbConnectionFactory connectionFactory,
            IDbFilterFactory filterFactory,
            IDbProviderFactory providerFactory
            )
        {
            _connectionFactory = connectionFactory;
            _filterFactory = filterFactory;
            _providerFactory = providerFactory;
        }

        /// <summary>
        /// 连接描述器 工厂
        /// </summary>
        public IDbConnectionFactory ConnectionFactory => _connectionFactory;

        /// <summary>
        /// 提供程序 工厂
        /// </summary>
        public IDbProviderFactory ProviderFactory => _providerFactory;

        /// <summary>
        /// 创建一个数据库工作者
        /// </summary>
        /// <returns></returns>
        public IDbWork CreateWork(string connectionName)
        {
            var connectionDescriptor = _connectionFactory.GetConnection(connectionName);
            var provider = _providerFactory.GetProvider(connectionDescriptor.DatabaseType);
            if (provider is null) { throw new NotSupportedException($"Database type '{connectionDescriptor.DatabaseType}' not supported."); }
            var work = new DefaultDbWork(this, provider, connectionDescriptor, _filterFactory.GetFilters());
            SetCurrentWork(work);
            return work;
        }

        /// <summary>
        /// 获取当前工作者
        /// </summary>
        /// <returns></returns>
#if NETSTANDARD2_0
        public IDbWork GetCurrentWork()
        {
            if (_asyncLocal.Value is null) { return null; }
            return _asyncLocal.Value.Value;
        }
#else
        public IDbWork? GetCurrentWork()
        {
            return _asyncLocal.Value?.Value;
        }
#endif


        /// <summary>
        /// 释放工作者
        /// </summary>
        public void ReleaseWork()
        {
            if (_asyncLocal.Value?.Value is null) { return; }
            SetCurrentWork(null);
        }

        /// <summary>
        /// 设置当前工作者
        /// </summary>
        /// <param name="work"></param>
#if NETSTANDARD2_0
        public static void SetCurrentWork(IDbWork work)
#else
        public static void SetCurrentWork(IDbWork? work)
#endif
        {
            lock (_lock)
            {
                _asyncLocal.Value?.Value?.Dispose();
                if (work is null)
                {
                    _asyncLocal.Value = new DbWorkWrapper();
                }
                else
                {
                    _asyncLocal.Value = new DbWorkWrapper(work);
                }
            }
        }
    }
}


