using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny
{
    /// <summary>
    /// Sql异常
    /// </summary>
    public class SqlException : Exception
    {
        /// <summary>
        /// Sql异常
        /// </summary>
        /// <param name="sqled"></param>
        /// <param name="innerException"></param>
        public SqlException(Sqled sqled, Exception innerException) : base($"{innerException.Message}, {sqled}", innerException)
        {
            Sqled = sqled;
        }

        /// <summary>
        /// SqlSet
        /// </summary>
        public Sqled Sqled { get; }
    }

}

