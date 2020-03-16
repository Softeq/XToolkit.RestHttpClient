using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.DefaultAuthorization
{
    internal class RefreshTokenDto
    {
        public string GrantType { get; } = "refresh_token";

        [Security]
        public string ClientId { get; set; }

        [Security]
        public string ClientSecret { get; set; }

        [Security]
        public string RefreshToken { get; set; }
    }
}
