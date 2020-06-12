using System.Net.Http.Headers;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public class DefaultHttpClientProvider : IHttpClientProvider
    {
        public System.Net.Http.HttpClient CreateHttpClient()
        {
            var httpClient = DoCreateHttpClientInstance();
            DoConfigureHttpClientInstance(httpClient);
            return httpClient;
        }

        protected virtual System.Net.Http.HttpClient DoCreateHttpClientInstance()
        {
            return new System.Net.Http.HttpClient();
        }

        protected virtual void DoConfigureHttpClientInstance(System.Net.Http.HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }
    }
}