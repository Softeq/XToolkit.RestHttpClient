using System.Net.Http;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.HttpClient.Network;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    internal class CompleteHttpRequestScheduledTask : HttpRequestScheduledTaskBase
    {
        public HttpRequestMessage Request { get; set; }
        public HttpResponse Response { get; set; }
        public bool IsBinaryContent { get; set; }

        public CompleteHttpRequestScheduledTask(int taskTimeoutInMilliseconds) : base(taskTimeoutInMilliseconds)
        {
        }
    }
}
