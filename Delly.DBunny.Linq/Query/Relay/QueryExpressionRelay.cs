using Delly.Modeling;
using Delly.DBunny.Linq.Dependency;
using Delly.DBunny.Linq.Compiler;

namespace Delly.DBunny.Linq.Query.Relay
{
    /// <summary>
    /// 查询表达式中继
    /// </summary>
    internal sealed class QueryExpressionRelay : IQueryExpressionRelay
    {
        private readonly IEntityModelFactory _entityModelFactory;
        private readonly IExpressionProvider _expressionProvider;
        private readonly System.Linq.Expressions.Expression _expression;
#if NET5_0_OR_GREATER
        private VariableBuilder? _variableBuilder;
#else
        private VariableBuilder _variableBuilder;
#endif
#if NET5_0_OR_GREATER
        private object? _relay;
#else
        private object _relay;
#endif

        /// <summary>
        /// 查询表达式中继
        /// </summary>
        public QueryExpressionRelay(
            IEntityModelFactory entityModelFactory,
            IExpressionProvider expressionProvider,
            System.Linq.Expressions.Expression expression)
        {
            _entityModelFactory = entityModelFactory;
            _expressionProvider = expressionProvider;
            _expression = expression;
        }

        /// <summary>
        /// 分析
        /// </summary>
        public void Analyse()
        {
            // TODO: 分析表达式树，构建查询中继（无反射方案）
            _relay = new object();
        }

        /// <summary>
        /// 设置变量构建器
        /// </summary>
        /// <param name="variable">变量构建器</param>
        public void SetVariable(VariableBuilder variable)
        {
            _variableBuilder = variable;
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns>查询中继</returns>
        public object Build() => _relay;
    }
}