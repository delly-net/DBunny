using System;
using Delly.Modeling;

namespace Delly.DBunny.Linq.Query.Expression
{
    /// <summary>
    /// Where 表达式
    /// </summary>
    internal class QueryWhereExpression : System.Linq.Expressions.Expression
    {
        private readonly IEntityModel _model;
        private readonly System.Linq.Expressions.Expression _source;
        private readonly System.Linq.Expressions.Expression _predicate;

        /// <summary>
        /// Where 表达式
        /// </summary>
        public QueryWhereExpression(
            Type type,
            IEntityModel model,
            System.Linq.Expressions.Expression source,
            System.Linq.Expressions.Expression predicate)
        {
            Type = type;
            _model = model;
            _source = source;
            _predicate = predicate;
        }

        /// <summary>
        /// 实体模型
        /// </summary>
        public IEntityModel Model => _model;

        /// <summary>
        /// 源表达式
        /// </summary>
        public System.Linq.Expressions.Expression Source => _source;

        /// <summary>
        /// 谓词表达式
        /// </summary>
        public System.Linq.Expressions.Expression Predicate => _predicate;

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