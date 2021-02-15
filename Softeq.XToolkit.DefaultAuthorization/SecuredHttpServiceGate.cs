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
        private readonly IHttpClient _client;

        private Task<ExecutionStatus> _refreshingTokenTask;

        public SecuredHttpServiceGate(
            ISecuredTokenManager tokenManager,
            ISessionApiService sessionApiService,
            IHttpClientProvider httpClientProvider,
            IHttpClientErrorHandler httpClientErrorHandler,
            HttpServiceGateConfig httpConfig)
        {
            _tokenManager = tokenManager;
            _sessionApiService = sessionApiService;
            _client = new ModifiedHttpClient(
                new HttpRequestsScheduler(httpClientProvider, httpClientErrorHandler, httpConfig));
        }

        async Task<HttpResponse> ISecuredHttpServiceGate.ExecuteApiCallAsync(
            HttpRequest request,
            int timeout,
            HttpRequestPriority priority,
            bool includeDefaultCredentials,
            bool isBinaryContent,
            params HttpStatusCode[] ignoreErrorCodes)
        {
            if (_tokenManager.IsTokenExpired)
            {
                return await RefreshTokenAndExecuteAsync(request, timeout, priority, ignoreErrorCodes)
                    .ConfigureAwait(false);
            }

            if (includeDefaultCredentials)
            {
                request.WithCredentials(_tokenManager);
            }

            HttpResponse response;
            if (isBinaryContent)
            {
                response = await _client.ExecuteAsBinaryResponseAsync(request, priority, timeout)
                    .ConfigureAwait(false);
            }
            else
            {
                response = await _client.ExecuteAsStringResponseAsync(request, priority, timeout)
                    .ConfigureAwait(false);
            }

            if (response == null)
            {
                HandleInvalidResponse(response);
                return response;
            }
            else if (response.IsSuccessful || ignoreErrorCodes.Contains(response.StatusCode))
            {
                return response;
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return await RefreshTokenAndExecuteAsync(request, timeout, priority, ignoreErrorCodes).ConfigureAwait(false);
            }
            else
            {
                HandleInvalidResponse(response);
                return response;
            }
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

        private async Task<HttpResponse> RefreshTokenAndExecuteAsync(
            HttpRequest request,
            int timeout,
            HttpRequestPriority priority,
            params HttpStatusCode[] ignoreErrorCodes)
        {
            await RefreshTokenAsync().ConfigureAwait(false);

            request.WithCredentials(_tokenManager);
            var response = await _client.ExecuteAsStringResponseAsync(request, priority, timeout).ConfigureAwait(false);
            if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidSessionException("Got 401 status even after refreshing access token");
            }

            if (response == null || !response.IsSuccessful && !ignoreErrorCodes.Contains(response.StatusCode))
            {
                HandleInvalidResponse(response);
            }

            return response;
        }

        private async Task RefreshTokenAsync()
        {
            ExecutionStatus refreshingTokenResult;
            if (_refreshingTokenTask == null)
            {
                _refreshingTokenTask = _sessionApiService.RefreshTokenAsync();
                refreshingTokenResult = await _refreshingTokenTask.ConfigureAwait(false);
                _refreshingTokenTask = null;
            }
            else
            {
                refreshingTokenResult = await _refreshingTokenTask.ConfigureAwait(false);
            }

            if (refreshingTokenResult == ExecutionStatus.NotCompleted)
            {
                throw new PoorInternetException("Refreshing access token failed because of connection issues");
            }
            else if (refreshingTokenResult != ExecutionStatus.Completed)
            {
                throw new InvalidSessionException("Unable to refresh access token, probably session is expired");
            }
        }
    }
}