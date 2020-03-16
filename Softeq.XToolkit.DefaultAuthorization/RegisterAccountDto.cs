using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.DefaultAuthorization
{
    internal class RegisterAccountDto
    {
        public string Email { get; set; }

        [Security]
        public string Password { get; set; }

        public bool IsAcceptedTermsOfService { get; } = true;
    }
}
