using Delly.DBunny;

namespace Delly.DBunny.Repository.Extension
{
    /// <summary>
    /// 数据库作业扩展
    /// </summary>
    public static class DbWorkExtension
    {
        /// <summary>
        /// 获取Sql仓库
        /// </summary>
        /// <returns></returns>
        public static ISqlRepository GetSqlRepository(this IDbWork work)
        {
            return new SqlRepository(work.EntityModelFactory, work.Manager);
        }

        /// <summary>
        /// 获取Sql仓库
        /// </summary>
        /// <returns></returns>
        public static IRepository<T> GetRepository<T>(this IDbWork work)
        {
            return new Repository<T>(work.EntityModelFactory, work.Manager);
        }
    }
}
