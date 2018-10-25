// Developed for LilBytes by Softeq Development Corporation
//

using Softeq.XToolkit.HttpClient.Enums;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public class ExecutionResult<T>
    {
        public T Result { get; set; }
        public ExecutionStatus Status { get; set; }

        public ExecutionResult<T> Report(T result, ExecutionStatus status)
        {
            Result = result;
            Status = status;

            return this;
        }
    }
}
