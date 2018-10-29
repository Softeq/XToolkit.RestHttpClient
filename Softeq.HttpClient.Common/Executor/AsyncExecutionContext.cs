// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Softeq.HttpClient.Common.Executor
{
    internal class AsyncExecutionContext : IAsyncExecutionContext
    {
        private readonly ReaderWriterLockSlim _lock;
        private ExecutionContextStatus _status;
        private Func<IAsyncExecutionContext, Task> _targetAction;

        public AsyncExecutionContext()
        {
            _lock = new ReaderWriterLockSlim();
        }

        public ExecutionContextStatus Status
        {
            get
            {
                _lock.EnterReadLock();

                var result = _status;

                _lock.ExitReadLock();

                return result;
            }

            set
            {
                _lock.EnterWriteLock();

                _status = value;

                _lock.ExitWriteLock();
            }
        }

        public int TotalExecutionsCount { get; private set; }
        public int ExecutionsCount { get; set; }

        public async Task ExecuteAgainAsync()
        {
            if (TotalExecutionsCount == 0)
            {
                throw new InvalidOperationException(
                    "Cannot perform new action execution. Call ExecuteFirstAsync() before this method.");
            }

            TotalExecutionsCount++;
            ExecutionsCount++;

            Status = ExecutionContextStatus.Running;

            await _targetAction(this).ConfigureAwait(false);
        }

        public void WrapAction(Func<IAsyncExecutionContext, Task> action)
        {
            _targetAction = action;
        }

        public async Task ExecuteFirstAsync()
        {
            if (TotalExecutionsCount != 0)
            {
                throw new InvalidOperationException("Cannot perform first action execution. It has been already executed.");
            }

            TotalExecutionsCount++;
            ExecutionsCount++;

            Status = ExecutionContextStatus.Running;

            await _targetAction(this).ConfigureAwait(false);
        }
    }
}