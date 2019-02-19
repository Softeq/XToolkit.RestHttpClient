using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Softeq.XToolkit.CrossCutting
{
    public static class JsonConverter
    {
        public static T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static bool TryDeserialize<T>(string jsonString, out T result)
        {
            try
            {
                result = Deserialize<T>(jsonString);
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        public static string Serialize(object obj, bool shouldIgnoreNullValue = false)
        {
            var parsingSettings = new JsonSerializerSettings
            {
                NullValueHandling = shouldIgnoreNullValue ? NullValueHandling.Ignore : NullValueHandling.Include,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return JsonConvert.SerializeObject(obj, Formatting.None, parsingSettings);
        }
    }
}
