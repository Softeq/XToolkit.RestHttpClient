using System.Threading.Tasks;
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
            if (target.CustomHeaders.ContainsKey(AuthorizationKey))
            {
                target.CustomHeaders.Remove(AuthorizationKey);
            }

            target.CustomHeaders.Add(AuthorizationKey, $"{BearerKey} {tokenManager.Token}");
            return target;
        }

        public static async Task<T> ExecuteApiCallAndParseAsync<T>(
            this ISecuredHttpServiceGate securedHttpServiceGate,
            HttpRequest request,
            HttpRequestPriority priority = HttpRequestPriority.Normal,
            bool includeDefaultCredentials = true)
        {
            var response = await securedHttpServiceGate
                .ExecuteApiCallAsync(
                    request,
                    priority: priority,
                    includeDefaultCredentials: includeDefaultCredentials)
                .ConfigureAwait(false);
            response.TryParseContentAsJson<T>(out var result);

            return result;
        }
    }
}