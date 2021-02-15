using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.HttpClient.Abstract
{
    public interface IHttpServiceGate
    {
        Task<HttpResponse> ExecuteApiCallAsync(
            HttpRequest request,
            int timeout = 0,
            HttpRequestPriority priority = HttpRequestPriority.Normal,
            bool isBinaryContent = false,
            params HttpStatusCode[] ignoreErrorCodes);
    }
}