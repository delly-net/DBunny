using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny.Core
{
    /// <summary>
    /// 数据索引描述器
    /// </summary>
    public class DbIndexDesciptor : DbTableDesciptor
    {
        /// <summary>
        /// 索引名称
        /// </summary>
        public string IndexName { get; set; } = string.Empty;

        /// <summary>
        /// 唯一标识
        /// </summary>
        public bool UniqueFlag { get; set; }

        /// <summary>
        /// 列名称（用于创建索引）
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;
    }
}
