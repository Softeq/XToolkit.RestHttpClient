using System;
using Softeq.HttpClient.Common;

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
            return _uriMaker.Combine(_baseUrl, Defines.Api.Connect.ApiRoot, Defines.Api.Connect.Token.ApiRoot);
        }

        public Uri Login()
        {
            return _uriMaker.Combine(_baseUrl, Defines.Api.Connect.ApiRoot, Defines.Api.Connect.Token.ApiRoot);
        }

        public Uri Register()
        {
            return _uriMaker.Combine(_baseUrl, Defines.Api.ApiRoot, Defines.Api.Account.ApiRoot,
                Defines.Api.Account.Register.ApiRoot);
        }
    }
}