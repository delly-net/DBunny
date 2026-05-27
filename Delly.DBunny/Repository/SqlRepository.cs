using System.Diagnostics.CodeAnalysis;
using Delly.DBunny;
using Delly.Modeling;
using System;

namespace Delly.DBunny.Repository
{
    /// <summary>
    /// Sql脚本仓库
    /// </summary>
    public sealed class SqlRepository : ISqlRepository
    {
        private readonly IEntityModelFactory _entityModelFactory;
        private readonly IDbManager _manager;

        /// <summary>
        /// Sql脚本仓库
        /// </summary>
        public SqlRepository(
            IEntityModelFactory entityModelFactory,
            IDbManager manager
            )
        {
            _entityModelFactory = entityModelFactory;
            _manager = manager;
        }

        /// <summary>
        /// 获取数据库作业
        /// </summary>
        /// <returns></returns>
        public IDbWork GetDbWork()
        {
            var work = _manager.GetCurrentWork() ?? throw new NullReferenceException($"Db work not found.");
            return work;
        }

        /// <summary>
        /// 获取建模
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEntityModel GetModel<T>()
            where T : class
        {
            return _entityModelFactory.GetModel<T>();
        }
    }
}
