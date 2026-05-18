using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Delly.DBunny.Filting
{
    /// <summary>
    /// 默认数据库 过滤器工厂
    /// </summary>
    public class DefaultDbFilterFactory : IDbFilterFactory
    {
        private readonly List<IDbFilter> _filters;

        /// <summary>
        /// 默认数据库 过滤器工厂
        /// </summary>
        public DefaultDbFilterFactory(params IDbFilter[] filters)
        {
            _filters = new List<IDbFilter>(filters);
        }

        /// <summary>
        /// 获取过滤器集合
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDbFilter> GetFilters()
        {
            return _filters.ToArray();
        }
    }
}


