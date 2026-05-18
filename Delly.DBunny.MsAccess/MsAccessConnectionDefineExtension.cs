namespace Delly.DBunny.MsAccess
{
    /// <summary>
    /// MsAccess 连接定义扩展
    /// </summary>
    public static class MsAccessConnectionDefineExtension
    {
        /// <summary>
        /// 设置数据库文件路径
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="dataSource">数据库文件路径</param>
        /// <returns>当前实例</returns>
        public static MsAccessConnectionDefine WithDataSource(this MsAccessConnectionDefine define, string dataSource)
        {
            define.DataSource = dataSource;
            return define;
        }

        /// <summary>
        /// 设置数据库提供程序（使用主流版本 ACE 12.0 支持 .accdb）
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <returns>当前实例</returns>
        public static MsAccessConnectionDefine WithProviderAce(this MsAccessConnectionDefine define)
        {
            define.Provider = MsAccessConnectionDefine.PROVIDER_ACE;
            return define;
        }

        /// <summary>
        /// 设置数据库提供程序（使用 JET 4.0 支持 2003 版本 .mdb）
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <returns>当前实例</returns>
        public static MsAccessConnectionDefine WithProviderJet(this MsAccessConnectionDefine define)
        {
            define.Provider = MsAccessConnectionDefine.PROVIDER_JET;
            return define;
        }

        /// <summary>
        /// 设置数据库密码
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <param name="password">密码</param>
        /// <returns>当前实例</returns>
        public static MsAccessConnectionDefine WithPassword(this MsAccessConnectionDefine define, string password)
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
        public static MsAccessConnectionDefine WithConnectionTimeout(this MsAccessConnectionDefine define, int timeout)
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
        public static MsAccessConnectionDefine WithDefaultCommandTimeout(this MsAccessConnectionDefine define, int timeout)
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
        public static MsAccessConnectionDefine WithPooling(this MsAccessConnectionDefine define, bool pooling)
        {
            define.Pooling = pooling;
            return define;
        }

        /// <summary>
        /// 设置只读模式
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <returns>当前实例</returns>
        public static MsAccessConnectionDefine WithReadOnly(this MsAccessConnectionDefine define)
        {
            define.Mode = "Read";
            return define;
        }

        /// <summary>
        /// 设置读写模式
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <returns>当前实例</returns>
        public static MsAccessConnectionDefine WithReadWrite(this MsAccessConnectionDefine define)
        {
            define.Mode = "Read Write";
            return define;
        }

        /// <summary>
        /// 设置独占模式
        /// </summary>
        /// <param name="define">连接定义</param>
        /// <returns>当前实例</returns>
        public static MsAccessConnectionDefine WithExclusive(this MsAccessConnectionDefine define)
        {
            define.Mode = "Share Exclusive";
            return define;
        }
    }
}