using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delly.DBunny;
using Delly.Modeling;

namespace Delly.DBunny.Repository
{
    /// <summary>
    /// 仓库
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly IEntityModelFactory _entityModelFactory;
        private readonly IDbManager _manager;

        /// <summary>
        /// Sql脚本仓库
        /// </summary>
        public Repository(
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
        /// <returns></returns>
        public IEntityModel GetModel()
        {
            return _entityModelFactory.GetModel<T>();
        }
    }
}
