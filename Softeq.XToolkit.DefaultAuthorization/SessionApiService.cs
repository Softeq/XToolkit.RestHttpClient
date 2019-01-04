using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.HttpClient;
using System.Net;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.DefaultAuthorization.Infrastructure;

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
        private const string ApplicationJsonContentType = "application/json";
        private const int RetryNumber = 3;

        private readonly AuthConfig _authConfig;
        private readonly HttpServiceGate _httpClient;
        private readonly ISecuredTokenManager _tokenService;
        private readonly ApiEndpoints _apiEndpoints;

        public SessionApiService(AuthConfig authConfig, HttpServiceGateConfig httpConfig, ISecuredTokenManager tokenService)
        {
            _authConfig = authConfig;
            _httpClient = new HttpServiceGate(httpConfig);
            _tokenService = tokenService;
            _apiEndpoints = new ApiEndpoints(authConfig.BaseUrl);
        }

        public async Task<ExecutionStatus> LoginAsync(string login, string password)
        {
            var result = ExecutionStatus.Failed;

            _tokenService.ResetTokens();

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

                    await _tokenService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken)
                        .ConfigureAwait(false);

                    result = ExecutionStatus.Completed;
                }
            }, RetryNumber);

            return result;
        }

        public async Task<ExecutionStatus> RefreshTokenAsync()
        {
            var result = ExecutionStatus.Failed;

            _tokenService.ResetTokens();

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

                    await _tokenService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken)
                        .ConfigureAwait(false);
                    result = ExecutionStatus.Completed;
                }
            }, RetryNumber);

            return result;
        }

        public Task LogoutAsync()
        {
            _tokenService.ResetTokens();
            return Task.CompletedTask;
        }

        public async Task<RegistrationStatus> RegisterAccount(string login, string password)
        {
            var result = RegistrationStatus.Undefined;

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.Register())
                    .WithData(JsonConverter.Serialize(new
                    {
                        email = login,
                        password = password,
                        isAcceptedTermsOfService = true
                    }));

                request.ContentType = ApplicationJsonContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request, 0, HttpStatusCode.Conflict)
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result = RegistrationStatus.Succussfull;
                }
                else
                {
                    result = HandleRegistrationError(response.Content);
                }
            }, RetryNumber);

            return result;
        }

        public async Task<ExecutionStatus> ForgotPassword(string login)
        {
            var result = ExecutionStatus.Failed;

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.Register())
                    .WithData(JsonConverter.Serialize(new { email = login }));

                request.ContentType = ApplicationJsonContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request)
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result = ExecutionStatus.Completed;
                }
            }, RetryNumber);

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
                {RefreshTokenKey, _tokenService.RefreshToken},
                {ClientSecretKey, _authConfig.ClientSecret},
                {GrantTypeKey, RefreshTokenKey}
            }).ReadAsStringAsync();
        }

        private RegistrationStatus HandleRegistrationError(string responseContent)
        {
            if (string.IsNullOrEmpty(responseContent))
            {
                return RegistrationStatus.Failed;
            }

            var data = JsonConverter.Deserialize<List<ErrorData>>(responseContent);

            if (data == null || data.Count == 0)
            {
                return RegistrationStatus.Failed;
            }
            if (data[0].Code == 5001)
            {
                return RegistrationStatus.UserAlreadyExists;
            }

            return RegistrationStatus.Failed;
        }
    }
}