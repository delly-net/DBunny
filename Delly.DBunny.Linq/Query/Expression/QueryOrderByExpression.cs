using System;

namespace Delly.DBunny.Linq.Query.Expression
{
    /// <summary>
    /// OrderBy 表达式
    /// </summary>
    internal class QueryOrderByExpression : System.Linq.Expressions.Expression
    {
        private readonly System.Linq.Expressions.Expression _source;
        private readonly System.Linq.Expressions.Expression _keySelector;
        private readonly bool _descending;

        /// <summary>
        /// OrderBy 表达式
        /// </summary>
        public QueryOrderByExpression(
            Type type,
            System.Linq.Expressions.Expression source,
            System.Linq.Expressions.Expression keySelector,
            bool descending = false)
        {
            Type = type;
            _source = source;
            _keySelector = keySelector;
            _descending = descending;
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
        /// 是否降序
        /// </summary>
        public bool Descending => _descending;

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