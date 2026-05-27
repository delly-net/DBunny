using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Delly.Modeling;
using Delly.DBunny.Core;
using Delly.DBunny.Linq.Compiler;
using Delly.DBunny.Linq.Dependency;
using Delly.DBunny.Linq.Query.Expression;

namespace Delly.DBunny.Linq.Query
{
    /// <summary>
    /// 实体查询提供者
    /// </summary>
    public sealed class EntityQueryProvider : IQueryProvider
    {
        private readonly IDbProvider _dbProvider;
        private readonly IDbConnectionDefine _connection;
        private readonly IEntityModelFactory _entityModelFactory;
        private readonly IExpressionProvider _expressionProvider;

        /// <summary>
        /// 实体查询提供者
        /// </summary>
        /// <param name="dbProvider">数据库提供者</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="entityModelFactory">实体模型工厂</param>
        /// <param name="expressionProvider">表达式提供者</param>
        public EntityQueryProvider(
            IDbProvider dbProvider,
            IDbConnectionDefine connection,
            IEntityModelFactory entityModelFactory,
            IExpressionProvider expressionProvider)
        {
            _dbProvider = dbProvider;
            _connection = connection;
            _entityModelFactory = entityModelFactory;
            _expressionProvider = expressionProvider;
        }

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>可查询对象</returns>
        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            // TODO: 实现动态类型查询（需要非反射方案）
            throw new NotImplementedException();
        }

        /// <summary>
        /// 创建查询
        /// </summary>
        /// <typeparam name="TElement">元素类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>可查询对象</returns>
        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            // TODO: 实现动态类型查询（需要非反射方案）
            // EntityQueryable 有 class 约束，与 IQueryProvider 接口不匹配
            // 当前通过 QueryProviderExtension 使用，此方法留待后续实现
            throw new NotImplementedException("请使用 QueryProviderExtension.Query<T> 方法创建查询");
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>执行结果</returns>
        public object Execute(System.Linq.Expressions.Expression expression)
        {
            // TODO: 实现非泛型执行
            throw new NotImplementedException();
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>执行结果</returns>
        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            var variableBuilder = new VariableBuilder();
            var compiler = new ExpressionCompiler(_expressionProvider, _entityModelFactory, variableBuilder);
            var (sql, parameters) = compiler.Compile(expression);

            // 执行查询并返回结果
            var sqled = new Sqled(sql, parameters);
            var connectionString = _connection.ConnectionString;
#if NET5_0_OR_GREATER
            using var connection = _dbProvider.GetDbConnection(connectionString);
            using var command = _dbProvider.GetDbCommand(connection);
#else
            using (var connection = _dbProvider.GetDbConnection(connectionString))
            using (var command = _dbProvider.GetDbCommand(connection))
#endif
            {
                command.CommandText = sql;
                _dbProvider.SetParameters(command, parameters);

                return ExecuteQuery<TResult>(connection, sqled);
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="connection">数据库连接</param>
        /// <param name="sqled">SQL 命令</param>
        /// <returns>执行结果</returns>
        private TResult ExecuteQuery<TResult>(DbConnection connection, Sqled sqled)
        {
            // TODO: 实现查询执行逻辑
            throw new NotImplementedException();
        }
    }
}