using System;
using System.Linq;
using System.Reflection;
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
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            };

            return JsonConvert.SerializeObject(obj, Formatting.None, parsingSettings);
        }
    }

    public class SecurityContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.AttributeProvider.GetAttributes(typeof(SecurityAttribute), false).Any())
            {
                property.ShouldSerialize =
                    instance => false;
            }

            return property;
        }
    }
}
