// Developed for LilBytes by Softeq Development Corporation
//

using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.HttpClient;

namespace Softeq.XToolkit.DefaultAuthorization.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string AuthorizationKey = "Authorization";
        private const string BearerKey = "Bearer";

        public static HttpRequest WithCredentials(this HttpRequest target, IMembershipService membershipService)
        {
            target.CustomHeaders.Add(AuthorizationKey, $"{BearerKey} {membershipService.Token}");
            return target;
        }
    }
}
