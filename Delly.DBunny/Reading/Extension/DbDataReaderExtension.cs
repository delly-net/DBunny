using Delly.DBunny.Reading.Extension;
using Delly.Modeling;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Delly.DBunny.Reading.Extension
{
    /// <summary>
    /// 数据库阅读器
    /// </summary>
    public static class DbDataReaderExtension
    {
        /// <summary>
        /// 读取单个数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
#if NETSTANDARD2_0
        public static T ToValue<T>(this DbDataReader reader)
        {
            var type = typeof(T);
            return (T)reader.ToValue(type);
        }
#else
        public static T? ToValue<T>(this DbDataReader reader)
        {
            var type = typeof(T);
            return (T?)reader.ToValue(type);
        }
#endif

        /// <summary>
        /// 读取单个数据
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
#if NETSTANDARD2_0
        public static object ToValue(this DbDataReader reader, Type type)
#else
        public static object? ToValue(this DbDataReader reader, Type type)
#endif
        {
            // 读取内容
            var value = reader.GetValue(0);

            // 判断是否为空
            if (value is DBNull) { return null; }

            return ConvertValue(value, type);
        }

        #region 【核心：类型转换逻辑】
        /// <summary>
        /// 通用类型转换（处理兼容类型、可空类型、枚举等）
        /// </summary>
#if NETSTANDARD2_0
        private static object ConvertValue(object rawValue, Type targetType)
#else
        private static object? ConvertValue(object? rawValue, Type targetType)
#endif
        {
            // 跳过空值
            if (rawValue is null) { return null; }

            // 目标类型是原始值类型，直接返回
            if (targetType.IsInstanceOfType(rawValue))
            {
                return rawValue;
            }

            // 处理可空类型（如 int?、DateTime?）
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
            {
                rawValue = ConvertValue(rawValue, underlyingType);
                return rawValue;
            }

            // 处理枚举类型
            if (targetType.IsEnum)
            {
                var str = Convert.ToString(rawValue) ?? string.Empty;
                return Enum.Parse(targetType, str);
            }

            // 处理Guid类型（数据库常存储为字符串）
            if (targetType == typeof(Guid))
            {
                var str = Convert.ToString(rawValue) ?? string.Empty;
                return Guid.Parse(str);
            }

            return Convert.ChangeType(rawValue, targetType);
        }
        #endregion

        /// <summary>
        /// 转化为泛型实例
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entity"></param>
        /// <param name="ordinals"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        public static T ToInstance<T>(this DbDataReader reader, IEntityModel entity, IDictionary<string, int> ordinals)
        {
            return (T)reader.ToInstance(entity, ordinals);
        }

        /// <summary>
        /// 转化为泛型实例
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entity"></param>
        /// <param name="ordinals"></param>
        /// <returns></returns>
        /// <exception cref="DbException"></exception>
        public static object ToInstance(this DbDataReader reader, IEntityModel entity, IDictionary<string, int> ordinals)
        {
            var obj = entity.CreateInstance();
            foreach (var property in entity.GetProperties())
            {
                if (!ordinals.TryGetValue(property.Name, out int idx)) { continue; }
                var ordinalValue = reader.GetValue(idx);
                if (ordinalValue is DBNull) { continue; }
                var value = property.PropertyModel.Parse(ordinalValue);
                property.SetValue(obj, value);
            }
            return obj;
        }

        /// <summary>
        /// 获取字段索引集合
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetEntityOrdinals(this DbDataReader reader, IEntityModel entity)
        {
            var ordinals = new Dictionary<string, int>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i).ToUpper();
                foreach (var property in entity.GetProperties())
                {
                    if (!property.CanWrite) continue;
                    if (property.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)) ordinals[property.Name] = i;
                }
            }
            return ordinals;
        }
        /// <summary>
        /// 填充数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="datas"></param>
        /// <param name="model"></param>
#if NETSTANDARD2_0
        public static void FillDatas<T>(this DbDataReader reader, IList<T> datas, IEntityModel model)
#else
        public static void FillDatas<T>(this DbDataReader reader, IList<T> datas, IEntityModel? model)
#endif
        {
            // 无内容则退出
            if (!reader.HasRows) return;
            // 读取内容
            while (reader.Read())
            {
                // 跳过
                if (reader.FieldCount <= 0) continue;
                if (model is null)
                {
                    var value = reader.ToValue<T>();
                    if (value != null) { datas.Add(value); }
                }
                else
                {
                    // 进行字段名称初始化
                    Dictionary<string, int> ordinals = reader.GetEntityOrdinals(model);
                    datas.Add(reader.ToInstance<T>(model, ordinals));
                }
            }
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="model"></param>
#if NETSTANDARD2_0
        public static T GetData<T>(this DbDataReader reader, IEntityModel model)
#else
        public static T? GetData<T>(this DbDataReader reader, IEntityModel? model)
#endif
        {
            // 无内容则退出
            if (!reader.HasRows) { return default; }
            // 读取内容
            if (!reader.Read()) { return default; }
            // 跳过无字段
            if (reader.FieldCount <= 0) { return default; }
            if (model is null)
            {
                return reader.ToValue<T>();
            }
            Dictionary<string, int> ordinals = reader.GetEntityOrdinals(model);
            return (T)reader.ToInstance(model, ordinals);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="model"></param>
        /// <param name="data"></param>
        /// <param name="ordinals"></param>
#if NETSTANDARD2_0
        public static bool ReadData(this DbDataReader reader, IEntityModel model, ref Dictionary<string, int> ordinals, out object data)
#else
        public static bool ReadData(this DbDataReader reader, IEntityModel model, ref Dictionary<string, int> ordinals, out object? data)
#endif
        {
            data = null;
            // 无内容则退出
            if (!reader.HasRows) return false;
            // 读取内容
            if (!reader.Read()) return false;
            // 跳过无字段
            if (reader.FieldCount <= 0) return false;
            // 进行字段名称初始化
            if (ordinals.Count == 0)
            {
                ordinals = reader.GetEntityOrdinals(model);
            }
            data = reader.ToInstance(model, ordinals);
            return true;
        }
    }
}


