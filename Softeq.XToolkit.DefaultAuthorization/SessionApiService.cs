using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Softeq.HttpClient.Common;
using Softeq.HttpClient.Common.Executor;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.HttpClient;

namespace Softeq.XToolkit.DefaultAuthorization
{
    public class SessionApiService
    {
        private const string GrantTypeKey = "grant_type";
        private const string PasswordKey = "password";
        private const string ClientIdKey = "client_id";
        private const string ClientSecretKey = "client_secret";
        private const string UsernameKey = "username";
        private const string RefreshTokenKey = "refresh_token";
        private const string ContentType = "application/x-www-form-urlencoded";

        private readonly AuthConfig _authConfig;
        private readonly HttpServiceGate _httpClient;
        private readonly IMembershipService _membershipService;
        private readonly ApiEndpoints _apiEndpoints;

        public SessionApiService(AuthConfig authConfig, HttpServiceGateConfig httpConfig,
            IMembershipService membershipService)
        {
            _authConfig = authConfig;
            _httpClient = new HttpServiceGate(httpConfig);
            _membershipService = membershipService;
            _apiEndpoints = new ApiEndpoints(authConfig.BaseUrl);
        }

        public async Task<ExecutionStatus> LoginAsync(string login, string password)
        {
            var result = ExecutionStatus.Failed;

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.Login())
                    .WithData(await GetLogInContent(login, password).ConfigureAwait(false));

                request.ContentType = ContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request)
                    .ConfigureAwait(false);

                if (response.IsSuccessful && response.Content != null)
                {
                    var tokens = JsonConverter.Deserialize<LoginData>(response.Content);

                    await _membershipService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken)
                        .ConfigureAwait(false);

                    result = ExecutionStatus.Completed;
                }
            }, 3);

            return result;
        }

        public async Task<ExecutionStatus> RefreshTokenAsync()
        {
            var result = ExecutionStatus.Failed;

            _membershipService.ResetTokens();

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.RefreshToken())
                    .WithData(await GetRefreshTokenRequestDataAsync().ConfigureAwait(false));

                request.ContentType = ContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request)
                    .ConfigureAwait(false);

                if (response.IsSuccessful && response.Content != null)
                {
                    var tokens = JsonConverter.Deserialize<LoginData>(response.Content);

                    await _membershipService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken)
                        .ConfigureAwait(false);
                    result = ExecutionStatus.Completed;
                }
            }, 3);

            return result;
        }

        public async Task<ExecutionStatus> RegisterAccount(string login, string password)
        {
            var result = ExecutionStatus.Failed;

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.Register());

                request.ContentType = ContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request)
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result = ExecutionStatus.Completed;
                }
            }, 3);

            return result;
        }

        private Task<string> GetLogInContent(string login, string password)
        {
            var dict = new Dictionary<string, string>
            {
                {GrantTypeKey, PasswordKey},
                {ClientIdKey, _authConfig.ClientId},
                {ClientSecretKey, _authConfig.ClientSecret},
                {UsernameKey, login},
                {PasswordKey, password}
            };

            return new FormUrlEncodedContent(dict).ReadAsStringAsync();
        }

        private Task<string> GetRefreshTokenRequestDataAsync()
        {
            return new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {ClientIdKey, _authConfig.ClientId},
                {RefreshTokenKey, _membershipService.RefreshToken},
                {ClientSecretKey, _authConfig.ClientSecret},
                {GrantTypeKey, RefreshTokenKey}
            }).ReadAsStringAsync();
        }
    }
}