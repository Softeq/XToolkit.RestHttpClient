// Developed for LilBytes by Softeq Development Corporation
//

namespace Softeq.HttpClient.Common.Executor
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
