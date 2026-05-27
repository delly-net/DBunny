using Delly.DBunny;
using Delly.DBunny.Core;
using Delly.DBunny.Reading.Extension;
using Delly.Modeling;
using Eazy.Data.Work.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
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
        public static async Task ExecuteNonQueryAsync(this ISqlRepository repository, Sqled sqled)
        {
            var work = repository.GetDbWork();
            work.Sqleds.Add(sqled);
            await Task.CompletedTask;
        }

        /// <summary>
        /// 执行原始数据读取
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <param name="actionDbDataReader"></param>
        /// <returns></returns>
        public static async Task ExecuteReaderAsync(this ISqlRepository repository, Sqled sqled, Func<DbDataReader, Task> actionDbDataReader)
        {
            var work = repository.GetDbWork();
            using (var conn = await work.ConnectAsync())
            {
                using (var command = work.GetSqlCommand(conn, sqled))
                {
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            await actionDbDataReader(reader);
#if NETSTANDARD2_0
                            reader.Close();
#else
                            await reader.CloseAsync();
#endif
                        }
                    }
                    catch (System.Exception ex)
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
        public static async Task<List<T>> GetDatasAsync<T>(this ISqlRepository repository, Sqled sqled)
            where T : class
        {
            var datas = new List<T>();
            var model = repository.GetModelOrNull<T>();
            // 执行读取
            await repository.ExecuteReaderAsync(sqled, async reader =>
            {
                reader.FillDatas(datas, model);
                await Task.CompletedTask;
            });
            return datas;
        }

        /// <summary>
        /// 执行数据读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        public static async Task<T> GetDataAsync<T>(this ISqlRepository repository, Sqled sqled)
            where T : class
        {
            T data = default;
#else
        public static async Task<T?> GetDataAsync<T>(this ISqlRepository repository, Sqled sqled)
            where T : class
        {
            T? data = default;
#endif
            var model = repository.GetModelOrNull<T>();
            // 执行读取
            await repository.ExecuteReaderAsync(sqled, async reader =>
            {
                data = reader.GetData<T>(model);
                await Task.CompletedTask;
            });
            return data;
        }

        /// <summary>
        /// 执行数据读取
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public static async Task<bool> AnyAsync(this ISqlRepository repository, Sqled sqled)
        {
            bool result = false;
            // 执行读取
            await repository.ExecuteReaderAsync(sqled, async reader =>
            {
                result = reader.Read();
                await Task.CompletedTask;
            });
            return result;
        }
    }
}


