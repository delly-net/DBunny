namespace Delly.DBunny
{
    /// <summary>
    /// 数据库连接 描述器
    /// </summary>
    public class DbConnectionDescriptor : IDbConnectionDefine
    {
        /// <summary>
        /// 数据库连接 描述器
        /// </summary>
        /// <param name="name">连接名称</param>
        /// <param name="databaseType">数据库类型</param>
        /// <param name="connectionString">连接字符串</param>
        public DbConnectionDescriptor(string name, string databaseType, string connectionString)
        {
            Name = name;
            DatabaseType = databaseType;
            ConnectionString = connectionString;
        }

        /// <summary>
        /// 连接名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType { get; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        public string ConnectionString { get; }
    }
}


