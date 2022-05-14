using System;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Sample.Console
{
    public class ConsoleSecuredTokenManager : ISecuredTokenManager
    {
        private DateTimeOffset _tokenExpiration;
        
        public event EventHandler<string> TokenChanged;
        public string Token { get; private set; }
        public string RefreshToken { get; private set; }
        
        public bool IsTokenExpired => _tokenExpiration > DateTimeOffset.Now;
        
        public void SaveTokens(string token, string refreshToken, int tokenExpirationTimespanInSeconds)
        {
            Token = token;
            RefreshToken = refreshToken;
            _tokenExpiration = DateTimeOffset.Now + TimeSpan.FromSeconds(tokenExpirationTimespanInSeconds);
        }

        public void ResetTokens()
        {
            Token = string.Empty;
            RefreshToken = string.Empty;
            _tokenExpiration = DateTimeOffset.Now;
        }
    }
}