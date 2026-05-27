using Delly.Modeling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny
{
    /// <summary>
    /// 仓库
    /// </summary>
    public interface IRepository<T>
    {
        /// <summary>
        /// 获取数据库作业
        /// </summary>
        /// <returns></returns>
        IDbWork GetDbWork();

        /// <summary>
        /// 获取数据库作业
        /// </summary>
        /// <returns></returns>
        IEntityModel GetModel();
    }
}


