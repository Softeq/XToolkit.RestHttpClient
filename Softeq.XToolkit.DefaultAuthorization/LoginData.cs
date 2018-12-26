using System.Runtime.Serialization;

namespace Softeq.XToolkit.DefaultAuthorization
{
    [DataContract]
    public class LoginData
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
}
