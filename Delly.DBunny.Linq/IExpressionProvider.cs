namespace Delly.DBunny.Linq.Dependency
{
    /// <summary>
    /// 表达式提供者接口
    /// </summary>
    public interface IExpressionProvider
    {
        /// <summary>
        /// 获取数据库特定名称
        /// </summary>
        /// <param name="name">原始名称</param>
        /// <returns>数据库特定名称</returns>
        string GetName(string name);

        /// <summary>
        /// 获取参数名前缀
        /// </summary>
        string ParameterPrefix { get; }
    }
}