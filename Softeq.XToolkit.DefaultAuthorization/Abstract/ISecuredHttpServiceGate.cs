using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.DefaultAuthorization.Abstract
{
    public interface ISecuredHttpServiceGate
    {
        Task<HttpResponse> ExecuteApiCallAsync(HttpRequest request,
            int timeout = 0, HttpRequestPriority priority = HttpRequestPriority.Normal,
            bool includeDefaultCredentials = true,
            params HttpStatusCode[] ignoreErrorCodes);

        Task<T> ExecuteApiCallAndParseAsync<T>(HttpRequest request,
            HttpRequestPriority priority = HttpRequestPriority.Normal, bool includeDefaultCredentials = true);
        void SetHttpConfig(HttpServiceGateConfig httpConfig);
    }
}