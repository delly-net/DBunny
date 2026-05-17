using System;
using System.Collections.Generic;
using System.Text;

namespace Delly.DBunny
{
    /// <summary>
    /// 数据列描述器
    /// </summary>
    public class DbColumnDesciptor : DbTableDesciptor
    {
        /// <summary>
        /// 列名称
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 列类型
        /// </summary>
        public string ColumnType { get; set; } = string.Empty;

        /// <summary>
        /// 主键标识
        /// </summary>
        public bool PrimaryKeyFlag { get; set; }

        /// <summary>
        /// 可空标识
        /// </summary>
        public bool NullableFlag { get; set; }
    }
}
