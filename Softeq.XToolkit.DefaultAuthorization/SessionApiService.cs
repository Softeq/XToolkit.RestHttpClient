// Developed for LilBytes by Softeq Development Corporation
//

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.HttpClient;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Enums;
using Softeq.XToolkit.HttpClient.Infrastructure;

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

        private readonly AuthConfig _authConfig;
        private readonly IExecutor _executor;
        private readonly HttpServiceGate _httpClient;
        private readonly IMembershipService _membershipService;
        private readonly ApiEndpoints _apiEndpoints;

        public SessionApiService(AuthConfig authConfig, HttpServiceGate httpServiceGate, IMembershipService membershipService, IExecutor executor)
        {
            _executor = executor;
            _authConfig = authConfig;
            _httpClient = httpServiceGate;
            _membershipService = membershipService;
            _apiEndpoints = new ApiEndpoints(authConfig.BaseUrl);
        }

        public async Task<ExecutionStatus> LoginAsync(string login, string password)
        {
            ExecutionStatus result = ExecutionStatus.Failed;

            await _executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.Login())
                    .WithData(await LoginContent(login, password));

                request.ContentType = "application/x-www-form-urlencoded";

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request).ConfigureAwait(false);

                if (response.IsSuccessful && response.Content != null)
                {
                    var tokens = JsonConverter.Deserialize<LoginResultDto>(response.Content);

                    await _membershipService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken);

                    result = ExecutionStatus.Completed;
                }
            }, 3);

            return result;
        }

        public async Task<ExecutionStatus> RefreshTokenAsync()
        {
            ExecutionStatus result = ExecutionStatus.Failed;

            _membershipService.ResetTokens();

            await _executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.RefreshToken())
                    .WithData(await GetRefreshTokenRequestDataAsync());

                request.ContentType = "application/x-www-form-urlencoded";

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request).ConfigureAwait(false);

                if (response.IsSuccessful && response.Content != null)
                {
                    var tokens = JsonConverter.Deserialize<LoginResultDto>(response.Content);

                    await _membershipService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken);
                    result = ExecutionStatus.Completed;
                }
            }, 3);

            return result;
        }

        private Task<string> LoginContent(string login, string password)
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