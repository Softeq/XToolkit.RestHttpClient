using System;
using Softeq.XToolkit.CrossCutting;
using Api = Softeq.XToolkit.DefaultAuthorization.Defines.Api;

namespace Softeq.XToolkit.DefaultAuthorization
{
    internal class ApiEndpoints
    {
        private readonly string _baseUrl;
        private readonly UriMaker _uriMaker;

        public ApiEndpoints(string baseUrl)
        {
            _baseUrl = baseUrl;
            _uriMaker = new UriMaker();
        }

        public Uri RefreshToken()
        {
            return _uriMaker.Combine(_baseUrl, Api.Connect.ApiRoot, Api.Connect.Token.ApiRoot);
        }

        public Uri Login()
        {
            return _uriMaker.Combine(_baseUrl, Api.Connect.ApiRoot, Api.Connect.Token.ApiRoot);
        }

        public Uri Register()
        {
            return _uriMaker.Combine(_baseUrl, Api.Account.ApiRoot, Api.Account.Register.ApiRoot);
        }

        public Uri ForgotPassword()
        {
            return _uriMaker.Combine(_baseUrl, Api.Account.ApiRoot, Api.Account.ForgotPassword.ApiRoot);
        }

        public Uri ResendConfirmationEmail()
        {
            return _uriMaker.Combine(_baseUrl, Api.Account.ApiRoot, Api.Account.ResendConfirmationEmail.ApiRoot);
        }
    }
}