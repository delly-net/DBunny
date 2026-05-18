using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据库提供程序工厂
    /// </summary>
    public interface IDbProviderFactory
    {
        /// <summary>
        /// 获取提供程序
        /// </summary>
        /// <param name="databaseType">数据库类型</param>
        /// <returns>数据库提供程序</returns>
#if NETSTANDARD2_0
        IDbProvider GetProvider(string databaseType);
#else
        IDbProvider? GetProvider(string databaseType);
#endif

        /// <summary>
        /// 获取数据库类型集合
        /// </summary>
        /// <returns>数据库类型集合</returns>
        IReadOnlyList<string> GetDatabaseTypes();

    }
}


