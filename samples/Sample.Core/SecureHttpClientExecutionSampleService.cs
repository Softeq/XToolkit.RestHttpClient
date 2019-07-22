using System;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.DefaultAuthorization;
using Softeq.XToolkit.DefaultAuthorization.Abstract;

namespace Sample.Core
{
    public class SecureHttpClientExecutionSampleService
    {
        private readonly ISecuredTokenManager _tokenManager;
        private readonly SessionApiService _sessionApiService;
        private readonly ISecuredHttpServiceGate _http;

        public SecureHttpClientExecutionSampleService(ISecuredTokenManager manager)
        {
            var testAuthConfig = new AuthConfig("registration uri", "client", "secret");
            var httpConfig = new HttpServiceGateConfig
            {
                DeadRequestTimeoutInMilliseconds = 1000000000
            };
            _tokenManager = manager;
            _sessionApiService = new SessionApiService(testAuthConfig, httpConfig, _tokenManager);
            _http = new SecuredHttpServiceGate(_sessionApiService, httpConfig, _tokenManager);
        }

        internal async Task ResendConfirmationAsync()
        {
            var result = await _sessionApiService.ResendConfirmationAsync("test@gmail.com");
        }

        public async Task LoginAsync()
        {
            var result = await _sessionApiService.LoginAsync("test@gmail.com", "Test");
        }

        public async Task RegisterAsync()
        {
            var result = await _sessionApiService.RegisterAccountAsync("test@gmail.com", "A1B2C#asd");
        }

        public async Task ForgotPasswordAsync()
        {
            var result = await _sessionApiService.ForgotPasswordAsync("test@gmail.com");
        }

        public async Task<ExecutionResult<string>> MakeRequestWithCredentials()
        {
            var executionResult = new ExecutionResult<string>();

            await Executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    var request = new HttpRequest()
                        .SetUri(new Uri("registration uri"))
                        .SetMethod(HttpMethods.Get);

                    var response = await _http.ExecuteApiCallAsync(request);

                    if (response.IsSuccessful)
                    {
                        executionResult.Report(response.Content, ExecutionStatus.Completed);
                    }
                });

            if (executionResult.Status == ExecutionStatus.NotCompleted)
            {
                executionResult.Report(null, ExecutionStatus.Failed);
            }

            return executionResult;
        }
    }
}