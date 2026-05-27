using System;

namespace Delly.DBunny.Linq.Query.Expression
{
    /// <summary>
    /// Select 表达式
    /// </summary>
    internal class QuerySelectExpression : System.Linq.Expressions.Expression
    {
        private readonly System.Linq.Expressions.Expression _source;
        private readonly System.Linq.Expressions.Expression _selector;

        /// <summary>
        /// Select 表达式
        /// </summary>
        public QuerySelectExpression(
            Type type,
            System.Linq.Expressions.Expression source,
            System.Linq.Expressions.Expression selector)
        {
            Type = type;
            _source = source;
            _selector = selector;
        }

        /// <summary>
        /// 源表达式
        /// </summary>
        public System.Linq.Expressions.Expression Source => _source;

        /// <summary>
        /// 选择器表达式
        /// </summary>
        public System.Linq.Expressions.Expression Selector => _selector;

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