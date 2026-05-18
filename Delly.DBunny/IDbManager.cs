using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据库 管理器
    /// </summary>
    public interface IDbManager
    {
        /// <summary>
        /// 连接描述器工厂
        /// </summary>
        IDbConnectionFactory ConnectionFactory { get; }

        /// <summary>
        /// 提供程序工厂
        /// </summary>
        IDbProviderFactory ProviderFactory { get; }

        /// <summary>
        /// 创建一个新的作业
        /// </summary>
        /// <returns></returns>
        IDbWork CreateWork(string connectionName);

        /// <summary>
        /// 获取当前作业
        /// </summary>
        /// <returns></returns>
#if NETSTANDARD2_0
        IDbWork GetCurrentWork();
#else
        IDbWork? GetCurrentWork();
#endif

        /// <summary>
        /// 释放当前作业
        /// </summary>
        void ReleaseWork();
    }
}


