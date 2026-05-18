using Delly.DBunny.Core.Connecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Delly.DBunny.Oracle
{
    /// <summary>
    /// Oracle 连接定义
    /// </summary>
    public class OracleConnectionDefine : BaseConnectionDefine
    {
        // Oracle 连接字符串常量键
        private const string DATA_SOURCE_KEY = "Data Source";
        private const string USER_ID_KEY = "User Id";
        private const string PASSWORD_KEY = "Password";
        private const string CONNECTION_TIMEOUT_KEY = "Connection Timeout";
        private const string POOLING_KEY = "Pooling";
        private const string MIN_POOL_SIZE_KEY = "Min Pool Size";
        private const string MAX_POOL_SIZE_KEY = "Max Pool Size";

        /// <summary>
        /// 数据库类型
        /// </summary>
        public const string DATABASE_TYPE = "ORACLE";

        /// <summary>
        /// Oracle 连接定义
        /// </summary>
        public OracleConnectionDefine()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// 从连接字符串创建 Oracle 连接定义
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public OracleConnectionDefine(string connectionString)
        {
            SetDefaultValues();
            ParseConnectionString(connectionString);
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            Set(CONNECTION_TIMEOUT_KEY, "30");
            Set(POOLING_KEY, "True");
            Set(MIN_POOL_SIZE_KEY, "0");
            Set(MAX_POOL_SIZE_KEY, "100");
        }

        /// <summary>
        /// 数据源 (TNS 名称或连接描述符)
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
        /// 用户名
        /// </summary>
#if NETSTANDARD2_0
        public string UserId
#else
        public string? UserId
#endif
        {
            get => Get(USER_ID_KEY, string.Empty);
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
            get => Get(PASSWORD_KEY, string.Empty);
            set => Set(PASSWORD_KEY, value);
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