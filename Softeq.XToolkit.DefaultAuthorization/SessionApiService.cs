using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.DefaultAuthorization.Infrastructure;
using Softeq.XToolkit.HttpClient.Abstract;

namespace Softeq.XToolkit.DefaultAuthorization
{
    [SuppressMessage("ReSharper", "RedundantAnonymousTypePropertyName")]
    public class SessionApiService : ISessionApiService
    {
        private const string AccountNotConfirmedCode = "account_not_confirmed";
        private const int RetryNumber = 3;

        private readonly ISecuredTokenManager _tokenService;
        private readonly IHttpServiceGate _httpClient;

        private AuthConfig _authConfig;
        private ApiEndpoints _apiEndpoints;

        public SessionApiService(AuthConfig authConfig, IHttpServiceGate httpClient,
            ISecuredTokenManager tokenService)
        {
            _authConfig = authConfig;
            _httpClient = httpClient;
            _tokenService = tokenService;

            _apiEndpoints = new ApiEndpoints(_authConfig.BaseUrl);
        }

        public void SetBaseUrl(string baseUrl)
        {
            _authConfig.SetBaseUrl(baseUrl);
            _apiEndpoints = new ApiEndpoints(_authConfig.BaseUrl);
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
                    .WithFormUrlEncodedContent(new LoginDto
                    {
                        ClientId = _authConfig.ClientId,
                        ClientSecret = _authConfig.ClientSecret,
                        Username = login,
                        Password = password
                    });

                var response = await _httpClient.ExecuteApiCallAsync(request,
                        priority: HttpRequestPriority.High,
                        ignoreErrorCodes: new[] { HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound })
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    var tokens = JsonConverter.Deserialize<LoginData>(response.Content);

                    _tokenService.SaveTokens(tokens.AccessToken,
                        tokens.RefreshToken,
                        tokens.AccessTokenExpirationTimespanInSeconds);

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

            if (string.IsNullOrEmpty(_tokenService.RefreshToken))
            {
                return result;
            }

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.RefreshToken())
                    .WithFormUrlEncodedContent(new RefreshTokenDto
                    {
                        ClientId = _authConfig.ClientId,
                        ClientSecret = _authConfig.ClientSecret,
                        RefreshToken = _tokenService.RefreshToken
                    });

                var noInternetCodes = new[] { HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout, HttpStatusCode.RequestTimeout, HttpStatusCode.BadGateway };
                var response = await _httpClient.ExecuteApiCallAsync(request, 0, HttpRequestPriority.High, ignoreErrorCodes: noInternetCodes)
                    .ConfigureAwait(false);

                if (response.IsSuccessful && response.Content != null)
                {
                    var tokens = JsonConverter.Deserialize<LoginData>(response.Content);

                    _tokenService.SaveTokens(tokens.AccessToken,
                        tokens.RefreshToken,
                        tokens.AccessTokenExpirationTimespanInSeconds);
                    result = ExecutionStatus.Completed;
                }
                else if (response.IsNoInternet || noInternetCodes.Contains(response.StatusCode))
                {
                    result = ExecutionStatus.NotCompleted;
                }
            }, RetryNumber);

            return result;
        }

        public async Task<ExecutionResult<ResendEmailStatus>> ResendConfirmationAsync(string email)
        {
            var result = new ExecutionResult<ResendEmailStatus>();

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.ResendConfirmationEmail())
                    .WithJsonData(new { email });

                var response = await _httpClient
                    .ExecuteApiCallAsync(request,
                        priority: HttpRequestPriority.High,
                        ignoreErrorCodes: new[]
                            {HttpStatusCode.NotFound, HttpStatusCode.Conflict})
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result.Report(ResendEmailStatus.Successful, ExecutionStatus.Completed);
                }
                else
                {
                    result.Report(HandleResendConfirmationError(response.Content), ExecutionStatus.Failed);
                }
            }, RetryNumber);

            return result;
        }

        public async Task<CheckRegistrationStatus> IsAccountAlreadyRegistered(string email)
        {
            var result = CheckRegistrationStatus.Undefined;
            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetUri(_apiEndpoints.IsAccountFreeToUse(new { email }))
                    .SetMethod(HttpMethods.Get);

                var response = await _httpClient
                    .ExecuteApiCallAsync(request,
                        priority: HttpRequestPriority.High,
                        ignoreErrorCodes: HttpStatusCode.Conflict)
                    .ConfigureAwait(false);

                if (response.IsNoInternet)
                {
                    result = CheckRegistrationStatus.Undefined;
                }
                else
                {
                    result = response.StatusCode == HttpStatusCode.OK
                        ? CheckRegistrationStatus.Free
                        : CheckRegistrationStatus.EmailAlreadyTaken;
                }
            }, RetryNumber);

            return result;
        }

        public void Logout()
        {
            _tokenService.ResetTokens();
        }

        public async Task<ExecutionResult<RegistrationStatus>> RegisterAccountAsync(string login, string password)
        {
            var result = new ExecutionResult<RegistrationStatus>();

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.Register())
                    .DisableCaching()
                    .WithJsonData(new RegisterAccountDto
                    {
                        Email = login,
                        Password = password,
                    });

                var response = await _httpClient
                    .ExecuteApiCallAsync(request,
                        priority: HttpRequestPriority.High,
                        ignoreErrorCodes: new[] { HttpStatusCode.Conflict })
                    .ConfigureAwait(false);

                if (response.IsSuccessful)
                {
                    result.Report(RegistrationStatus.Successful, ExecutionStatus.Completed);
                }
                else
                {
                    result.Report(HandleRegistrationError(response.Content), ExecutionStatus.Failed);
                }
            }, RetryNumber);

            return result;
        }

        public async Task<ExecutionResult<ForgotPasswordStatus>> ForgotPasswordAsync(string login)
        {
            var result = new ExecutionResult<ForgotPasswordStatus>();

            await Executor.ExecuteWithRetryAsync(async executionContext =>
            {
                var request = new HttpRequest()
                    .SetMethod(HttpMethods.Post)
                    .SetUri(_apiEndpoints.ForgotPassword())
                    .WithJsonData(new { email = login });

                var response = await _httpClient
                    .ExecuteApiCallAsync(request,
                        priority: HttpRequestPriority.High,
                        ignoreErrorCodes: new[] { HttpStatusCode.Conflict, HttpStatusCode.NotFound })
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

        private ResendEmailStatus HandleResendConfirmationError(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return ResendEmailStatus.Undefined;
            }

            var errorData = JsonConverter.Deserialize<ErrorData>(content);

            switch (errorData.ErrorCode)
            {
                case ErrorCodes.UserNotFound:
                    return ResendEmailStatus.UserNotFound;
                case ErrorCodes.EmailAlreadyConfirmed:
                    return ResendEmailStatus.EmailAlreadyConfirmed;
                default:
                    return ResendEmailStatus.Failed;
            }
        }

        private LoginStatus HandleLoginError(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return LoginStatus.Undefined;
            }

            var errorData = JsonConverter.Deserialize<ErrorData>(content);

            if (errorData.ErrorCode == 0)
            {
                var loginErrorData = JsonConverter.Deserialize<CustomLoginErrorData>(content);

                if (loginErrorData == null)
                {
                    return LoginStatus.Undefined;
                }

                if (loginErrorData.Error == CustomLoginErrors.InvalidGrant)
                {
                    if (loginErrorData.ErrorCode == AccountNotConfirmedCode)
                    {
                        return LoginStatus.EmailNotConfirmed;
                    }

                    return LoginStatus.EmailOrPasswordIncorrect;
                }

                return LoginStatus.Failed;
            }

            if (errorData.ErrorCode == ErrorCodes.EmailIsNotConfirmed)
            {
                return LoginStatus.EmailNotConfirmed;
            }

            if (errorData.ErrorCode == ErrorCodes.UserNotFound)
            {
                return LoginStatus.UserNotFound;
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

            switch (errorData.ErrorCode)
            {
                case ErrorCodes.UserNotFound:
                    return ForgotPasswordStatus.UserNotFound;
                case ErrorCodes.UserIsInPendingState:
                    return ForgotPasswordStatus.EmailNotConfirmed;
                default:
                    return ForgotPasswordStatus.Failed;
            }
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

            switch (data.ErrorCode)
            {
                case ErrorCodes.UserWithDefinedEmailAlreadyExist:
                    return RegistrationStatus.EmailAlreadyTaken;
                case ErrorCodes.RequestModelValidationFailed:
                    return RegistrationStatus.Failed;
                default:
                    return RegistrationStatus.Failed;
            }
        }
    }
}