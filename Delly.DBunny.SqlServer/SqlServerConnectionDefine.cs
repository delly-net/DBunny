using Delly.DBunny.Connecting;
using System;

namespace Delly.DBunny.SqlServer
{
    /// <summary>
    /// SQL Server 连接定义
    /// </summary>
    public class SqlServerConnectionDefine : BaseConnectionDefine
    {
        // SQL Server 连接字符串常量键
        private const string SERVER_KEY = "Server";
        private const string DATA_SOURCE_KEY = "Data Source";
        private const string DATABASE_KEY = "Database";
        private const string INITIAL_CATALOG_KEY = "Initial Catalog";
        private const string USER_ID_KEY = "User Id";
        private const string USER_KEY = "User";
        private const string PASSWORD_KEY = "Password";
        private const string PWD_KEY = "Pwd";
        private const string TRUST_SERVER_CERTIFICATE_KEY = "TrustServerCertificate";
        private const string ENCRYPT_KEY = "Encrypt";
        private const string CONNECTION_TIMEOUT_KEY = "Connect Timeout";
        private const string COMMAND_TIMEOUT_KEY = "Command Timeout";
        private const string POOLING_KEY = "Pooling";
        private const string MIN_POOL_SIZE_KEY = "Min Pool Size";
        private const string MAX_POOL_SIZE_KEY = "Max Pool Size";
        private const string MULTIPLE_ACTIVE_RESULT_SETS_KEY = "MultipleActiveResultSets";
        private const string PACKET_SIZE_KEY = "Packet Size";
        private const string APPLICATION_NAME_KEY = "Application Name";

        /// <summary>
        /// 数据库类型
        /// </summary>
        public const string DATABASE_TYPE = "SQLSERVER";

        /// <summary>
        /// SQL Server 连接定义
        /// </summary>
        public SqlServerConnectionDefine()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// 从连接字符串创建 SQL Server 连接定义
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public SqlServerConnectionDefine(string connectionString)
        {
            SetDefaultValues();
            ParseConnectionString(connectionString);
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            Set(ENCRYPT_KEY, "True");
            Set(TRUST_SERVER_CERTIFICATE_KEY, "True");
            Set(CONNECTION_TIMEOUT_KEY, "30");
            Set(COMMAND_TIMEOUT_KEY, "600");
            Set(POOLING_KEY, "True");
            Set(MIN_POOL_SIZE_KEY, "0");
            Set(MAX_POOL_SIZE_KEY, "100");
            Set(MULTIPLE_ACTIVE_RESULT_SETS_KEY, "True");
            Set(PACKET_SIZE_KEY, "8000");
        }

        /// <summary>
        /// 服务器地址
        /// </summary>
#if NETSTANDARD2_0
        public string Server
#else
        public string? Server
#endif
        {
            get => ContainsKey(DATA_SOURCE_KEY) ? Get<string>(DATA_SOURCE_KEY, "localhost") : Get(SERVER_KEY, "localhost");
            set => Set(DATA_SOURCE_KEY, value);
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
            get => ContainsKey(DATABASE_KEY) ? Get<string>(DATABASE_KEY, string.Empty) : Get(INITIAL_CATALOG_KEY, string.Empty);
            set => Set(DATABASE_KEY, value);
        }

        /// <summary>
        /// 用户名
        /// </summary>
#if NETSTANDARD2_0
        public string UserId
#else
        public string? UserId
#endif
        {
            get => ContainsKey(USER_ID_KEY) ? Get<string>(USER_ID_KEY, string.Empty) : Get(USER_KEY, string.Empty);
            set => Set(USER_ID_KEY, value);
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
            get => ContainsKey(PASSWORD_KEY) ? Get<string>(PASSWORD_KEY, string.Empty) : Get(PWD_KEY, string.Empty);
            set => Set(PASSWORD_KEY, value);
        }

        /// <summary>
        /// 是否信任服务器证书
        /// </summary>
        public bool TrustServerCertificate
        {
            get
            {
                var value = Get(TRUST_SERVER_CERTIFICATE_KEY, "True");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(TRUST_SERVER_CERTIFICATE_KEY, value ? "True" : "False");
        }

        /// <summary>
        /// 是否加密连接
        /// </summary>
        public bool Encrypt
        {
            get
            {
                var value = Get(ENCRYPT_KEY, "True");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(ENCRYPT_KEY, value ? "True" : "False");
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
            get => Get(CONNECTION_TIMEOUT_KEY, 30);
#if NETSTANDARD2_0
            set => Set(CONNECTION_TIMEOUT_KEY, value.ToString());
#else
            set => Set(CONNECTION_TIMEOUT_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 命令超时时间（秒）
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
        /// 是否启用多活动结果集（MARS）
        /// </summary>
        public bool MultipleActiveResultSets
        {
            get
            {
                var value = Get(MULTIPLE_ACTIVE_RESULT_SETS_KEY, "True");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(MULTIPLE_ACTIVE_RESULT_SETS_KEY, value ? "True" : "False");
        }

        /// <summary>
        /// 数据包大小（字节）
        /// </summary>
#if NETSTANDARD2_0
        public int PacketSize
#else
        public int? PacketSize
#endif
        {
            get => Get(PACKET_SIZE_KEY, 8000);
#if NETSTANDARD2_0
            set => Set(PACKET_SIZE_KEY, value.ToString());
#else
            set => Set(PACKET_SIZE_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 应用程序名称
        /// </summary>
#if NETSTANDARD2_0
        public string ApplicationName
#else
        public string? ApplicationName
#endif
        {
            get => Get(APPLICATION_NAME_KEY, string.Empty);
            set => Set(APPLICATION_NAME_KEY, value);
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