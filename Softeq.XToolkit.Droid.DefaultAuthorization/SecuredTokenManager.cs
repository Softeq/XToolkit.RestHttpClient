using System;
using System.Threading.Tasks;
using Plugin.SecureStorage;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.XToolkit.Droid.DefaultAuthorization
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

        public Task ResetTokensAsync()
        {
            Token = null;
            RefreshToken = null;

            CrossSecureStorage.Current.DeleteKey(SESSION_TOKEN_KEY);
            CrossSecureStorage.Current.DeleteKey(REFRESH_TOKEN_KEY);

            return Task.CompletedTask;
        }

        public Task SaveTokensAsync(string token, string refreshToken)
        {
            var tokenSavingResult = CrossSecureStorage.Current.SetValue(SESSION_TOKEN_KEY, token);
            var refreshTokenSavingResult = CrossSecureStorage.Current.SetValue(REFRESH_TOKEN_KEY, refreshToken);

            if (!tokenSavingResult || !refreshTokenSavingResult)
            {
                throw new Exception("Something go wrong, please recheck project settings");
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