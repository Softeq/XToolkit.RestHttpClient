using System;
using System.Globalization;
using Plugin.SecureStorage;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.XToolkit.DefaultAuthorization
{
    public abstract class SecuredTokenManagerBase : ISecuredTokenManager
    {
        private const string TokenKey = "SessionToken";
        private const string TokenExpirationKey = "SessionTokenExpiresIn";
        private const string RefreshTokenKey = "RefreshToken";

        private static readonly IFormatProvider DateTimeFormatProvider = CultureInfo.InvariantCulture;

        private DateTime? _tokenExpirationTime;

        protected SecuredTokenManagerBase()
        {
            Token = CrossSecureStorage.Current.GetValue(TokenKey);
            _tokenExpirationTime = StringToDate(CrossSecureStorage.Current.GetValue(TokenExpirationKey));
            RefreshToken = CrossSecureStorage.Current.GetValue(RefreshTokenKey);
        }

        public string Token { get; private set; }

        public bool IsTokenExpired => !_tokenExpirationTime.HasValue ||
            _tokenExpirationTime.Value.CompareTo(DateTime.UtcNow) <= 0;

        public string RefreshToken { get; private set; }

        protected abstract string SavingResultExceptionMessage { get; }

        public void ResetTokens()
        {
            Token = null;
            _tokenExpirationTime = null;
            RefreshToken = null;

            CrossSecureStorage.Current.DeleteKey(TokenKey);
            CrossSecureStorage.Current.DeleteKey(TokenExpirationKey);
            CrossSecureStorage.Current.DeleteKey(RefreshTokenKey);
        }

        public void SaveTokens(string token, string refreshToken, int tokenExpirationTimespanInSeconds)
        {
            var tokenSavingResult = CrossSecureStorage.Current.SetValue(TokenKey, token);
            var refreshTokenSavingResult = CrossSecureStorage.Current.SetValue(RefreshTokenKey, refreshToken);

            var tokenExpirationTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(tokenExpirationTimespanInSeconds));
            var tokenExpirationSavingResult = CrossSecureStorage.Current.SetValue(TokenExpirationKey,
                DateToString(tokenExpirationTime));

            if (!tokenSavingResult || !refreshTokenSavingResult || !tokenExpirationSavingResult)
            {
                throw new Exception(SavingResultExceptionMessage);
            }

            Token = token;
            _tokenExpirationTime = tokenExpirationTime;
            RefreshToken = refreshToken;
        }

        private static string DateToString(DateTime dateTime)
        {
            return dateTime.ToString("o", DateTimeFormatProvider);
        }

        private static DateTime? StringToDate(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return null;
            }

            return DateTime.Parse(dateTimeString, DateTimeFormatProvider, DateTimeStyles.AdjustToUniversal);
        }
    }
}