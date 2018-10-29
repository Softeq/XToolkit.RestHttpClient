﻿using System;

namespace Softeq.HttpClient.Common.Executor
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
