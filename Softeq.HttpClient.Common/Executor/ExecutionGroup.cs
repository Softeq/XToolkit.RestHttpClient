using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Softeq.HttpClient.Common.Executor
{
    internal class ExecutionGroup<TContext>
    {
        private readonly ReaderWriterLockSlim _lock;

        public ExecutionGroup()
        {
            _lock = new ReaderWriterLockSlim();

            Workflows = new ConcurrentDictionary<Guid, TContext>();
        }

        public string Name { get; set; }

        public ConcurrentDictionary<Guid, TContext> Workflows { get; set; }
    }
}
