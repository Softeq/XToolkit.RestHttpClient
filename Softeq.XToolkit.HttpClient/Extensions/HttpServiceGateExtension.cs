using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.HttpClient.Abstract;

namespace Softeq.XToolkit.HttpClient.Extensions
{
    public static class HttpServiceGateExtension
    {
        public static async Task<T> ExecuteApiCallAndParseAsync<T>(this IHttpServiceGate httpServiceGate,
            HttpRequestPriority priority, HttpRequest request)
        {
            var response = await httpServiceGate
                .ExecuteApiCallAsync(request, priority: priority)
                .ConfigureAwait(false);

            return response.ParseContentAsJson<T>();
        }
    }
}