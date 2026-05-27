using Delly.Modeling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny
{
    /// <summary>
    /// SQL仓库
    /// </summary>
    public interface ISqlRepository
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
        IEntityModel GetModel<T>() where T : class;
    }
}


