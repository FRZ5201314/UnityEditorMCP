using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
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
            if (value is JToken)
            {
                return ((JToken)value).ToObject<T>();
            }

            if (value is T)
            {
                return (T)value;
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
    }
}
