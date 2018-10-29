// Developed for LilBytes by Softeq Development Corporation
//

using System.Net.Http;
using Softeq.HttpClient.Common;
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
