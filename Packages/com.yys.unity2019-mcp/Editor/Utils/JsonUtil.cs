using Newtonsoft.Json;

namespace Unity2019Mcp.Utils
{
    public static class JsonUtil
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include
        };

        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }

        public static string ToJson(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.None, Settings);
        }
    }
}
