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
    /// 空仓库
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class NullRepository<T> : IRepository<T>
    {
        private static readonly NullRepository<T> _nullRepository = new NullRepository<T>();

        /// <summary>
        /// 空仓库实例
        /// </summary>
        public static NullRepository<T> Instance => _nullRepository;

        /// <summary>
        /// 获取数据库作业
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IDbWork GetDbWork()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取建模
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEntityModel GetModel()
        {
            throw new NotImplementedException();
        }
    }
}
