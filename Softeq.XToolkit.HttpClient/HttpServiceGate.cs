using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Exceptions;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Network;

namespace Softeq.XToolkit.HttpClient
{
    public class HttpServiceGate : IHttpServiceGate
    {
        private const int DeadRequestTimeoutInMilliseconds = 10000;

        private readonly ModifiedHttpClient _client;

        public HttpServiceGate(HttpServiceGateConfig config)
        {
            var httpRequestsScheduler = new HttpRequestsScheduler(config);

            _client = new ModifiedHttpClient(httpRequestsScheduler);
        }

        public async Task<HttpResponse> ExecuteApiCallAsync(HttpRequestPriority priority, HttpRequest request,
            int timeout = 0, params HttpStatusCode[] ignoreErrorCodes)
        {
            var response = await _client.ExecuteAsStringResponseAsync(request, priority, timeout).ConfigureAwait(false);

            if (response.IsSuccessful || ignoreErrorCodes.Contains(response.StatusCode))
            {
                return response;
            }

            if (HttpStatusCodes.IsErrorStatus(response.StatusCode))
            {
                throw new HttpException($"Error status code received {response.StatusCode}", response);
            }

            return response;
        }

        public async Task<T> ExecuteApiCallAndParseAsync<T>(HttpRequestPriority priority, HttpRequest request)
        {
            var response = await ExecuteApiCallAsync(priority, request).ConfigureAwait(false);

            return response.ParseContentAsJson<T>();
        }
    }
}