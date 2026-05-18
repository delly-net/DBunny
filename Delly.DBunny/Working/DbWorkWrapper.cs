using Delly.DBunny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny.Working
{
    /// <summary>
    /// 数据库作业包裹层
    /// </summary>
    public sealed class DbWorkWrapper
    {
        /// <summary>
        /// 数据库作业包裹层
        /// </summary>
        public DbWorkWrapper() { }

        /// <summary>
        /// 数据库作业包裹层
        /// </summary>
        /// <param name="value"></param>
        public DbWorkWrapper(IDbWork value)
        {
            Value = value;
        }

        /// <summary>
        /// 数据库作业
        /// </summary>
#if NETSTANDARD2_0
        public IDbWork Value { get; }
#else
        public IDbWork? Value { get; }
#endif

    }

}

