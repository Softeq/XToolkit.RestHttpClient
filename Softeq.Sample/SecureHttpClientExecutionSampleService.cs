// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Threading.Tasks;
using Softeq.HttpClient.Common;
using Softeq.HttpClient.Common.Executor;
using Softeq.XToolkit.DefaultAuthorization;
using Softeq.XToolkit.DefaultAuthorization.Abstract;
using Softeq.XToolkit.DefaultAuthorization.Extensions;

namespace Softeq.Sample
{
    public class SecureHttpClientExecutionSampleService
    {
        private readonly IMembershipService _membershipService;
        private readonly SessionApiService _sessionApiService;
        private readonly SecuredHttpServiceGate _http;

        public SecureHttpClientExecutionSampleService()
        {
            var authConfig = new AuthConfig("base url", "clientid", "your secret");
            var httpConfig = new HttpServiceGateConfig();

            _membershipService = new MembershipService(new SecureStorage());

            _sessionApiService = new SessionApiService(authConfig, httpConfig, _membershipService);
            _http = new SecuredHttpServiceGate(_sessionApiService, httpConfig, _membershipService);
        }

        public async Task LoginAsync()
        {
            await _sessionApiService.LoginAsync("login", "password");
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