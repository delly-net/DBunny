using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Core
{
    /// <summary>
    /// 数据表 描述器
    /// </summary>
    public class DbTableDesciptor
    {
        /// <summary>
        /// Schema名称
        /// </summary>
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; } = string.Empty;
    }
}
