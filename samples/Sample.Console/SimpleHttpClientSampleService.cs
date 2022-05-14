using System;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.HttpClient;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Infrastructure;

namespace Sample.Console
{
    public class SimpleHttpClientSampleService
    {
        private readonly IHttpServiceGate _http;

        public SimpleHttpClientSampleService()
        {
            var httpConfig = new HttpServiceGateConfig();
            var httpClientProvider = new DefaultHttpClientProvider();
            var httpClientErrorHandler = new HttpClientNoInternetHandler();
            _http = new HttpServiceGate(httpClientProvider, httpClientErrorHandler, httpConfig);
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

                    var response = await _http.ExecuteApiCallAsync(request, priority:HttpRequestPriority.High).ConfigureAwait(false);

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
