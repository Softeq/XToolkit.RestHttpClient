// Developed for LilBytes by Softeq Development Corporation
//

using Newtonsoft.Json;

namespace Softeq.XToolkit.DefaultAuthorization
{
    public class LoginResultDto
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
