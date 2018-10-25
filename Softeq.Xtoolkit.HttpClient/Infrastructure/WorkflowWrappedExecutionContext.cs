// Developed for LilBytes by Softeq Development Corporation
//

using System;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    internal class WorkflowWrappedExecutionContext
    {
        public Guid Id { get; private set; }
        public AsyncExecutionContext Context { get; set; }
        public int AllowedAttempts { get; set; }
        public string ExecutionGroup { get; set; }

        public WorkflowWrappedExecutionContext()
        {
            Id = Guid.NewGuid();
        }
    }
}
