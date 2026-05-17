namespace Delly.DBunny.MySql
{
    /// <summary>
    /// MySql 连接定义扩展
    /// </summary>
    public static class MySqlConnectionDefineExtension
    {
        /// <summary>
        /// 设置服务器地址
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="server">服务器地址</param>
        /// <returns>当前实例</returns>
        public static MySqlConnectionDefine WithServer(this MySqlConnectionDefine define, string server)
        {
            define.Server = server;
            return define;
        }

        /// <summary>
        /// 设置服务器端口
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="port">端口</param>
        /// <returns>当前实例</returns>
        public static MySqlConnectionDefine WithPort(this MySqlConnectionDefine define, int port)
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
        public static MySqlConnectionDefine WithDatabase(this MySqlConnectionDefine define, string database)
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
        public static MySqlConnectionDefine WithUserId(this MySqlConnectionDefine define, string userId)
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
        public static MySqlConnectionDefine WithPassword(this MySqlConnectionDefine define, string password)
        {
            define.Password = password;
            return define;
        }

        /// <summary>
        /// 设置字符集
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="charset">字符集</param>
        /// <returns>当前实例</returns>
        public static MySqlConnectionDefine WithCharset(this MySqlConnectionDefine define, string charset)
        {
            define.Charset = charset;
            return define;
        }

        /// <summary>
        /// 设置 SSL 模式
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="sslMode">SSL 模式</param>
        /// <returns>当前实例</returns>
        public static MySqlConnectionDefine WithSslMode(this MySqlConnectionDefine define, string sslMode)
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
        public static MySqlConnectionDefine WithConnectionTimeout(this MySqlConnectionDefine define, int timeout)
        {
            define.ConnectionTimeout = timeout;
            return define;
        }

        /// <summary>
        /// 设置默认命令超时时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>当前实例</returns>
        public static MySqlConnectionDefine WithDefaultCommandTimeout(this MySqlConnectionDefine define, int timeout)
        {
            define.DefaultCommandTimeout = timeout;
            return define;
        }

        /// <summary>
        /// 设置连接池
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="pooling">是否启用连接池</param>
        /// <returns>当前实例</returns>
        public static MySqlConnectionDefine WithPooling(this MySqlConnectionDefine define, bool pooling)
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
        public static MySqlConnectionDefine WithMinPoolSize(this MySqlConnectionDefine define, int minPoolSize)
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
        public static MySqlConnectionDefine WithMaxPoolSize(this MySqlConnectionDefine define, int maxPoolSize)
        {
            define.MaxPoolSize = maxPoolSize;
            return define;
        }
    }
}