using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Softeq.XToolkit.DefaultAuthorization.Infrastructure
{
    public class CustomLoginErrorData
    {
        public CustomLoginErrors Error { get; set; }

        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }
        
        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }
    }

    public enum CustomLoginErrors
    {
        [EnumMember(Value = "invalid_grant")]
        InvalidGrant
    }
}
