using System;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.DefaultAuthorization;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.DefaultAuthorization.Extensions;
using Softeq.XToolkit.HttpClient;
using Softeq.XToolkit.HttpClient.Infrastructure;

namespace Sample.Console
{
    public class SecureHttpClientExecutionSampleService
    {
        private readonly ISecuredTokenManager _tokenManager;
        private readonly SessionApiService _sessionApiService;
        private readonly ISecuredHttpServiceGate _http;

        public SecureHttpClientExecutionSampleService()
        {
            var testAuthConfig = new AuthConfig("http://qwerty.azurewebsites.net", "ro.client", "secret");
            var httpConfig = new HttpServiceGateConfig();
            var httpClientProvider = new DefaultHttpClientProvider();
            var httpClientErrorHandler = new HttpClientNoInternetHandler();
            var httpClient = new HttpServiceGate(httpClientProvider, httpClientErrorHandler, httpConfig);
            _tokenManager = new ConsoleSecuredTokenManager();
            _sessionApiService = new SessionApiService(testAuthConfig, httpClient, _tokenManager);
            _http = new SecuredHttpServiceGate(_tokenManager, _sessionApiService, httpClientProvider, httpClientErrorHandler, httpConfig);
        }

        public async Task LoginAsync()
        {
            await _sessionApiService.LoginAsync("login", "pass");
        }

        public async Task<ExecutionResult<string>> MakeRequestWithCredentials()
        {
            var executionResult = new ExecutionResult<string>();

            await Executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    var request = new HttpRequest()
                    .SetUri(new Uri("provide your url"))
                    .SetMethod(HttpMethods.Get)
                    .WithCredentials(_tokenManager);

                    var response = await _http.ExecuteApiCallAsync(request, priority: HttpRequestPriority.High);

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