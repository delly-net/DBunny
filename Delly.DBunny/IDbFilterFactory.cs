using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据库 过滤器工厂
    /// </summary>
    public interface IDbFilterFactory
    {
        /// <summary>
        /// 获取过滤器集合
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDbFilter> GetFilters();
    }
}
