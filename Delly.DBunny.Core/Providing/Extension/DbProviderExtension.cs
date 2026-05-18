using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny.Providing.Extension
{
    /// <summary>
    /// 数据库提供程序 扩展
    /// </summary>
    public static class DbProviderExtension
    {
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="provider">数据库提供程序</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 命令</param>
        /// <param name="action">读取操作</param>
        public static void Read(this IDbProvider provider, DbConnection connection, Sqled sql, Action<DbDataReader> action)
        {
            using (var command = provider.GetDbCommand(connection))
            {
                command.CommandText = sql.Sql;
                provider.SetParameters(command, sql.Parameters);
                var reader = command.ExecuteReader();
                action.Invoke(reader);
                reader.Close();
            }
        }

        /// <summary>
        /// 异步读取数据
        /// </summary>
        /// <param name="provider">数据库提供程序</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">SQL 命令</param>
        /// <param name="func">异步读取操作</param>
        public static async Task ReadAsync(this IDbProvider provider, DbConnection connection, Sqled sql, Func<DbDataReader, Task> func)
        {
            using (var command = provider.GetDbCommand(connection))
            {
                command.CommandText = sql.Sql;
                provider.SetParameters(command, sql.Parameters);
                var reader = await command.ExecuteReaderAsync();
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
