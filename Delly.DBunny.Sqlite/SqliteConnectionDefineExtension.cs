namespace Delly.DBunny.Sqlite
{
    /// <summary>
    /// Sqlite 连接定义扩展
    /// </summary>
    public static class SqliteConnectionDefineExtension
    {
        /// <summary>
        /// 设置数据库文件路径
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="dataSource">数据库文件路径</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithDataSource(this SqliteConnectionDefine define, string dataSource)
        {
            define.DataSource = dataSource;
            return define;
        }

        /// <summary>
        /// 设置数据库密码
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="password">密码</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithPassword(this SqliteConnectionDefine define, string password)
        {
            define.Password = password;
            return define;
        }

        /// <summary>
        /// 设置页大小
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithPageSize(this SqliteConnectionDefine define, int pageSize)
        {
            define.PageSize = pageSize;
            return define;
        }

        /// <summary>
        /// 设置缓存大小
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="cacheSize">缓存页数</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithCacheSize(this SqliteConnectionDefine define, int cacheSize)
        {
            define.CacheSize = cacheSize;
            return define;
        }

        /// <summary>
        /// 设置默认超时时间
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithDefaultTimeout(this SqliteConnectionDefine define, int timeout)
        {
            define.DefaultTimeout = timeout;
            return define;
        }

        /// <summary>
        /// 设置连接池
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="pooling">是否启用连接池</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithPooling(this SqliteConnectionDefine define, bool pooling)
        {
            define.Pooling = pooling;
            return define;
        }

        /// <summary>
        /// 设置外键约束
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="foreignKeys">是否启用外键约束</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithForeignKeys(this SqliteConnectionDefine define, bool foreignKeys)
        {
            define.ForeignKeys = foreignKeys;
            return define;
        }

        /// <summary>
        /// 设置只读模式
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="readOnly">是否只读</param>
        /// <returns>当前实例</returns>
        public static SqliteConnectionDefine WithReadOnly(this SqliteConnectionDefine define, bool readOnly)
        {
            define.ReadOnly = readOnly;
            return define;
        }
    }
}