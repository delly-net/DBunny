using Delly.Modeling;
using Delly.DBunny.Core;

namespace Delly.DBunny.Linq.Extension
{
    /// <summary>
    /// 查询提供者扩展
    /// </summary>
    public static class QueryProviderExtension
    {
        /// <summary>
        /// 创建可查询集合
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dbProvider">数据库提供者</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="entityModelFactory">实体模型工厂</param>
        /// <param name="expressionProvider">表达式提供者</param>
        /// <returns>可查询集合</returns>
        public static Query.EntityQueryable<T> Query<T>(this IDbProvider dbProvider,
            IDbConnectionDefine connection,
            IEntityModelFactory entityModelFactory,
            Dependency.IExpressionProvider expressionProvider)
            where T : class
        {
            var provider = new Query.EntityQueryProvider(
                dbProvider,
                connection,
                entityModelFactory,
                expressionProvider);
            return new Query.EntityQueryable<T>(provider, entityModelFactory);
        }
    }
}