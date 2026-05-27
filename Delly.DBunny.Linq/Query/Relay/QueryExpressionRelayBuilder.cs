using Delly.Modeling;
using Delly.DBunny.Linq.Dependency;

namespace Delly.DBunny.Linq.Query.Relay
{
    /// <summary>
    /// 查询表达式中继构建器
    /// </summary>
    public static class QueryExpressionRelayBuilder
    {
        /// <summary>
        /// 创建表达式中继
        /// </summary>
        /// <param name="entityModelFactory">实体模型工厂</param>
        /// <param name="provider">表达式提供者</param>
        /// <param name="expression">表达式</param>
        /// <returns>表达式中继</returns>
        public static IQueryExpressionRelay Build(
            IEntityModelFactory entityModelFactory,
            IExpressionProvider provider,
            System.Linq.Expressions.Expression expression)
        {
            var relay = new QueryExpressionRelay(entityModelFactory, provider, expression);
            relay.Analyse();
            return relay;
        }
    }
}