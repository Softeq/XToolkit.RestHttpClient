using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Exceptions;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Infrastructure;
using Softeq.XToolkit.HttpClient.Network;

namespace Softeq.XToolkit.HttpClient
{
    public class HttpServiceGate : IHttpServiceGate
    {
        private readonly ModifiedHttpClient _client;

        public HttpServiceGate(
            IHttpClientProvider httpClientProvider,
            IHttpClientErrorHandler httpClientErrorHandler,
            HttpServiceGateConfig httpConfig)
        {
            _client = new ModifiedHttpClient(
                new HttpRequestsScheduler(httpClientProvider, httpClientErrorHandler, httpConfig));
        }

        async Task<HttpResponse> IHttpServiceGate.ExecuteApiCallAsync(
            HttpRequest request,
            int timeout,
            HttpRequestPriority priority,
            params HttpStatusCode[] ignoreErrorCodes)
        {
            var response = await _client.ExecuteAsStringResponseAsync(request, priority, timeout)
                .ConfigureAwait(false);

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
    }
}