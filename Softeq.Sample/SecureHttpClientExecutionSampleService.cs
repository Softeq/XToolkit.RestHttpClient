// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Threading.Tasks;
using Softeq.XToolkit.DefaultAuthorization;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.DefaultAuthorization.Extensions;
using Softeq.XToolkit.HttpClient;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Enums;
using Softeq.XToolkit.HttpClient.Infrastructure;

namespace Softeq.Sample
{
    public class SecureHttpClientExecutionSampleService
    {
        readonly IExecutor _executor;
        private readonly IMembershipService _membershipService;
        private readonly SessionApiService _sessionApiService;
        private readonly SecuredHttpServiceGate _http;

        public SecureHttpClientExecutionSampleService()
        {
            var authConfig = new AuthConfig("yourBaseUrl", "yourclient", "yoursecret");
            var httpConfig = new HttpServiceGateConfig {};
            var httpServiceGate = new HttpServiceGate(httpConfig);

            _executor = new Executor();
            _membershipService = new MembershipService(new SecureStorage());
            _sessionApiService = new SessionApiService(authConfig, httpServiceGate, _membershipService, _executor);
            _http = new SecuredHttpServiceGate(_sessionApiService, httpConfig, _membershipService);
        }

        public async Task LoginAsync()
        {
            await _sessionApiService.LoginAsync("username", "password");
        }

        public async Task<ExecutionResult<string>> MakeRequestWithCredentials()
        {
            var executionResult = new ExecutionResult<string>();

            await _executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    var request = new HttpRequest()
                    .SetUri(new Uri("yourApiUrl"))
                    .SetMethod(HttpMethods.Get)
                    .WithCredentials(_membershipService);

                    var response = await _http.ExecuteApiCallAsync(HttpRequestPriority.High, request);

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