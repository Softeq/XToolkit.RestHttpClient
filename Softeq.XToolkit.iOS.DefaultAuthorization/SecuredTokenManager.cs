using System;
using System.Threading.Tasks;
using CoreFoundation;
using Plugin.SecureStorage;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.iOS.DefaultAuthorization
{
    public class SecuredTokenManager : ISecuredTokenManager
    {
        private const string SESSION_TOKEN_KEY = "SessionToken";
        private const string REFRESH_TOKEN_KEY = "RefreshToken";
        private const string TOKENS_MARKED_FOR_DELETION_KEY = "TokensMarkedForDeletion";

        public string Token { get; private set; }
        public string RefreshToken { get; private set; }
        public bool AreTokensMarkedForDeletion { get; private set; }

        public SecuredTokenManager()
        {
            RestoreTokens();
        }

        public Task ResetTokensAsync(bool shouldDeleteTokensPermanently = true)
        {
            if (shouldDeleteTokensPermanently)
            {
                UpdateTokens(null, null, false);

                CrossSecureStorage.Current.DeleteKey(SESSION_TOKEN_KEY);
                CrossSecureStorage.Current.DeleteKey(REFRESH_TOKEN_KEY);
                CrossSecureStorage.Current.DeleteKey(TOKENS_MARKED_FOR_DELETION_KEY);
            }
            else
            {
                AreTokensMarkedForDeletion = true;
                CrossSecureStorage.Current.SetValue(TOKENS_MARKED_FOR_DELETION_KEY, "true");
            }

            return Task.CompletedTask;
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

            UpdateTokens(token, refreshToken, AreTokensMarkedForDeletion);

            return Task.CompletedTask;
        }

        private void RestoreTokens()
        {
            UpdateTokens(CrossSecureStorage.Current.GetValue(SESSION_TOKEN_KEY),
                         CrossSecureStorage.Current.GetValue(REFRESH_TOKEN_KEY),
                         CrossSecureStorage.Current.HasKey(TOKENS_MARKED_FOR_DELETION_KEY));
        }

        private void UpdateTokens(string token, string refreshToken, bool areTokensMarkedForDeletion)
        {
            Token = token;
            RefreshToken = refreshToken;
            AreTokensMarkedForDeletion = areTokensMarkedForDeletion;
        }
    }
}