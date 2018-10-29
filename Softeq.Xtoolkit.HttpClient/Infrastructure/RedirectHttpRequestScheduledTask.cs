using Softeq.XToolkit.HttpClient.Network;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    internal class RedirectHttpRequestScheduledTask : HttpRequestScheduledTaskBase
    {
        public string RequestUrl { get; set; }
        public string ResponseRedirectUrl { get; set; }

        public RedirectHttpRequestScheduledTask(int taskTimeoutInMilliseconds) : base(taskTimeoutInMilliseconds)
        {
        }
    }
}
