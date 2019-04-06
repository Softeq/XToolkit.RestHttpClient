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
        private ForegroundTaskDeferral _sessionRetrievalDeferral;
        private IHttpClient _client;

        public SecuredHttpServiceGate(ISessionApiService sessionApiService, HttpServiceGateConfig httpConfig,
            ISecuredTokenManager tokenManager)
        {
            _tokenManager = tokenManager;
            _sessionApiService = sessionApiService;
            _sessionRetrievalDeferral = new ForegroundTaskDeferral();
            _client = new ModifiedHttpClient(new HttpRequestsScheduler(httpConfig));
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

            if (ValidateResponse(response, true, ignoreErrorCodes))
            {
                return response;
            }

            //try to retrieve session again if token is not valid
            if (!IsSessionValid(response))
            {
                //if session request is already in progress, then await it
                var shouldRetrieveSession = await EnsureNoSessionRetrievalIsRunningAsync().ConfigureAwait(false);

                if (shouldRetrieveSession)
                {
                    var executionStatus = await RetrieveSessionAsync().ConfigureAwait(false);

                    if (executionStatus == ExecutionStatus.Completed)
                    {
                        request.WithCredentials(_tokenManager);
                        response = await _client.ExecuteAsStringResponseAsync(request, priority).ConfigureAwait(false);
                    }
                }
                else
                {
                    request.WithCredentials(_tokenManager);

                    response = await _client.ExecuteAsStringResponseAsync(request, priority).ConfigureAwait(false);
                }

                //if session issue is still not resolved, redirect to first install login screen, because we can't perform activities without session
                if (!IsSessionValid(response))
                {
                    return response;
                }
            }

            ValidateResponse(response, ignoreErrorCodes: ignoreErrorCodes);

            return response;
        }

        public async Task<T> ExecuteApiCallAndParseAsync<T>(HttpRequest request,
            HttpRequestPriority priority = HttpRequestPriority.Normal, bool includeDefaultCredentials = true)
        {
            var response =
                await ExecuteApiCallAsync(request, priority: priority,
                    includeDefaultCredentials: includeDefaultCredentials).ConfigureAwait(false);

            response.TryParseContentAsJson<T>(out T result);
            return result;
        }

        private bool ValidateResponse(HttpResponse response, bool shouldCheckIfForbidden = false,
            params HttpStatusCode[] ignoreErrorCodes)
        {
            if (response.IsSuccessful || ignoreErrorCodes.Contains(response.StatusCode))
            {
                return true;
            }

            if (shouldCheckIfForbidden && !IsSessionValid(response))
            {
                return false;
            }

            if (response.IsPoorConnection)
            {
                return false;
            }

            if (HttpStatusCodes.IsErrorStatus(response.StatusCode))
            {
                throw new HttpException("Error status code received", response);
            }

            return true;
        }

        private async Task<ExecutionStatus> RetrieveSessionAsync()
        {
            var deferral = CreateSessionRetrievalDeferral();

            deferral.Begin();

            var result = await _sessionApiService.RefreshTokenAsync().ConfigureAwait(false);

            deferral.Complete();

            return result;
        }

        private ForegroundTaskDeferral CreateSessionRetrievalDeferral()
        {
            _sessionRetrievalDeferral = new ForegroundTaskDeferral();

            return _sessionRetrievalDeferral;
        }

        private async Task<bool> EnsureNoSessionRetrievalIsRunningAsync()
        {
            if (!_sessionRetrievalDeferral.IsInProgress)
            {
                return true;
            }

            await _sessionRetrievalDeferral.WaitForCompletionAsync();

            return false;
        }

        private bool IsSessionValid(HttpResponse response)
        {
            if (response == null)
            {
                return false;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return false;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return false;
            }

            return true;
        }
    }
}