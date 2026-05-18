using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据库 过滤器
    /// </summary>
    public interface IDbFilter
    {
        /// <summary>
        /// Sql装载
        /// </summary>
        /// <returns></returns>
        Sqled SqlLoading(Sqled sqled);

        /// <summary>
        /// 创建数据库命令
        /// </summary>
        /// <returns></returns>
#if NETSTANDARD2_0
        DbCommand CommandCreating(DbCommand command);
#else
        DbCommand? CommandCreating(DbCommand? command);
#endif

        /// <summary>
        /// 创建数据库命令
        /// </summary>
        /// <returns></returns>
        DbCommand CommandExecuting(DbCommand command);
    }
}


