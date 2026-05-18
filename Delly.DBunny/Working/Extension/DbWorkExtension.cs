using Delly.DBunny;
using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Eazy.Data.Work.Extension
{
    /// <summary>
    /// 数据库作业扩展
    /// </summary>
    public static class DbWorkExtension
    {

        #region 直接执行Sql语句

        /// <summary>
        /// 执行Sql语句
        /// </summary>
        /// <param name="dbWork"></param>
        /// <param name="sqlSet"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(this IDbWork dbWork, Sqled sqled)
        {
            dbWork.Sqleds.Clear();
            dbWork.Sqleds.Add(sqled);
            return dbWork.Commit();
        }

        /// <summary>
        /// 执行Sql语句
        /// </summary>
        /// <param name="dbWork"></param>
        /// <param name="sqlSet"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteNonQueryAsync(this IDbWork dbWork, Sqled sqled)
        {
            dbWork.Sqleds.Clear();
            dbWork.Sqleds.Add(sqled);
            return await dbWork.CommitAsync();
        }

        #endregion

        #region 执行Sql语句并读取DataSet

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqlSet"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(this IDbWork work, Sqled sqled)
        {
            using (var conn = work.Connect())
            {
                using (var command = work.GetSqlCommand(conn, sqled))
                {
                    return work.Provider.GetDataSet(command);
                }
            }
        }

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqlSet"></param>
        /// <returns></returns>
        public static async Task<DataSet> GetDataSetAsync(this IDbWork work, Sqled sqled)
        {
            var dataSet = work.GetDataSet(sqled);
            return await Task.FromResult(dataSet);
        }

        #endregion

        #region 执行Sql语句并读取DataSet

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(this IDbWork work, Sqled sqled)
        {
            var dataSet = work.GetDataSet(sqled);
            return GetDataTable(dataSet);
        }

        /// <summary>
        /// 获取DataSet
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static async Task<DataTable> GetDataTableAsync(this IDbWork work, Sqled sqled)
        {
            var dataSet = await work.GetDataSetAsync(sqled);
            return GetDataTable(dataSet);
        }

        // 获取DataTable
        private static DataTable GetDataTable(DataSet dataSet)
        {
            if (dataSet is null) { return new DataTable(); }
            return dataSet.Tables[0];
        }

        #endregion

        #region 执行Sql语句读取

        /// <summary>
        /// 执行原始数据读取
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <param name="actionDbDataReader"></param>
        /// <returns></returns>
        public static void ExecuteReader(this IDbWork work, Sqled sqled, Action<DbDataReader> actionDbDataReader)
        {
            using (var conn = work.Connect())
            {
                using (var command = work.GetSqlCommand(conn, sqled))
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        actionDbDataReader(reader);
                        reader.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行原始数据读取
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <param name="actionDbDataReader"></param>
        /// <returns></returns>
        public static async Task ExecuteReaderAsync(this IDbWork work, Sqled sqled, Func<DbDataReader, Task> func)
        {
            using (var dbc = await work.ConnectAsync())
            {
                using (var command = work.GetSqlCommand(dbc, sqled))
                {
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        await func.Invoke(reader);
#if NETSTANDARD2_0
                        reader.Close();
#else
                        await reader.CloseAsync();
#endif
                    }
                }
            }
        }

        #endregion

        #region 执行Sql语句并判断是否具有返回结果

        /// <summary>
        /// 执行Sql语句并判断是否具有返回结果
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static bool Any(this IDbWork work, Sqled sqled)
        {
            bool result = false;
            // 执行读取
            work.ExecuteReader(sqled, reader =>
            {
                result = reader.Read();
            });
            return result;
        }

        /// <summary>
        /// 执行Sql语句并判断是否具有返回结果
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync(this IDbWork work, Sqled sqled)
        {
            bool result = false;
            // 执行读取
            await work.ExecuteReaderAsync(sqled, async reader =>
            {
                result = await reader.ReadAsync();
            });
            return result;
        }

        #endregion

        #region 获取单条数据

        /// <summary>
        /// 执行Sql语句并获取单个对象
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        public static T FirstOrDefault<T>(this IDbWork work, Sqled sqled, IDbMapper<T> mapper)
#else
        public static T? FirstOrDefault<T>(this IDbWork work, Sqled sqled, IDbMapper<T> mapper)
#endif
        {
            var result = default(T);
            // 执行读取
            work.ExecuteReader(sqled, reader =>
            {
                if (reader.Read())
                {
                    result = mapper.Map(reader);
                }
            });
            return result;
        }

        /// <summary>
        /// 异步执行Sql语句并获取单个对象
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        public static async Task<T> FirstOrDefaultAsync<T>(this IDbWork work, Sqled sqled, IDbMapper<T> mapper)
#else
        public static async Task<T?> FirstOrDefaultAsync<T>(this IDbWork work, Sqled sqled, IDbMapper<T> mapper)
#endif
        {
            var result = default(T);
            // 执行读取
            await work.ExecuteReaderAsync(sqled, async reader =>
             {
                 if (await reader.ReadAsync())
                 {
                     result = mapper.Map(reader);
                 }
             });
            return result;
        }

        #endregion

        #region 获取数据集合

        /// <summary>
        /// 执行Sql语句并获取数据集合
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static List<T> List<T>(this IDbWork work, Sqled sqled, IDbMapper<T> mapper)
        {
            var list = new List<T>();
            // 执行读取
            work.ExecuteReader(sqled, reader =>
            {
                while (reader.Read())
                {
                    list.Add(mapper.Map(reader));
                }
            });
            return list;
        }

        /// <summary>
        /// 异步执行Sql语句并获取数据集合
        /// </summary>
        /// <param name="work"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static async Task<List<T>> ListAsync<T>(this IDbWork work, Sqled sqled, IDbMapper<T> mapper)
        {
            var list = new List<T>();
            // 执行读取
            await work.ExecuteReaderAsync(sqled, async reader =>
            {
                while (await reader.ReadAsync())
                {
                    list.Add(mapper.Map(reader));
                }
            });
            return list;
        }

        #endregion

    }
}


