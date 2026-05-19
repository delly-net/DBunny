using Delly.DBunny;
using Delly.DBunny.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Delly.DBunny.Filting
{
    /// <summary>
    /// 数据库 过滤器
    /// </summary>
    public abstract class BaseDbFilter : IDbFilter
    {

        /// <summary>
        /// 命令管理器创建
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        public DbCommand CommandCreating(DbCommand command)
#else
        public DbCommand? CommandCreating(DbCommand? command)
#endif        
        {
            return OnCommandCreating(command);
        }

        /// <summary>
        /// 命令管理器创建
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
#if NETSTANDARD2_0
        protected virtual DbCommand OnCommandCreating(DbCommand command)
#else
        protected virtual DbCommand? OnCommandCreating(DbCommand? command)
#endif
        {
            return command;
        }

        /// <summary>
        /// 命令管理器执行
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public DbCommand CommandExecuting(DbCommand command)
        {
            return OnCommandExecuting(command);
        }

        /// <summary>
        /// 命令管理器执行
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected virtual DbCommand OnCommandExecuting(DbCommand command)
        {
            return command;
        }

        /// <summary>
        /// Sql装载
        /// </summary>
        /// <param name="sqled"></param>
        /// <returns></returns>
        public Sqled SqlLoading(Sqled sqled)
        {
            return OnSqlLoading(sqled);
        }

        /// <summary>
        /// Sql装载
        /// </summary>
        /// <param name="sqled"></param>
        /// <returns></returns>
        protected virtual Sqled OnSqlLoading(Sqled sqled)
        {
            return sqled;
        }
    }
}


