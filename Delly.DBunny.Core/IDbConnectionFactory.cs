namespace Delly.DBunny
{
    /// <summary>
    /// 主机数据库上下文配置工厂
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// 获取主机数据库上下文配置
        /// </summary>
        /// <param name="name">连接名称</param>
        /// <returns>连接描述符</returns>
        DbConnectionDescriptor GetConnection(string name);

        /// <summary>
        /// 默认主机数据库上下文配置
        /// </summary>
        /// <returns>默认连接描述符</returns>
        DbConnectionDescriptor GetDefaultConnection();
    }
}


