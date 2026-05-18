using Delly.DBunny;
using Delly.DBunny.Working.Extension;
using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Delly.DBunny.Working.Extension
{
    /// <summary>
    /// 数据库作业扩展
    /// </summary>
    public static class DbWorkManagerExtension
    {
        /// <summary>
        /// 获取数据库命令管理器
        /// </summary>
        /// <returns></returns>
        public static IDbWork CreateWork(this IDbManager manager)
        {
            var connectionDescriptor = manager.ConnectionFactory.GetDefaultConnection();
            return manager.CreateWork(connectionDescriptor.Name);
        }
    }

}

