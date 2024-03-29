﻿using System;
using Plugin.SecureStorage;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.XToolkit.DefaultAuthorization
{
    public abstract class SecuredTokenManagerBase : ISecuredTokenManager
    {
        private const string TokenKey = "SessionToken";
        private const string TokenExpirationKey = "SessionTokenExpiresIn";
        private const string RefreshTokenKey = "RefreshToken";

        private DateTime? _tokenExpirationTime;
        private string _token;

        protected SecuredTokenManagerBase()
        {
            Token = CrossSecureStorage.Current.GetValue(TokenKey);
            _tokenExpirationTime = DateTimeToSerializedStringConverter.StringToDate(
                CrossSecureStorage.Current.GetValue(TokenExpirationKey));
            RefreshToken = CrossSecureStorage.Current.GetValue(RefreshTokenKey);
        }

        public event EventHandler<string> TokenChanged;

        public string Token
        {
            get => _token;
            private set
            {
                _token = value;
                TokenChanged?.Invoke(this, _token);
            }
        }

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
            if (tokenExpirationTimespanInSeconds == 0)
            {
                tokenExpirationTime = DateTime.MaxValue;
            }

            var tokenExpirationSavingResult = CrossSecureStorage.Current.SetValue(TokenExpirationKey,
                DateTimeToSerializedStringConverter.DateToString(tokenExpirationTime));

            if (!tokenSavingResult || !refreshTokenSavingResult || !tokenExpirationSavingResult)
            {
                throw new Exception(SavingResultExceptionMessage);
            }

            Token = token;
            _tokenExpirationTime = tokenExpirationTime;
            RefreshToken = refreshToken;
        }
    }
}