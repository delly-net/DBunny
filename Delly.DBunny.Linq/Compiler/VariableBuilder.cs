using System;
using System.Collections.Generic;

namespace Delly.DBunny.Linq.Compiler
{
    /// <summary>
    /// 变量构建器，用于生成 SQL 参数
    /// </summary>
    public sealed class VariableBuilder
    {
        private readonly Dictionary<string, object> _parameters;
        private int _parameterIndex;

        /// <summary>
        /// 变量构建器
        /// </summary>
        public VariableBuilder()
        {
#if NET8_0
            _parameters = [];
#else
            _parameters = new Dictionary<string, object>();
#endif
            _parameterIndex = 0;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="value">参数值</param>
        /// <returns>参数名</returns>
        public string AddParameter(object value)
        {
            var paramName = $"p{_parameterIndex++}";
            _parameters[paramName] = value;
            return paramName;
        }

        /// <summary>
        /// 获取所有参数
        /// </summary>
        /// <returns>参数集合</returns>
        public IReadOnlyDictionary<string, object> GetParameters() => _parameters;
    }
}