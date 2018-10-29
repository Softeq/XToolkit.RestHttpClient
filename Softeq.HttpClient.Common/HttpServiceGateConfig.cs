using System.Net;

namespace Softeq.HttpClient.Common
{
    public class HttpServiceGateConfig
    {
        public int MaxHttpLoadThreads { get; set; } = 5;
        public int HighPriorityHttpLoadThreads { get; set; } = 2;
        public int HttpRequestPerPriorityLimit { get; set; } = 350;
        public int DeadRequestTimeoutInMilliseconds { get; set; } = 10000;
        public WebProxy Proxy { get; set; } = null;
    }
}
