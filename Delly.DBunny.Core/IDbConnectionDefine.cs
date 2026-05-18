using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny.Core
{
    /// <summary>
    /// 数据库连接定义
    /// </summary>
    public interface IDbConnectionDefine
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        string ConnectionString { get; }
    }
}


