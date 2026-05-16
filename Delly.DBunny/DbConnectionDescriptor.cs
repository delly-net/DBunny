namespace Delly.DBunny
{
    /// <summary>
    /// 数据库连接 描述器
    /// </summary>
    /// <remarks>
    /// 数据库连接 描述器
    /// </remarks>
#if NET8_0
    public class DbConnectionDescriptor(string name, string databaseType, string connectionString, IDbProvider provider) : IDbConnectionDefine
    {

        /// <summary>
        /// 连接名称
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// 数据库提供程序
        /// </summary>
        public IDbProvider Provider { get; } = provider;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType { get; } = databaseType;

        /// <summary>
        /// 数据库连接
        /// </summary>
        public string ConnectionString { get; } = connectionString;
    }
#else
    public class DbConnectionDescriptor : IDbConnectionDefine
    {
        /// <summary>
        /// 数据库连接 描述器
        /// </summary>
        public DbConnectionDescriptor(string name, string databaseType, string connectionString, IDbProvider provider)
        {
            Name = name;
            DatabaseType = databaseType;
            ConnectionString = connectionString;
            Provider = provider;
        }

        /// <summary>
        /// 连接名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 数据库提供程序
        /// </summary>
        public IDbProvider Provider { get; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DatabaseType { get; }

        /// <summary>
        /// 数据库连接
        /// </summary>
        public string ConnectionString { get; }
    }
#endif
}


