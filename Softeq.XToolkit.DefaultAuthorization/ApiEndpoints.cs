// Developed for LilBytes by Softeq Development Corporation
//

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
            return _uriMaker.Combine(_baseUrl, Defines.Api.Connect.ApiEntry, Defines.Api.Connect.Token.ApiEntry);
        }

        public Uri Login()
        {
            return _uriMaker.Combine(_baseUrl, Defines.Api.Connect.ApiEntry, Defines.Api.Connect.Token.ApiEntry);
        }
    }
}
