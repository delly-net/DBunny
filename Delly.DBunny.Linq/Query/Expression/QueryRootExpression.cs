using System;
using System.Collections.Generic;
using Delly.Modeling;

namespace Delly.DBunny.Linq.Query.Expression
{
    /// <summary>
    /// 查询根表达式
    /// </summary>
    internal class QueryRootExpression : System.Linq.Expressions.Expression
    {
        private readonly IEntityModel _model;

        /// <summary>
        /// 查询根表达式
        /// </summary>
        /// <param name="model">实体模型</param>
        public QueryRootExpression(IEntityModel model)
        {
            _model = model;
        }

        /// <summary>
        /// 实体模型
        /// </summary>
        public IEntityModel Model => _model;

        /// <summary>
        /// 表达式类型
        /// </summary>
        public override Type Type => typeof(IEnumerable<>);

        /// <summary>
        /// 节点类型
        /// </summary>
        public override System.Linq.Expressions.ExpressionType NodeType =>
            System.Linq.Expressions.ExpressionType.Extension;
    }
}