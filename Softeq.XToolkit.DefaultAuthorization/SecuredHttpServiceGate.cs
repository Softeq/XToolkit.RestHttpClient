using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Exceptions;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.DefaultAuthorization.Extensions;
using Softeq.XToolkit.HttpClient.Infrastructure;
using Softeq.XToolkit.HttpClient.Network;

namespace Softeq.XToolkit.DefaultAuthorization
{
    public class SecuredHttpServiceGate : ISecuredHttpServiceGate
    {
        private readonly ISecuredTokenManager _tokenManager;
        private readonly ISessionApiService _sessionApiService;
        private ForegroundTaskDeferral<ExecutionStatus> _sessionRetrievalDeferral;
        private IHttpClient _client;

        public SecuredHttpServiceGate(
            ISessionApiService sessionApiService, 
            HttpServiceGateConfig httpConfig,
            ISecuredTokenManager tokenManager,
            IHttpClientProvider httpClientProvider)
        {
            _tokenManager = tokenManager;
            _sessionApiService = sessionApiService;
            _sessionRetrievalDeferral = new ForegroundTaskDeferral<ExecutionStatus>();
            _client = new ModifiedHttpClient(new HttpRequestsScheduler(httpClientProvider, httpConfig));
        }

        public async Task<HttpResponse> ExecuteApiCallAsync(HttpRequest request,
            int timeout = 0, HttpRequestPriority priority = HttpRequestPriority.Normal,
            bool includeDefaultCredentials = true,
            params HttpStatusCode[] ignoreErrorCodes)
        {
            if (includeDefaultCredentials)
            {
                //add credentials to every request using this approach
                request.WithCredentials(_tokenManager);
            }

            var response = await _client.ExecuteAsStringResponseAsync(request, priority, timeout).ConfigureAwait(false);

            if (response == null)
            {
                HandleInvalidResponse(response);
                return response;
            }
            if (response.IsSuccessful || ignoreErrorCodes.Contains(response.StatusCode))
            {
                return response;
            }

            //try to retrieve session again if token is not valid
            if (!IsSessionValid(response))
            {
                //if session request is already in progress, then await it
                var sessionRetrievalResult = await EnsureNoSessionRetrievalIsRunningAsync().ConfigureAwait(false);

                if (!sessionRetrievalResult.HasValue)
                {
                    sessionRetrievalResult = await RetrieveSessionAsync().ConfigureAwait(false);
                }
                if (sessionRetrievalResult == ExecutionStatus.Completed)
                {
                    request.WithCredentials(_tokenManager);
                    response = await _client.ExecuteAsStringResponseAsync(request, priority).ConfigureAwait(false);
                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new InvalidSessionException("Got 401 status even after refreshing access token");
                    }
                    if (response == null || !response.IsSuccessful && !ignoreErrorCodes.Contains(response.StatusCode))
                    {
                        HandleInvalidResponse(response);
                    }
                }
                else if (sessionRetrievalResult != ExecutionStatus.NotCompleted)
                {
                    throw new InvalidSessionException("Unable to refresh access token, probably session is expired", response);
                }
                else
                {
                    throw new PoorInternetException("Refreshing access token failed because of connection issues");
                }
            }
            else
            {
                HandleInvalidResponse(response);
            }

            return response;
        }

        protected virtual void HandleInvalidResponse(HttpResponse response)
        {
            if (response == null)
            {
                throw new HttpException("Response is null!");
            }
            if (HttpStatusCodes.IsErrorStatus(response.StatusCode))
            {
                throw new HttpException($"Error status code received {response.StatusCode}", response);
            }
        }

        private async Task<ExecutionStatus> RetrieveSessionAsync()
        {
            var deferral = CreateSessionRetrievalDeferral();

            deferral.Begin();

            var result = await _sessionApiService.RefreshTokenAsync().ConfigureAwait(false);

            deferral.Complete(result);

            return result;
        }

        private ForegroundTaskDeferral<ExecutionStatus> CreateSessionRetrievalDeferral()
        {
            _sessionRetrievalDeferral = new ForegroundTaskDeferral<ExecutionStatus>();

            return _sessionRetrievalDeferral;
        }

        private async Task<ExecutionStatus?> EnsureNoSessionRetrievalIsRunningAsync()
        {
            if (!_sessionRetrievalDeferral.IsInProgress)
            {
                return null;
            }

            return await _sessionRetrievalDeferral.WaitForCompletionAsync();
        }

        private static bool IsSessionValid(HttpResponse response)
        {
            return response.StatusCode != HttpStatusCode.Unauthorized;
        }
    }
}