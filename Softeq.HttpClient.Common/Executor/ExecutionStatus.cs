
namespace Softeq.HttpClient.Common.Executor
{
    public enum ExecutionStatus
    {
        NotCompleted,
        /// <summary>
        /// Execution completed
        /// </summary>
        Completed,
        /// <summary>
        /// Execution failed
        /// </summary>
        Failed,
        /// <summary>
        /// Requires some input to complete action
        /// </summary>
        Interrupted
    }
}
