using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Delly.DBunny
{
    /// <summary>
    /// Sql对象
    /// </summary>
    public sealed class Sqled
    {
        private readonly StringBuilder _stringBuilder;

        /// <summary>
        /// 数据库作业命令
        /// </summary>
        public Sqled(string sql, IEnumerable<KeyValuePair<string, object>> parameters) : this(sql)
        {
            foreach (var pair in parameters)
            {
                Parameters.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// 数据库作业命令包裹曾
        /// </summary>
        public Sqled(string sql)
        {
            _stringBuilder = new StringBuilder(sql);
#if NET8_0
            Parameters = [];
#else
            Parameters = new Dictionary<string, object>();
#endif
        }

        /// <summary>
        /// 数据库作业命令包裹曾
        /// </summary>
        public Sqled()
        {
            _stringBuilder = new StringBuilder();
#if NET8_0
            Parameters = [];
#else
            Parameters = new Dictionary<string, object>();
#endif
        }

        /// <summary>
        /// 字符串
        /// </summary>
        public StringBuilder Builder => _stringBuilder;

        /// <summary>
        /// Sql脚本
        /// </summary>
        public string Sql => _stringBuilder.ToString();

        /// <summary>
        /// 参数集合
        /// </summary>
        public Dictionary<string, object> Parameters { get; }

        /// <summary>
        /// 运算符重载
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static implicit operator Sqled(string sql)
        {
            return new Sqled(sql);
        }

        /// <summary>
        /// 获取参数字符串
        /// </summary>
        /// <param name="sqlSet"></param>
        /// <returns></returns>
        public string GetParametersString()
        {
            var sb = new StringBuilder();
            sb.Append('{');
            foreach (var parameter in this.Parameters)
            {
                if (sb.Length > 1) { sb.Append(", "); }
                sb.Append(parameter.Key);
                sb.Append(": \"");
                sb.Append(parameter.Value.ToString());
                sb.Append('"');
            }
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// 转为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sql = _stringBuilder.ToString().Replace("\r", "\\r").Replace("\n", "\\n");
            return $"{{ Sql:\"{sql}\", Params: {GetParametersString()} }}";
        }
    }
}


