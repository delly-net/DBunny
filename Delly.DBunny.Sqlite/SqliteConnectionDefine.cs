using Delly.DBunny.Connecting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Delly.DBunny.Sqlite
{
    /// <summary>
    /// Sqlite 连接定义
    /// </summary>
    public class SqliteConnectionDefine : BaseConnectionDefine
    {
        // SQLite 连接字符串常量键
        private const string DATA_SOURCE_KEY = "Data Source";
        private const string VERSION_KEY = "Version";
        private const string PASSWORD_KEY = "Password";
        private const string PAGE_SIZE_KEY = "Page Size";
        private const string CACHE_SIZE_KEY = "Cache Size";
        private const string MODE_KEY = "Mode";
        private const string DEFAULT_TIMEOUT_KEY = "Default Timeout";
        private const string JOURNAL_MODE_KEY = "Journal Mode";
        private const string POOLING_KEY = "Pooling";
        private const string FOREIGN_KEYS_KEY = "Foreign Keys";
        private const string FAIL_IF_MISSING_KEY = "FailIfMissing";
        private const string READ_ONLY_KEY = "Read Only";
        private const string LEGACY_FORMAT_KEY = "Legacy Format";
        private const string DATE_TIME_FORMAT_KEY = "DateTimeFormat";
        private const string DATE_TIME_KIND_KEY = "DateTimeKind";

        /// <summary>
        /// Sqlite 连接定义
        /// </summary>
        public SqliteConnectionDefine()
        {
            SetDefaultValues();
        }

        /// <summary>
        /// 从连接字符串创建 Sqlite 连接定义
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public SqliteConnectionDefine(string connectionString)
        {
            SetDefaultValues();
            ParseConnectionString(connectionString);
        }

        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            Set(VERSION_KEY, "3");
            Set(DEFAULT_TIMEOUT_KEY, "30");
            Set(POOLING_KEY, "True");
            Set(FOREIGN_KEYS_KEY, "True");
            Set(JOURNAL_MODE_KEY, "WAL");
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
        /// SQLite 版本 (2 或 3)
        /// </summary>
#if NETSTANDARD2_0
        public string Version
#else
        public string? Version
#endif
        {
            get => Get(VERSION_KEY, "3");
            set => Set(VERSION_KEY, value);
        }

        /// <summary>
        /// 数据库密码（加密）
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
        /// 页大小（字节）
        /// </summary>
#if NETSTANDARD2_0
        public int PageSize
#else
        public int? PageSize
#endif
        {
            get => Get(PAGE_SIZE_KEY, 4096);
#if NETSTANDARD2_0
            set => Set(PAGE_SIZE_KEY, value.ToString());
#else
            set => Set(PAGE_SIZE_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 缓存页数
        /// </summary>
#if NETSTANDARD2_0
        public int CacheSize
#else
        public int? CacheSize
#endif
        {
            get => Get(CACHE_SIZE_KEY, 2000);
#if NETSTANDARD2_0
            set => Set(CACHE_SIZE_KEY, value.ToString());
#else
            set => Set(CACHE_SIZE_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 打开模式
        /// </summary>
#if NETSTANDARD2_0
        public string Mode
#else
        public string? Mode
#endif
        {
            get => Get(MODE_KEY, "ReadWriteCreate");
            set => Set(MODE_KEY, value);
        }

        /// <summary>
        /// 默认超时时间（秒）
        /// </summary>
#if NETSTANDARD2_0
        public int DefaultTimeout
#else
        public int? DefaultTimeout
#endif
        {
            get => Get(DEFAULT_TIMEOUT_KEY, 30);
#if NETSTANDARD2_0
            set => Set(DEFAULT_TIMEOUT_KEY, value.ToString());
#else
            set => Set(DEFAULT_TIMEOUT_KEY, value?.ToString());
#endif
        }

        /// <summary>
        /// 日志模式 (DELETE, TRUNCATE, PERSIST, MEMORY, WAL, OFF)
        /// </summary>
#if NETSTANDARD2_0
        public string JournalMode
#else
        public string? JournalMode
#endif
        {
            get => Get(JOURNAL_MODE_KEY, "WAL");
            set => Set(JOURNAL_MODE_KEY, value);
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
        /// 是否启用外键约束
        /// </summary>
        public bool ForeignKeys
        {
            get
            {
                var value = Get(FOREIGN_KEYS_KEY, "True");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(FOREIGN_KEYS_KEY, value ? "True" : "False");
        }

        /// <summary>
        /// 如果数据库文件不存在是否抛出异常
        /// </summary>
        public bool FailIfMissing
        {
            get
            {
                var value = Get(FAIL_IF_MISSING_KEY, "False");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(FAIL_IF_MISSING_KEY, value ? "True" : "False");
        }

        /// <summary>
        /// 是否以只读方式打开
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                var value = Get(READ_ONLY_KEY, "False");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(READ_ONLY_KEY, value ? "True" : "False");
        }

        /// <summary>
        /// 是否使用旧格式（兼容 2.x）
        /// </summary>
        public bool LegacyFormat
        {
            get
            {
                var value = Get(LEGACY_FORMAT_KEY, "False");
                return string.Equals(value, "True", StringComparison.OrdinalIgnoreCase);
            }
            set => Set(LEGACY_FORMAT_KEY, value ? "True" : "False");
        }

        /// <summary>
        /// DateTime 格式
        /// </summary>
#if NETSTANDARD2_0
        public string DateTimeFormat
#else
        public string? DateTimeFormat
#endif
        {
            get => Get(DATE_TIME_FORMAT_KEY, "ISO8601");
            set => Set(DATE_TIME_FORMAT_KEY, value);
        }

        /// <summary>
        /// DateTime 类型
        /// </summary>
#if NETSTANDARD2_0
        public string DateTimeKind
#else
        public string? DateTimeKind
#endif
        {
            get => Get(DATE_TIME_KIND_KEY, "Utc");
            set => Set(DATE_TIME_KIND_KEY, value);
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