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
            bool isBinaryContent = false,
            params HttpStatusCode[] ignoreErrorCodes);
    }
}