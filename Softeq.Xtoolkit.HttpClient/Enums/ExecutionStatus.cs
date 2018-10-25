// Developed for LilBytes by Softeq Development Corporation
//
namespace Softeq.XToolkit.HttpClient.Enums
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
