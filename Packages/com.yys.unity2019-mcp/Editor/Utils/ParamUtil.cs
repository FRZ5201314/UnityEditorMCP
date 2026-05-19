using System;
using System.Collections;
using System.Collections.Generic;
using Unity2019Mcp.Models;

namespace Unity2019Mcp.Utils
{
    public static class ParamUtil
    {
        public static T Get<T>(Dictionary<string, object> parameters, string key, T fallback)
        {
            if (parameters == null || !parameters.ContainsKey(key) || parameters[key] == null)
            {
                return fallback;
            }

            var value = parameters[key];
            if (value is T)
            {
                return (T)value;
            }

            if (typeof(T) == typeof(string[]))
            {
                return (T)(object)ToStringArray(value);
            }

            if (typeof(T) == typeof(Vector3Dto))
            {
                return (T)(object)ToVector3Dto(value);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static string RequiredString(Dictionary<string, object> parameters, string key)
        {
            var value = Get<string>(parameters, key, null);
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Missing required parameter: " + key);
            }

            return value;
        }

        public static Vector3Dto Vector3(Dictionary<string, object> parameters, string key)
        {
            return Get<Vector3Dto>(parameters, key, null);
        }

        private static string[] ToStringArray(object value)
        {
            var list = value as IList;
            if (list == null)
            {
                return null;
            }

            var result = new string[list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                result[i] = list[i] == null ? null : Convert.ToString(list[i]);
            }

            return result;
        }

        private static Vector3Dto ToVector3Dto(object value)
        {
            var dictionary = value as IDictionary;
            if (dictionary == null)
            {
                return null;
            }

            return new Vector3Dto
            {
                x = ReadFloat(dictionary, "x"),
                y = ReadFloat(dictionary, "y"),
                z = ReadFloat(dictionary, "z")
            };
        }

        private static float ReadFloat(IDictionary dictionary, string key)
        {
            return dictionary.Contains(key) && dictionary[key] != null ? Convert.ToSingle(dictionary[key]) : 0f;
        }
    }
}
