using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据库 映射器
    /// </summary>
    public interface IDbMapper<T>
    {
        /// <summary>
        /// 从 DataReader 映射对象
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        T Map(DbDataReader reader);
#else
        T? Map(DbDataReader reader);
#endif
    }
}
