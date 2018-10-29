using System;
using System.Net.Http;
using System.Threading.Tasks;
using Softeq.HttpClient.Common;

namespace Softeq.XToolkit.HttpClient.Abstract
{
    public interface IHttpRequestsScheduler
    {
        void ChangePriority(Guid taskId, HttpRequestPriority newPriority);

        Guid Schedule(
            HttpRequestPriority priority,
            HttpRequestMessage request,
            int timeout = 0,
            bool isBinaryContent = false);

        Task<HttpResponse> WaitForCompletionAsync(Guid taskId);

        Task<HttpResponse> ExecuteAsync(
            HttpRequestPriority priority,
            HttpRequestMessage request,
            int timeout = 0,
            bool isBinaryContent = false);

        Task<string> ExecuteRedirectOnlyAsync(HttpRequestPriority priority, string urlWithRedirect);
    }
}
