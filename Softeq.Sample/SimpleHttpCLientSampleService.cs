// Developed for LilBytes by Softeq Development Corporation
//
using System;
using System.Threading.Tasks;
using Softeq.XToolkit.HttpClient;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Enums;
using Softeq.XToolkit.HttpClient.Infrastructure;

namespace Softeq.Sample
{
    public class SimpleHttpClientSampleService
    {
        readonly IExecutor _executor;
        private readonly HttpServiceGate _http;

        public SimpleHttpClientSampleService()
        {
            _executor = new Executor();
            _http = new HttpServiceGate(new HttpServiceGateConfig());
        }

        public async Task<ExecutionResult<string>> MakeRequest()
        {
            var executionResult = new ExecutionResult<string>();

            await _executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    var request = new HttpRequest()
                    .SetUri(new Uri("https://www.google.com/"))
                    .SetMethod(HttpMethods.Get);

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
