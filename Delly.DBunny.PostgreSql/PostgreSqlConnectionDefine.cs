using Delly.DBunny.Core.Connecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Delly.DBunny.PostgreSql
{
    /// <summary>
    /// PostgreSql 连接定义
    /// </summary>
    public class PostgreSqlConnectionDefine : BaseConnectionDefine
    {
        // PostgreSQL 连接字符串常量键
        private const string HOST_KEY = "Host";
        private const string PORT_KEY = "Port";
        private const string DATABASE_KEY = "Database";
        private const string USERNAME_KEY = "Username";
        private const string PASSWORD_KEY = "Password";
        private const string SEARCH_PATH_KEY = "Search Path";
        private const string SSL_MODE_KEY = "SSL Mode";
        private const string TRUST_SERVER_CERTIFICATE_KEY = "Trust Server Certificate";
        private const string TIMEOUT_KEY = "Timeout";
        private const string COMMAND_TIMEOUT_KEY = "Command Timeout";
        private const string POOLING_KEY = "Pooling";
        private const string MIN_POOL_SIZE_KEY = "Minimum Pool Size";
        private const string MAX_POOL_SIZE_KEY = "Maximum Pool Size";

        /// <summary>
        /// 数据库类型
        /// </summary>
        public const string DATABASE_TYPE = "POSTGRESQL";

        /// <summary>
        /// PostgreSql 连接定义
        /// </summary>
        public PostgreSqlConnectionDefine()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// 从连接字符串创建 PostgreSql 连接定义
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public PostgreSqlConnectionDefine(string connectionString)
        {
            SetDefaultValues();
            ParseConnectionString(connectionString);
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            Set(PORT_KEY, "5432");
            Set(SEARCH_PATH_KEY, "public");
            Set(SSL_MODE_KEY, "Prefer");
            Set(TRUST_SERVER_CERTIFICATE_KEY, "False");
            Set(TIMEOUT_KEY, "30");
            Set(COMMAND_TIMEOUT_KEY, "600");
            Set(POOLING_KEY, "True");
            Set(MIN_POOL_SIZE_KEY, "0");
            Set(MAX_POOL_SIZE_KEY, "100");
        }

        /// <summary>
        /// 服务器地址
        /// </summary>
#if NETSTANDARD2_0
        public string Host
#else
        public string? Host
#endif
        {
            get => Get(HOST_KEY, "localhost");
            set => Set(HOST_KEY, value);
        }

        /// <summary>
        /// 服务器端口
        /// </summary>
#if NETSTANDARD2_0
        public int Port
#else
        public int? Port
#endif
        {
            get => Get(PORT_KEY, 5432);
#if NETSTANDARD2_0
            set => Set(PORT_KEY, value.ToString());
#else
            set => Set(PORT_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 数据库名称
        /// </summary>
#if NETSTANDARD2_0
        public string Database
#else
        public string? Database
#endif
        {
            get => Get(DATABASE_KEY, string.Empty);
            set => Set(DATABASE_KEY, value);
        }

        /// <summary>
        /// 用户名
        /// </summary>
#if NETSTANDARD2_0
        public string Username
#else
        public string? Username
#endif
        {
            get => Get(USERNAME_KEY, string.Empty);
            set => Set(USERNAME_KEY, value);
        }

        /// <summary>
        /// 密码
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
        /// 搜索路径（Schema列表）
        /// </summary>
#if NETSTANDARD2_0
        public string SearchPath
#else
        public string? SearchPath
#endif
        {
            get => Get(SEARCH_PATH_KEY, "public");
            set => Set(SEARCH_PATH_KEY, value);
        }

        /// <summary>
        /// SSL 模式
        /// </summary>
#if NETSTANDARD2_0
        public string SslMode
#else
        public string? SslMode
#endif
        {
            get => Get(SSL_MODE_KEY, "Prefer");
            set => Set(SSL_MODE_KEY, value);
        }

        /// <summary>
        /// 连接超时时间（秒）
        /// </summary>
#if NETSTANDARD2_0
        public int Timeout
#else
        public int? Timeout
#endif
        {
            get => Get(TIMEOUT_KEY, 30);
#if NETSTANDARD2_0
            set => Set(TIMEOUT_KEY, value.ToString());
#else
            set => Set(TIMEOUT_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 默认命令超时时间（秒）
        /// </summary>
#if NETSTANDARD2_0
        public int CommandTimeout
#else
        public int? CommandTimeout
#endif
        {
            get => Get(COMMAND_TIMEOUT_KEY, 600);
#if NETSTANDARD2_0
            set => Set(COMMAND_TIMEOUT_KEY, value.ToString());
#else
            set => Set(COMMAND_TIMEOUT_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 是否使用连接池
        /// </summary>
        public bool Pooling
        {
            get
            {
                var value = Get(POOLING_KEY, "True");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(POOLING_KEY, value ? "True" : "False");
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
    }
}