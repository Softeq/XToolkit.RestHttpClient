using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.DefaultAuthorization
{
    internal class LoginDto
    {
        public string GrantType { get; } = "password";

        [Security]
        public string ClientId { get; set; }

        [Security]
        public string ClientSecret { get; set; }

        public string Username { get; set; }

        [Security]
        public string Password { get; set; }
    }
}
