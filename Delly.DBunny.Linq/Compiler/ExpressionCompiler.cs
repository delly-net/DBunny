using System;
using System.Collections.Generic;
using System.Text;
using Delly.Modeling;
using Delly.DBunny.Linq.Query.Expression;
using Delly.DBunny.Linq.Dependency;

namespace Delly.DBunny.Linq.Compiler
{
    /// <summary>
    /// 表达式编译器，无反射实现
    /// </summary>
    internal sealed class ExpressionCompiler
    {
        private readonly IExpressionProvider _expressionProvider;
        private readonly IEntityModelFactory _entityModelFactory;
        private readonly VariableBuilder _variableBuilder;

        /// <summary>
        /// 表达式编译器
        /// </summary>
        public ExpressionCompiler(
            IExpressionProvider expressionProvider,
            IEntityModelFactory entityModelFactory,
            VariableBuilder variableBuilder)
        {
            _expressionProvider = expressionProvider;
            _entityModelFactory = entityModelFactory;
            _variableBuilder = variableBuilder;
        }

        /// <summary>
        /// 编译表达式为 SQL
        /// </summary>
        /// <param name="expression">LINQ 表达式</param>
        /// <returns>SQL 和参数</returns>
        public (string Sql, IReadOnlyDictionary<string, object> Parameters) Compile(
            System.Linq.Expressions.Expression expression)
        {
            var sqlBuilder = new StringBuilder();

            // 根据表达式类型进行编译
            switch (expression)
            {
                case QueryRootExpression rootExpr:
                    CompileRoot(rootExpr, sqlBuilder);
                    break;
                case QueryWhereExpression whereExpr:
                    CompileWhere(whereExpr, sqlBuilder);
                    break;
                case QuerySelectExpression selectExpr:
                    CompileSelect(selectExpr, sqlBuilder);
                    break;
                case QueryOrderByExpression orderByExpr:
                    CompileOrderBy(orderByExpr, sqlBuilder);
                    break;
                // TODO: 添加其他表达式类型的编译
                default:
                    throw new NotSupportedException($"不支持的表达式类型: {expression.GetType().Name}");
            }

            return (sqlBuilder.ToString(), _variableBuilder.GetParameters());
        }

        private void CompileRoot(QueryRootExpression rootExpr, StringBuilder sqlBuilder)
        {
            var model = rootExpr.Model;
            // TODO: 使用 Delly.Modeling 的正确 API 获取表名
            var tableName = _expressionProvider.GetName(model.GetType().Name);
            sqlBuilder.Append($"SELECT * FROM {tableName}");
        }

        private void CompileWhere(QueryWhereExpression whereExpr, StringBuilder sqlBuilder)
        {
            // 递归编译源表达式
            if (whereExpr.Source is QueryRootExpression rootExpr)
            {
                CompileRoot(rootExpr, sqlBuilder);
            }
            else
            {
                CompileExpressionToSql(whereExpr.Source, sqlBuilder);
            }

            // 编译谓词表达式
            var predicate = CompilePredicate(whereExpr.Predicate);
            sqlBuilder.Append($" WHERE {predicate}");
        }

        private void CompileSelect(QuerySelectExpression selectExpr, StringBuilder sqlBuilder)
        {
            // TODO: 实现投影编译
            CompileExpressionToSql(selectExpr.Source, sqlBuilder);
        }

        private void CompileOrderBy(QueryOrderByExpression orderByExpr, StringBuilder sqlBuilder)
        {
            // TODO: 实现排序编译
            CompileExpressionToSql(orderByExpr.Source, sqlBuilder);
        }

        private void CompileExpressionToSql(System.Linq.Expressions.Expression expression, StringBuilder sqlBuilder)
        {
            switch (expression)
            {
                case QueryRootExpression rootExpr:
                    CompileRoot(rootExpr, sqlBuilder);
                    break;
                case QueryWhereExpression whereExpr:
                    CompileWhere(whereExpr, sqlBuilder);
                    break;
                case QuerySelectExpression selectExpr:
                    CompileSelect(selectExpr, sqlBuilder);
                    break;
                case QueryOrderByExpression orderByExpr:
                    CompileOrderBy(orderByExpr, sqlBuilder);
                    break;
                default:
                    throw new NotSupportedException($"不支持的表达式类型: {expression.GetType().Name}");
            }
        }

        private string CompilePredicate(System.Linq.Expressions.Expression expression)
        {
            // 简化的谓词编译（TODO: 完整实现）
            if (expression is System.Linq.Expressions.BinaryExpression binaryExpr)
            {
                var left = CompileExpression(binaryExpr.Left);
                var right = CompileExpression(binaryExpr.Right);
                var op = GetBinaryOperator(binaryExpr.NodeType);
                return $"{left} {op} {right}";
            }

            throw new NotSupportedException($"不支持的谓词表达式: {expression.GetType().Name}");
        }

        private string CompileExpression(System.Linq.Expressions.Expression expression)
        {
            var memberExpr = expression as System.Linq.Expressions.MemberExpression;
            if (memberExpr != null)
            {
                return _expressionProvider.GetName(memberExpr.Member.Name);
            }

            var constExpr = expression as System.Linq.Expressions.ConstantExpression;
            if (constExpr != null)
            {
                var paramName = _variableBuilder.AddParameter(constExpr.Value);
                return $"{_expressionProvider.ParameterPrefix}{paramName}";
            }

            var unaryExpr = expression as System.Linq.Expressions.UnaryExpression;
            if (unaryExpr != null && unaryExpr.NodeType == System.Linq.Expressions.ExpressionType.Convert)
            {
                return CompileExpression(unaryExpr.Operand);
            }

            throw new NotSupportedException($"不支持的表达式: {expression.GetType().Name}");
        }

        private string GetBinaryOperator(System.Linq.Expressions.ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case System.Linq.Expressions.ExpressionType.Equal:
                    return "=";
                case System.Linq.Expressions.ExpressionType.NotEqual:
                    return "<>";
                case System.Linq.Expressions.ExpressionType.GreaterThan:
                    return ">";
                case System.Linq.Expressions.ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case System.Linq.Expressions.ExpressionType.LessThan:
                    return "<";
                case System.Linq.Expressions.ExpressionType.LessThanOrEqual:
                    return "<=";
                case System.Linq.Expressions.ExpressionType.AndAlso:
                    return "AND";
                case System.Linq.Expressions.ExpressionType.OrElse:
                    return "OR";
                default:
                    throw new NotSupportedException($"不支持的二元操作符: {nodeType}");
            }
        }
    }
}