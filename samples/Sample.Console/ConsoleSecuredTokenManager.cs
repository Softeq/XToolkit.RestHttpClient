using System.Threading.Tasks;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Sample.Console
{
    public class ConsoleSecuredTokenManager : ISecuredTokenManager
    {
        public string Token { get; private set; }
        public string RefreshToken { get; private set; }

        public Task SaveTokensAsync(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;

            return Task.CompletedTask;
        }

        public Task ResetTokensAsync()
        {
            Token = string.Empty;
            RefreshToken = string.Empty;
            
            return Task.CompletedTask;
        }
    }
}