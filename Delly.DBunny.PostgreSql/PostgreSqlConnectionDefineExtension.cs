namespace Delly.DBunny.PostgreSql
{
    /// <summary>
    /// PostgreSql 连接定义扩展
    /// </summary>
    public static class PostgreSqlConnectionDefineExtension
    {
        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="host">服务器地址</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithHost(this PostgreSqlConnectionDefine define, string host)
        {
            define.Host = host;
            return define;
        }

        /// <summary>
        /// 设置服务器端口
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="port">端口</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithPort(this PostgreSqlConnectionDefine define, int port)
        {
            define.Port = port;
            return define;
        }

        /// <summary>
        /// 设置数据库名称
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="database">数据库名称</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithDatabase(this PostgreSqlConnectionDefine define, string database)
        {
            define.Database = database;
            return define;
        }

        /// <summary>
        /// 设置用户名
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="username">用户名</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithUsername(this PostgreSqlConnectionDefine define, string username)
        {
            define.Username = username;
            return define;
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="password">密码</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithPassword(this PostgreSqlConnectionDefine define, string password)
        {
            define.Password = password;
            return define;
        }

        /// <summary>
        /// 设置搜索路径（Schema列表）
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="searchPath">搜索路径</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithSearchPath(this PostgreSqlConnectionDefine define, string searchPath)
        {
            define.SearchPath = searchPath;
            return define;
        }

        /// <summary>
        /// 设置 SSL 模式
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="sslMode">SSL 模式</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithSslMode(this PostgreSqlConnectionDefine define, string sslMode)
        {
            define.SslMode = sslMode;
            return define;
        }

        /// <summary>
        /// 设置连接超时时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithTimeout(this PostgreSqlConnectionDefine define, int timeout)
        {
            define.Timeout = timeout;
            return define;
        }

        /// <summary>
        /// 设置默认命令超时时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithCommandTimeout(this PostgreSqlConnectionDefine define, int timeout)
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
        public static PostgreSqlConnectionDefine WithPooling(this PostgreSqlConnectionDefine define, bool pooling)
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
        public static PostgreSqlConnectionDefine WithMinPoolSize(this PostgreSqlConnectionDefine define, int minPoolSize)
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
        public static PostgreSqlConnectionDefine WithMaxPoolSize(this PostgreSqlConnectionDefine define, int maxPoolSize)
        {
            define.MaxPoolSize = maxPoolSize;
            return define;
        }

        /// <summary>
        /// 设置 Keepalive 间隔
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="keepalive">Keepalive 间隔（秒）</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithKeepalive(this PostgreSqlConnectionDefine define, int keepalive)
        {
            define.Keepalive = keepalive;
            return define;
        }

        /// <summary>
        /// 设置 Keepalive Idle 时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="keepaliveIdle">Keepalive Idle 时间（秒）</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithKeepaliveIdle(this PostgreSqlConnectionDefine define, int keepaliveIdle)
        {
            define.KeepaliveIdle = keepaliveIdle;
            return define;
        }

        /// <summary>
        /// 设置时区
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timezone">时区</param>
        /// <returns>当前实例</returns>
        public static PostgreSqlConnectionDefine WithTimezone(this PostgreSqlConnectionDefine define, string timezone)
        {
            define.Timezone = timezone;
            return define;
        }
    }
}