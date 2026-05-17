namespace Delly.DBunny.Oracle
{
    /// <summary>
    /// Oracle 连接定义扩展
    /// </summary>
    public static class OracleConnectionDefineExtension
    {
        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="dataSource">数据源</param>
        /// <returns>当前实例</returns>
        public static OracleConnectionDefine WithDataSource(this OracleConnectionDefine define, string dataSource)
        {
            define.DataSource = dataSource;
            return define;
        }

        /// <summary>
        /// 设置用户名
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="userId">用户名</param>
        /// <returns>当前实例</returns>
        public static OracleConnectionDefine WithUserId(this OracleConnectionDefine define, string userId)
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
        public static OracleConnectionDefine WithPassword(this OracleConnectionDefine define, string password)
        {
            define.Password = password;
            return define;
        }

        /// <summary>
        /// 设置连接超时时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>当前实例</returns>
        public static OracleConnectionDefine WithConnectionTimeout(this OracleConnectionDefine define, int timeout)
        {
            define.ConnectionTimeout = timeout;
            return define;
        }

        /// <summary>
        /// 设置连接池
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="pooling">是否启用连接池</param>
        /// <returns>当前实例</returns>
        public static OracleConnectionDefine WithPooling(this OracleConnectionDefine define, bool pooling)
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
        public static OracleConnectionDefine WithMinPoolSize(this OracleConnectionDefine define, int minPoolSize)
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
        public static OracleConnectionDefine WithMaxPoolSize(this OracleConnectionDefine define, int maxPoolSize)
        {
            define.MaxPoolSize = maxPoolSize;
            return define;
        }
    }
}