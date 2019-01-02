using System;
using System.Threading.Tasks;
using Plugin.SecureStorage;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.XToolkit.iOS.DefaultAuthorization
{
    public class SecuredTokenManager : ISecuredTokenManager
    {
        private const string SESSION_TOKEN_KEY = "SessionToken";
        private const string REFRESH_TOKEN_KEY = "RefreshToken";

        public string Token { get; private set; }
        public string RefreshToken { get; private set; }

        public SecuredTokenManager()
        {
            RestoreTokens();
        }

        public void ResetTokens()
        {
            Token = null;
            RefreshToken = null;

            CrossSecureStorage.Current.DeleteKey(SESSION_TOKEN_KEY);
            CrossSecureStorage.Current.DeleteKey(REFRESH_TOKEN_KEY);
        }

        public Task SaveTokensAsync(string token, string refreshToken)
        {
            var tokenSavingResult = CrossSecureStorage.Current.SetValue(SESSION_TOKEN_KEY, token);
            var refreshTokenSavingResult = CrossSecureStorage.Current.SetValue(REFRESH_TOKEN_KEY, refreshToken);

            if (!tokenSavingResult || !refreshTokenSavingResult)
            {
                throw new Exception(
                    "Please check iOS settings by the following link: https://github.com/sameerkapps/SecureStorage/issues/31#issuecomment-366205742");
            }

            Token = token;
            RefreshToken = refreshToken;

            return Task.CompletedTask;
        }

        public Task RestoreTokens()
        {
            Token = CrossSecureStorage.Current.GetValue(SESSION_TOKEN_KEY);
            RefreshToken = CrossSecureStorage.Current.GetValue(REFRESH_TOKEN_KEY);

            return Task.CompletedTask;
        }
    }
}