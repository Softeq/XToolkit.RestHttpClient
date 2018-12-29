using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Softeq.HttpClient.Common;
using Softeq.HttpClient.Common.Exceptions;
using Softeq.HttpClient.Common.Executor;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.DefaultAuthorization.Extensions;
using Softeq.XToolkit.HttpClient.Network;

namespace Softeq.XToolkit.DefaultAuthorization
{
    public class SecuredHttpServiceGate
    {
        private readonly ITokenManager _membershipService;
        private readonly SessionApiService _sessionApiService;
        private ForegroundTaskDeferral _sessionRetrievalDeferral;
        private readonly ModifiedHttpClient _client;

        public SecuredHttpServiceGate(SessionApiService sessionApiService, HttpServiceGateConfig config, ITokenManager membershipService)
        {
            var httpRequestsScheduler = new HttpRequestsScheduler(config);

            _client = new ModifiedHttpClient(httpRequestsScheduler);

            _sessionApiService = sessionApiService;

            _membershipService = membershipService;

            _sessionRetrievalDeferral = new ForegroundTaskDeferral();
        }

        public async Task<HttpResponse> ExecuteApiCallAsync(HttpRequestPriority priority, HttpRequest request, int timeout = 0, params HttpStatusCode[] ignoreErrorCodes)
        {
            var response = await _client.ExecuteAsStringResponseAsync(priority, request, timeout).ConfigureAwait(false);

            if (ValidateResponse(response, true, ignoreErrorCodes))
            {
                return response;
            }

            //try to retrive session again if token is not valid
            if (!IsSessionValid(response))
            {
                //if session request is already in progress, then await it
                var shouldRetrieveSession = await EnsureNoSessionRetrievalIsRunningAsync().ConfigureAwait(false);

                if (shouldRetrieveSession)
                {
                    var executionStatus = await RetrieveSessionAsync().ConfigureAwait(false);

                    if (executionStatus == ExecutionStatus.Completed)
                    {
                        request.WithCredentials(_membershipService);
                        response = await _client.ExecuteAsStringResponseAsync(priority, request).ConfigureAwait(false);
                    }
                }
                else
                {
                    request.WithCredentials(_membershipService);

                    response = await _client.ExecuteAsStringResponseAsync(priority, request).ConfigureAwait(false);
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

        private bool ValidateResponse(HttpResponse response, bool shouldCheckIfForbidden = false, params HttpStatusCode[] ignoreErrorCodes)
        {
            if (response.IsSuccessful || ignoreErrorCodes.Contains(response.StatusCode))
            {
                return true;
            }

            if (shouldCheckIfForbidden && !IsSessionValid(response))
            {
                return false;
            }

            if (HttpStatusCodes.IsErrorStatus(response.StatusCode))
            {
                throw new HttpException("Error status code recieved", response);
            }

            return true;
        }

        public async Task<T> ExecuteApiCallAndParseAsync<T>(HttpRequestPriority priority, HttpRequest request)
        {
            var response = await ExecuteApiCallAsync(priority, request).ConfigureAwait(false);

            return response.ParseContentAsJson<T>();
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
