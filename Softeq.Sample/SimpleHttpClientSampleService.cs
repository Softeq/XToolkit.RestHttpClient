using System;
using System.Threading.Tasks;
using Softeq.HttpClient.Common;
using Softeq.HttpClient.Common.Executor;
using Softeq.XToolkit.HttpClient;

namespace Softeq.Sample
{
    public class SimpleHttpClientSampleService
    {
        private readonly HttpServiceGate _http;

        public SimpleHttpClientSampleService()
        {
            _http = new HttpServiceGate(new HttpServiceGateConfig());
        }

        public async Task<ExecutionResult<string>> MakeRequest()
        {
            var executionResult = new ExecutionResult<string>();

            await Executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    var request = new HttpRequest()
                    .SetUri(new Uri("https://www.google.com/"))
                    .SetMethod(HttpMethods.Get);

                    var response = await _http.ExecuteApiCallAsync(HttpRequestPriority.High, request).ConfigureAwait(false);

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
