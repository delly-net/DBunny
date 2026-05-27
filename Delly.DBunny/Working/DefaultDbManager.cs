using Delly.DBunny;
using Delly.DBunny.Core;
using Delly.Modeling;
using System;
using System.Threading;

namespace Delly.DBunny.Working
{
    /// <summary>
    /// 数据库作业管理器
    /// </summary>
    public class DefaultDbManager : IDbManager
    {
        // 异步对象
        private static readonly AsyncLocal<DbWorkWrapper> _asyncLocal = new AsyncLocal<DbWorkWrapper>();
        // 互斥锁对象
        private static readonly object _lock = new object();

        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbFilterFactory _filterFactory;
        private readonly IDbProviderFactory _providerFactory;
        private readonly IEntityModelFactory _entityModelFactory;

        /// <summary>
        /// 数据库作业管理器
        /// </summary>
        public DefaultDbManager(
            IDbConnectionFactory connectionFactory,
            IDbFilterFactory filterFactory,
            IDbProviderFactory providerFactory,
            IEntityModelFactory entityModelFactory
            )
        {
            _connectionFactory = connectionFactory;
            _filterFactory = filterFactory;
            _providerFactory = providerFactory;
            _entityModelFactory = entityModelFactory;
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
        /// 实体建模工厂
        /// </summary>
        public IEntityModelFactory EntityModelFactory => _entityModelFactory;

        /// <summary>
        /// 创建一个数据库工作者
        /// </summary>
        /// <returns></returns>
        public IDbWork CreateWork(string connectionName)
        {
            var connectionDescriptor = _connectionFactory.GetConnection(connectionName);
            var provider = _providerFactory.GetProvider(connectionDescriptor.DatabaseType);
            if (provider is null) { throw new NotSupportedException($"Database type '{connectionDescriptor.DatabaseType}' not supported."); }
            var work = new DefaultDbWork(this, provider, _entityModelFactory, connectionDescriptor, _filterFactory.GetFilters());
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


