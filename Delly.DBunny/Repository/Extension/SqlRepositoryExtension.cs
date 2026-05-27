using Delly.DBunny;
using Delly.DBunny.Core;
using Delly.DBunny.Linq.Query;
using Delly.DBunny.Reading.Extension;
using Delly.Modeling;
using Eazy.Data.Work.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Delly.DBunny.Repository.Extension
{
    /// <summary>
    /// Sql 仓库助手
    /// </summary>
    public static partial class SqlRepositoryExtension
    {
        /// <summary>
        /// 执行Sql语句
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static void ExecuteNonQuery(this ISqlRepository repository, Sqled sqled)
        {
            var work = repository.GetDbWork();
            work.Sqleds.Add(sqled);
        }

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(this ISqlRepository repository, Sqled sqled)
        {
            var work = repository.GetDbWork();
            var dbProvider = work.Provider;
            using (var conn = work.Connect())
            {
                using (var command = work.GetSqlCommand(conn, sqled))
                {
                    return dbProvider.GetDataSet(command);
                }
            }
        }

        /// <summary>
        /// 执行原始数据读取
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(this ISqlRepository repository, Sqled sqled)
        {
            var dataSet = repository.GetDataSet(sqled);
            if (dataSet is null) return new DataTable();
            return dataSet.Tables[0];
        }

        /// <summary>
        /// 执行原始数据读取
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <param name="actionDbDataReader"></param>
        /// <returns></returns>
        public static void ExecuteReader(this ISqlRepository repository, Sqled sqled, Action<DbDataReader> actionDbDataReader)
        {
            var work = repository.GetDbWork();
            using (var conn = work.Connect())
            {
                using (var command = work.GetSqlCommand(conn, sqled))
                {
                    try
                    {
                        using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            actionDbDataReader(reader);
                            reader.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new SqlException(sqled, ex);
                    }
                }
            }
        }

        /// <summary>
        /// 执行数据集合读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static List<T> GetDatas<T>(this ISqlRepository repository, Sqled sqled)
            where T : class
        {
            try
            {
                var datas = new List<T>();
                var model = repository.GetModelOrNull<T>();
                // 执行读取
                repository.ExecuteReader(sqled, reader =>
                {
                    reader.FillDatas(datas, model);
                });
                return datas;
            }
            catch (System.Exception ex)
            {
                throw new SqlException(sqled, ex);
            }
        }

        /// <summary>
        /// 执行数据读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        public static T GetData<T>(this ISqlRepository repository, Sqled sqled)
            where T : class
        {
            T data = default;
#else
        public static T? GetData<T>(this ISqlRepository repository, Sqled sqled)
            where T : class
        {
            T? data = default;
#endif
            var model = repository.GetModelOrNull<T>();
            // 执行读取
            repository.ExecuteReader(sqled, reader =>
            {
                data = reader.GetData<T>(model);
            });
            return data;
        }

        /// <summary>
        /// 执行数据读取
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static bool Any(this ISqlRepository repository, Sqled sqled)
        {
            bool result = false;
            // 执行读取
            repository.ExecuteReader(sqled, reader =>
            {
                result = reader.Read();
            });
            return result;
        }

        /// <summary>
        /// 创建原始查询
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static IQueryable<T> Query<T>(this ISqlRepository repository, string sql)
        {
            var work = repository.GetDbWork();
            var entityQueryProvider = new EntityQueryProvider(work);
            return new SqlQueryable<T>(entityQueryProvider, work.EntityModelFactory, sql);
        }

        /// <summary>
        /// 创建原始查询
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static IQueryable<T> Query<T>(this ISqlRepository repository, Sqled sqled)
        {
            var work = repository.GetDbWork();
            var entityQueryProvider = new EntityQueryProvider(work);
            return new SqlQueryable<T>(entityQueryProvider, work.EntityModelFactory, sqled);
        }

        /// <summary>
        /// 获取建模或空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        public static IEntityModel GetModelOrNull<T>(this ISqlRepository repository)
#else
        private static IEntityModel? GetModelOrNull<T>(this ISqlRepository repository)
#endif
            where T : class
        {
            var type = typeof(T);
            if (!(type.IsValueType || type == typeof(string))) { return null; }
            return repository.GetModel<T>();
        }
    }
}


