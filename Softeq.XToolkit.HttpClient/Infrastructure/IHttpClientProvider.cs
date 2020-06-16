namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public interface IHttpClientProvider
    {
        System.Net.Http.HttpClient CreateHttpClient();
    }
}