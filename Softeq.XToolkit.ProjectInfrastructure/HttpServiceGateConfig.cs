namespace Softeq.XToolkit.CrossCutting
{
    public class HttpServiceGateConfig
    {
        public int HttpLoadThreads { get; set; } = 5;
        public int HighPriorityHttpLoadThreads { get; set; } = 2;
        public int MaxRequestsPerPriority { get; set; } = 350;
        public int DeadRequestTimeoutInMilliseconds { get; set; } = 10000;
    }
}
