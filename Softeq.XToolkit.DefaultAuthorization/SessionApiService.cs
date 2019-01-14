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
        private const string ApplicationFormContentType = "application/x-www-form-urlencoded";
        private const string ApplicationJsonContentType = "application/json";
        private const int RetryNumber = 3;

        private readonly AuthConfig _authConfig;
        private readonly HttpServiceGate _httpClient;
        private readonly ISecuredTokenManager _tokenService;
        private readonly ApiEndpoints _apiEndpoints;

        public SessionApiService(AuthConfig authConfig, HttpServiceGateConfig httpConfig,
            ISecuredTokenManager tokenService)
        {
            _authConfig = authConfig;
            _httpClient = new HttpServiceGate(httpConfig);
            _tokenService = tokenService;
            _apiEndpoints = new ApiEndpoints(authConfig.BaseUrl);
        }

        public async Task<ExecutionResult<LoginStatus>> LoginAsync(string login, string password)
        {
            var result = new ExecutionResult<LoginStatus>();

            _tokenService.ResetTokens();

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.Login())
                    .WithData(await GetLogInContent(login, password).ConfigureAwait(false));

                request.ContentType = ApplicationFormContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request,
                        ignoreErrorCodes: new[] {HttpStatusCode.BadRequest})
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    var tokens = JsonConverter.Deserialize<LoginData>(response.Content);

                    await _tokenService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken)
                        .ConfigureAwait(false);

                    result.Report(LoginStatus.Successful, ExecutionStatus.Completed);
                }
                else
                {
                    result.Report(HandleLoginError(response.Content), ExecutionStatus.Failed);
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

                request.ContentType = ApplicationFormContentType;

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

        public async Task<ExecutionResult<ResendEmailStatus>> ResendEmail(string code)
        {
            var result = new ExecutionResult<ResendEmailStatus>();

            _tokenService.ResetTokens();

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.ResendEmail())
                    .WithData(JsonConverter.Serialize(new
                    {
                        code = "1212"
                    }));

                request.ContentType = ApplicationFormContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request)
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result.Report(ResendEmailStatus.Successful, ExecutionStatus.Completed);
                }
                else
                {
                    result.Report(HandleResendEmailError(response.Content), ExecutionStatus.Failed);
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
                    .DisableCaching()
                    .WithData(JsonConverter.Serialize(new
                    {
                        email = login,
                        password = password,
                        isAcceptedTermsOfService = true
                    }));

                request.ContentType = ApplicationJsonContentType;

                var response = await _httpClient
                    .ExecuteApiCallAsync(HttpRequestPriority.High, request,
                        ignoreErrorCodes: new[] {HttpStatusCode.Conflict})
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result = RegistrationStatus.Successful;
                }
                else
                {
                    result = HandleRegistrationError(response.Content);
                }
            }, RetryNumber);

            return result;
        }

        public async Task<ExecutionResult<ForgotPasswordStatus>> ForgotPassword(string login)
        {
            var result = new ExecutionResult<ForgotPasswordStatus>();

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.ForgotPassword())
                    .WithData(JsonConverter.Serialize(new {email = login}));

                request.ContentType = ApplicationJsonContentType;

                var response = await _httpClient.ExecuteApiCallAsync(HttpRequestPriority.High, request,
                        ignoreErrorCodes: new[] {HttpStatusCode.Conflict, HttpStatusCode.NotFound})
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result.Report(ForgotPasswordStatus.Successful, ExecutionStatus.Completed);
                }
                else
                {
                    result.Report(HandleForgotPasswordError(response.Content), ExecutionStatus.Failed);
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

        private ResendEmailStatus HandleResendEmailError(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return ResendEmailStatus.Undefined;
            }

            var errorData = JsonConverter.Deserialize<ErrorData>(content);

            if (errorData.ErrorCode == ErrorCodes.UserNotFound)
            {
                return ResendEmailStatus.Failed;
            }

            return ResendEmailStatus.Failed;
        }

        private LoginStatus HandleLoginError(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return LoginStatus.Undefined;
            }

            var errorData = JsonConverter.Deserialize<ErrorData>(content);

            if (errorData.ErrorCode == ErrorCodes.UserNotFound)
            {
                return LoginStatus.EmailOrPasswordIncorrect;
            }

            if (errorData.ErrorCode == ErrorCodes.EmailIsNotConfirmed)
            {
                return LoginStatus.EmailNotConfirmed;
            }

            return LoginStatus.Failed;
        }

        private ForgotPasswordStatus HandleForgotPasswordError(string responseContent)
        {
            if (string.IsNullOrEmpty(responseContent))
            {
                return ForgotPasswordStatus.Undefined;
            }

            var errorData = JsonConverter.Deserialize<ErrorData>(responseContent);

            if (errorData.ErrorCode == ErrorCodes.EmailIsNotConfirmed)
            {
                return ForgotPasswordStatus.EmailNotConfirmed;
            }

            if (errorData.ErrorCode == ErrorCodes.UserNotFound)
            {
                return ForgotPasswordStatus.UserNotFound;
            }

            return ForgotPasswordStatus.Failed;
        }

        private RegistrationStatus HandleRegistrationError(string responseContent)
        {
            if (string.IsNullOrEmpty(responseContent))
            {
                return RegistrationStatus.Undefined;
            }

            var data = JsonConverter.Deserialize<ErrorData>(responseContent);

            if (data == null)
            {
                return RegistrationStatus.Failed;
            }

            if (data.ErrorCode == ErrorCodes.UserWithDefinedEmailAlreadyExist)
            {
                return RegistrationStatus.EmailAlreadyTaken;
            }

            if (data.ErrorCode == ErrorCodes.RequestModelValidationFailed)
            {
                return RegistrationStatus.Failed;
            }

            return RegistrationStatus.Failed;
        }
    }
}