using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delly.DBunny.Core.Connecting
{
    /// <summary>
    /// 基础连接定义
    /// </summary>
    public abstract class BaseConnectionDefine : IDbConnectionDefine
    {
        // 值集合
#if NETSTANDARD2_0
        private readonly Dictionary<string, object> _values;
#else
        private readonly Dictionary<string, object?> _values;
#endif

        /// <summary>
        /// 基础连接定义
        /// </summary>
        protected BaseConnectionDefine()
        {
#if NETSTANDARD2_0
            _values = new Dictionary<string, object>();
#else
            _values = new Dictionary<string, object?>();
#endif
        }

        /// <summary>
        /// 值集合
        /// </summary>
#if NETSTANDARD2_0
        protected Dictionary<string, object> Values => _values;
#else
        protected Dictionary<string, object?> Values => _values;
#endif

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString => ToConnectionString();

        /// <summary>
        /// 包含键
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>是否包含键</returns>
        protected bool ContainsKey(string key) => _values.ContainsKey(key);

        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="TValue">值类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>键对应的值</returns>
#if NETSTANDARD2_0
        protected TValue Get<TValue>(string key, TValue defaultValue)
        {

            if (_values.TryGetValue(key, out object value)) { return (TValue)value; }
            return defaultValue;
        }
#else
        protected TValue? Get<TValue>(string key, TValue defaultValue)
        {

            if (_values.TryGetValue(key, out object? value)) { return (TValue?)value; }
            return defaultValue;
        }

#endif

        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
#if NETSTANDARD2_0
        protected void Set(string key, object value)
#else
        protected void Set(string key, object? value)
#endif
        {
            _values[key] = value;
        }

        /// <summary>
        /// 转为连接字符串
        /// </summary>
        /// <returns>连接字符串</returns>
        public virtual string ToConnectionString()
        {
            var sb = new StringBuilder();
            foreach (var item in Values)
            {
                sb.Append(item.Key);
                sb.Append('=');
                if (item.Value is IDbConnectionDefine define)
                {
                    sb.Append(define.ConnectionString);
                }
                else
                {
                    sb.Append(item.Value?.ToString());
                }
                sb.Append(";");
            }
            return sb.ToString();
        }
    }
}


