using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Softeq.XToolkit.DefaultAuthorization.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string AuthorizationKey = "Authorization";
        private const string BearerKey = "Bearer";

        public static HttpRequest WithCredentials(this HttpRequest target, ISecuredTokenManager tokenManager)
        {
            target.CustomHeaders.Add(AuthorizationKey, $"{BearerKey} {tokenManager.Token}");
            return target;
        }
    }
}
