using System;

namespace Delly.DBunny.Linq.Query.Expression
{
    /// <summary>
    /// OrderByDescending 表达式
    /// </summary>
    internal class QueryOrderByDescendingExpression : System.Linq.Expressions.Expression
    {
        private readonly System.Linq.Expressions.Expression _source;
        private readonly System.Linq.Expressions.Expression _keySelector;

        /// <summary>
        /// OrderByDescending 表达式
        /// </summary>
        public QueryOrderByDescendingExpression(
            Type type,
            System.Linq.Expressions.Expression source,
            System.Linq.Expressions.Expression keySelector)
        {
            Type = type;
            _source = source;
            _keySelector = keySelector;
        }

        /// <summary>
        /// 源表达式
        /// </summary>
        public System.Linq.Expressions.Expression Source => _source;

        /// <summary>
        /// 键选择器表达式
        /// </summary>
        public System.Linq.Expressions.Expression KeySelector => _keySelector;

        /// <summary>
        /// 表达式类型
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public override System.Linq.Expressions.ExpressionType NodeType =>
            System.Linq.Expressions.ExpressionType.Extension;
    }
}