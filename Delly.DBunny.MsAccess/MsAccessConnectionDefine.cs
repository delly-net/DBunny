using Delly.DBunny.Core.Connecting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Delly.DBunny.MsAccess
{
    /// <summary>
    /// Microsoft Access 连接定义
    /// </summary>
    public class MsAccessConnectionDefine : BaseConnectionDefine
    {
        // Access 连接字符串常量键
        private const string DATA_SOURCE_KEY = "Data Source";
        private const string PROVIDER_KEY = "Provider";
        private const string PASSWORD_KEY = "Jet OLEDB:Database Password";
        private const string PERSIST_SECURITY_INFO_KEY = "Persist Security Info";
        private const string MODE_KEY = "Mode";
        private const string CONNECTION_TIMEOUT_KEY = "Connect Timeout";
        private const string DEFAULT_COMMAND_TIMEOUT_KEY = "Default Command Timeout";
        private const string POOLING_KEY = "OLE DB Services";
        private const string MIN_POOL_SIZE_KEY = "Min Pool Size";
        private const string MAX_POOL_SIZE_KEY = "Max Pool Size";

        /// <summary>
        /// 数据库类型
        /// </summary>
        public const string DATABASE_TYPE = "MSACCESS";

        /// <summary>
        /// Access 主流版本提供程序 (.accdb) - ACE 12.0+
        /// </summary>
        public const string PROVIDER_ACE = "Microsoft.ACE.OLEDB.12.0";

        /// <summary>
        /// Access 2003 版本提供程序 (.mdb) - Jet 4.0
        /// </summary>
        public const string PROVIDER_JET = "Microsoft.Jet.OLEDB.4.0";

        /// <summary>
        /// MsAccess 连接定义
        /// </summary>
        public MsAccessConnectionDefine()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// 从连接字符串创建 MsAccess 连接定义
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public MsAccessConnectionDefine(string connectionString)
        {
            SetDefaultValues();
            ParseConnectionString(connectionString);
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            Set(PROVIDER_KEY, PROVIDER_ACE);
        }

        /// <summary>
        /// 数据库文件路径
        /// </summary>
#if NETSTANDARD2_0
        public string DataSource
#else
        public string? DataSource
#endif
        {
            get => Get(DATA_SOURCE_KEY, string.Empty);
            set => Set(DATA_SOURCE_KEY, value);
        }

        /// <summary>
        /// 数据库提供程序 (ACE 或 JET)
        /// </summary>
#if NETSTANDARD2_0
        public string Provider
#else
        public string? Provider
#endif
        {
            get => Get(PROVIDER_KEY, PROVIDER_ACE);
            set => Set(PROVIDER_KEY, value);
        }

        /// <summary>
        /// 数据库密码
        /// </summary>
#if NETSTANDARD2_0
        public string Password
#else
        public string? Password
#endif
        {
            get => Get(PASSWORD_KEY, string.Empty);
            set => Set(PASSWORD_KEY, value);
        }

        /// <summary>
        /// 是否持久化安全信息
        /// </summary>
        public bool PersistSecurityInfo
        {
            get
            {
                var value = Get(PERSIST_SECURITY_INFO_KEY, "False");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(PERSIST_SECURITY_INFO_KEY, value ? "True" : "False");
        }

        /// <summary>
        /// 访问模式 (Share Deny None, Share Exclusive, Read, Read Write, etc.)
        /// </summary>
#if NETSTANDARD2_0
        public string Mode
#else
        public string? Mode
#endif
        {
            get => Get(MODE_KEY, "Share Deny None");
            set => Set(MODE_KEY, value);
        }

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
#if NETSTANDARD2_0
        public int ConnectionTimeout
#else
        public int? ConnectionTimeout
#endif
        {
            get => Get(CONNECTION_TIMEOUT_KEY, 15);
#if NETSTANDARD2_0
            set => Set(CONNECTION_TIMEOUT_KEY, value.ToString());
#else
            set => Set(CONNECTION_TIMEOUT_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 默认命令超时时间（秒）
        /// </summary>
#if NETSTANDARD2_0
        public int DefaultCommandTimeout
#else
        public int? DefaultCommandTimeout
#endif
        {
            get => Get(DEFAULT_COMMAND_TIMEOUT_KEY, 30);
#if NETSTANDARD2_0
            set => Set(DEFAULT_COMMAND_TIMEOUT_KEY, value.ToString());
#else
            set => Set(DEFAULT_COMMAND_TIMEOUT_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 是否启用连接池 (-1 = 启用, 0 = 禁用)
        /// </summary>
        public bool Pooling
        {
            get => Get(POOLING_KEY, "-1") != "0";
            set => Set(POOLING_KEY, value ? "-1" : "0");
        }

        /// <summary>
        /// 最小连接池大小
        /// </summary>
#if NETSTANDARD2_0
        public int MinPoolSize
#else
        public int? MinPoolSize
#endif
        {
            get => Get(MIN_POOL_SIZE_KEY, 0);
#if NETSTANDARD2_0
            set => Set(MIN_POOL_SIZE_KEY, value.ToString());
#else
            set => Set(MIN_POOL_SIZE_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 最大连接池大小
        /// </summary>
#if NETSTANDARD2_0
        public int MaxPoolSize
#else
        public int? MaxPoolSize
#endif
        {
            get => Get(MAX_POOL_SIZE_KEY, 100);
#if NETSTANDARD2_0
            set => Set(MAX_POOL_SIZE_KEY, value.ToString());
#else
            set => Set(MAX_POOL_SIZE_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 设置任意连接参数
        /// </summary>
        /// <param name="key">参数键</param>
        /// <param name="value">参数值</param>
        public void SetParameter(string key, object value)
        {
            Set(key, value);
        }

        /// <summary>
        /// 获取连接参数
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="key">参数键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>参数值</returns>
#if NETSTANDARD2_0
        public TValue GetParameter<TValue>(string key, TValue defaultValue)
        {
            return Get(key, defaultValue);
        }
#else
        public TValue GetParameter<TValue>(string key, TValue defaultValue) where TValue : notnull
        {
            return Get(key, defaultValue)!;
        }
#endif

        /// <summary>
        /// 解析连接字符串
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        private void ParseConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            var parts = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var keyValue = part.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0].Trim();
                    var value = keyValue[1].Trim();
                    Set(key, value);
                }
            }
        }

        /// <summary>
        /// 转为连接字符串
        /// </summary>
        /// <returns></returns>
        public override string ToConnectionString()
        {
            var sb = new StringBuilder();
            foreach (var item in Values)
            {
                if (item.Value == null || string.IsNullOrEmpty(item.Value?.ToString()))
                    continue;

                sb.Append(item.Key);
                sb.Append('=');
                sb.Append(item.Value?.ToString());
                sb.Append(';');
            }
            return sb.ToString();
        }
    }
}