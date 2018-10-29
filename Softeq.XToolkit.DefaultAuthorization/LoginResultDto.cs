// Developed for LilBytes by Softeq Development Corporation
//

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Softeq.XToolkit.DefaultAuthorization
{
    [DataContract]
    public class LoginResultDto
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
