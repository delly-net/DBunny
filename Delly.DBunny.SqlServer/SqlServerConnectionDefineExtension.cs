namespace Delly.DBunny.SqlServer
{
    /// <summary>
    /// SQL Server 连接定义扩展
    /// </summary>
    public static class SqlServerConnectionDefineExtension
    {
        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="server">服务器地址</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithServer(this SqlServerConnectionDefine define, string server)
        {
            define.Server = server;
            return define;
        }

        /// <summary>
        /// 设置数据库名称
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="database">数据库名称</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithDatabase(this SqlServerConnectionDefine define, string database)
        {
            define.Database = database;
            return define;
        }

        /// <summary>
        /// 设置用户名
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="userId">用户名</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithUserId(this SqlServerConnectionDefine define, string userId)
        {
            define.UserId = userId;
            return define;
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="password">密码</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithPassword(this SqlServerConnectionDefine define, string password)
        {
            define.Password = password;
            return define;
        }

        /// <summary>
        /// 设置是否信任服务器证书
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="trustServerCertificate">是否信任</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithTrustServerCertificate(this SqlServerConnectionDefine define, bool trustServerCertificate)
        {
            define.TrustServerCertificate = trustServerCertificate;
            return define;
        }

        /// <summary>
        /// 设置是否加密连接
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="encrypt">是否加密</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithEncrypt(this SqlServerConnectionDefine define, bool encrypt)
        {
            define.Encrypt = encrypt;
            return define;
        }

        /// <summary>
        /// 设置连接超时时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithConnectionTimeout(this SqlServerConnectionDefine define, int timeout)
        {
            define.ConnectionTimeout = timeout;
            return define;
        }

        /// <summary>
        /// 设置命令超时时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithCommandTimeout(this SqlServerConnectionDefine define, int timeout)
        {
            define.CommandTimeout = timeout;
            return define;
        }

        /// <summary>
        /// 设置连接池
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="pooling">是否启用连接池</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithPooling(this SqlServerConnectionDefine define, bool pooling)
        {
            define.Pooling = pooling;
            return define;
        }

        /// <summary>
        /// 设置最小连接池大小
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="minPoolSize">最小连接池大小</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithMinPoolSize(this SqlServerConnectionDefine define, int minPoolSize)
        {
            define.MinPoolSize = minPoolSize;
            return define;
        }

        /// <summary>
        /// 设置最大连接池大小
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="maxPoolSize">最大连接池大小</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithMaxPoolSize(this SqlServerConnectionDefine define, int maxPoolSize)
        {
            define.MaxPoolSize = maxPoolSize;
            return define;
        }

        /// <summary>
        /// 设置是否启用多活动结果集（MARS）
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="mars">是否启用 MARS</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithMultipleActiveResultSets(this SqlServerConnectionDefine define, bool mars)
        {
            define.MultipleActiveResultSets = mars;
            return define;
        }

        /// <summary>
        /// 设置数据包大小
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="packetSize">数据包大小（字节）</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithPacketSize(this SqlServerConnectionDefine define, int packetSize)
        {
            define.PacketSize = packetSize;
            return define;
        }

        /// <summary>
        /// 设置应用程序名称
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="applicationName">应用程序名称</param>
        /// <returns>当前实例</returns>
        public static SqlServerConnectionDefine WithApplicationName(this SqlServerConnectionDefine define, string applicationName)
        {
            define.ApplicationName = applicationName;
            return define;
        }
    }
}