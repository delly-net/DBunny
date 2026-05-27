namespace Delly.DBunny.Linq.Dependency
{
    /// <summary>
    /// 表达式中继接口
    /// </summary>
    public interface IQueryExpressionRelay
    {
        /// <summary>
        /// 设置变量构建器
        /// </summary>
        /// <param name="variable">变量构建器</param>
        void SetVariable(Compiler.VariableBuilder variable);

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns>查询中继</returns>
        object Build();
    }
}