using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.HttpClient.Abstract
{
    public interface IHttpServiceGate
    {
        Task<HttpResponse> ExecuteApiCallAsync(HttpRequestPriority priority,
            HttpRequest request,
            int timeout = 0, params HttpStatusCode[] ignoreErrorCodes);

        Task<T> ExecuteApiCallAndParseAsync<T>(HttpRequestPriority priority, HttpRequest request);
    }
}