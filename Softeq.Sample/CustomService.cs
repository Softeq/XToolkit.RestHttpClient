﻿// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Net;
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
    public class CustomService
    {
        readonly IExecutor _executor;
        private readonly IMembershipService _membershipService;
        private readonly SessionApiService _sessionApiService;
        private readonly SecuredHttpServiceGate _http;

        public CustomService()
        {
            _executor = new Executor();

            var authConfig = new AuthConfig("http://lilbytes-softeq-test.azurewebsites.net", "ro.client", "secret");
            var httpConfig = new HttpServiceGateConfig { Proxy = new WebProxy("10.55.1.191", 8888), DeadRequestTimeoutInMilliseconds = 10000000 };

            _membershipService = new MembershipService(new SecureStorage());

            var httpServiceGate = new HttpServiceGate(httpConfig);
            _sessionApiService = new SessionApiService(authConfig, httpServiceGate, _membershipService, _executor);
            _http = new SecuredHttpServiceGate(_sessionApiService, httpConfig, _membershipService);
        }


        public async Task LoginAsync()
        {
            await _sessionApiService.LoginAsync("user@test.com", "123QWqw1");
        }

        public async Task RestoreTokenAsync()
        {
            await _sessionApiService.RefreshTokenAsync();
        }

        public async Task<ExecutionResult<string>> TestMethodHighPriority()
        {
            var executionResult = new ExecutionResult<string>();

            await _executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    var request = new HttpRequest()
                    .SetUri(new Uri("http://lilbytes-softeq-test.azurewebsites.net/api/user"))
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

            Console.WriteLine(executionResult.Result != null);

            return executionResult;
        }
    }
}