using System.Net.Http.Headers;
using SystemHttpClient = System.Net.Http.HttpClient;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public class DefaultHttpClientProvider : IHttpClientProvider
    {
        public SystemHttpClient CreateHttpClient()
        {
            var httpClient = DoCreateHttpClientInstance();
            DoConfigureHttpClientInstance(httpClient);
            return httpClient;
        }

        protected virtual SystemHttpClient DoCreateHttpClientInstance()
        {
            return new SystemHttpClient();
        }

        protected virtual void DoConfigureHttpClientInstance(SystemHttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }
    }
}