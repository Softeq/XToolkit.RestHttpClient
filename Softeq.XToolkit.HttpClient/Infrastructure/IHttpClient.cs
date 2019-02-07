// Developed for PAWS-HALO by Softeq Development
// Corporation http://www.softeq.com

using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public interface IHttpClient
    {
        Task<HttpResponse> ExecuteAsStringResponseAsync(HttpRequest request,
            HttpRequestPriority priority = HttpRequestPriority.Normal,
            int timeout = 0);

        Task<HttpResponse> ExecuteAsBinaryResponseAsync(HttpRequest request,
            HttpRequestPriority priority = HttpRequestPriority.Normal,
            int timeout = 0);

        Task<string> GetRedirectedUrlAsync(string urlWithRedirect,
            HttpRequestPriority priority = HttpRequestPriority.Normal);
    }
}