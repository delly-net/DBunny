using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Delly.Modeling;
using Delly.DBunny.Linq.Query.Expression;

namespace Delly.DBunny.Linq.Query
{
    /// <summary>
    /// 实体可查询对象
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class EntityQueryable<T> : IOrderedQueryable<T> where T : class
    {
        /// <summary>
        /// 元素类型
        /// </summary>
        public Type ElementType => typeof(T);

        /// <summary>
        /// 表达式
        /// </summary>
        public System.Linq.Expressions.Expression Expression { get; }

        /// <summary>
        /// 查询提供者
        /// </summary>
        public IQueryProvider Provider { get; }

        /// <summary>
        /// 实体可查询对象
        /// </summary>
        /// <param name="provider">查询提供者</param>
        /// <param name="expression">表达式</param>
        public EntityQueryable(EntityQueryProvider provider, System.Linq.Expressions.Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        /// <summary>
        /// 实体可查询对象
        /// </summary>
        /// <param name="provider">查询提供者</param>
        /// <param name="entityModelFactory">实体模型工厂</param>
        public EntityQueryable(
            EntityQueryProvider provider,
            IEntityModelFactory entityModelFactory)
        {
            Provider = provider;
            var model = entityModelFactory.GetModel<T>();
            Expression = new QueryRootExpression(model);
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        public IEnumerator<T> GetEnumerator()
        {
            var list = Provider.Execute<IEnumerable<T>>(Expression);
            return list?.GetEnumerator() ?? Enumerable.Empty<T>().GetEnumerator();
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns>枚举器</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}