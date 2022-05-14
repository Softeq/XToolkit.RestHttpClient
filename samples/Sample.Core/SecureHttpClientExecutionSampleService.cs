using System;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.DefaultAuthorization;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.HttpClient;
using Softeq.XToolkit.HttpClient.Infrastructure;

namespace Sample.Core
{
    public class SecureHttpClientExecutionSampleService
    {
        private readonly SessionApiService _sessionApiService;
        private readonly ISecuredHttpServiceGate _http;

        public SecureHttpClientExecutionSampleService(ISecuredTokenManager manager)
        {
            var testAuthConfig = new AuthConfig("registration uri", "client", "secret");
            var httpConfig = new HttpServiceGateConfig
            {
                DeadRequestTimeoutInMilliseconds = 1000000000
            };
            var httpClientProvider = new DefaultHttpClientProvider();
            var httpClientErrorHandler = new HttpClientNoInternetHandler();
            var httpClient = new HttpServiceGate(httpClientProvider, httpClientErrorHandler, httpConfig);
            
            _sessionApiService = new SessionApiService(testAuthConfig, httpClient, manager);
            _http = new SecuredHttpServiceGate(manager, _sessionApiService, httpClientProvider, httpClientErrorHandler, httpConfig);
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