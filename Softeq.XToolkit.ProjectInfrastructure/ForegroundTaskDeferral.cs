using System;
using System.Threading;
using System.Threading.Tasks;

namespace Softeq.XToolkit.CrossCutting
{
    public abstract class ForegroundTaskDeferralBase<T>
    {
        private readonly TaskCompletionSource<T> _taskCompletition;

        private readonly ThreadSafe<bool> _isCancelRequested;
        private readonly ThreadSafe<bool> _isInProgress;
        private readonly ThreadSafe<bool> _isStarted;
        private readonly CancellationTokenSource _cancellationToken;

        public bool IsCancelRequested => _isCancelRequested.Get();

        public bool IsInProgress => _isInProgress.Get();

        public EventHandler TaskCancelationRequested;
        public CancellationToken Token => _cancellationToken.Token;

        protected ForegroundTaskDeferralBase()
        {
            _isStarted = new ThreadSafe<bool>(false);
            _isCancelRequested = new ThreadSafe<bool>(false);
            _isInProgress = new ThreadSafe<bool>(false);

            _taskCompletition = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

            _cancellationToken = new CancellationTokenSource();
        }

        public void RequestTaskCancel()
        {
            _isCancelRequested.Set(true);
            _cancellationToken.Cancel();

            TaskCancelationRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Begin()
        {
            if (_isStarted.Get())
            {
                throw new InvalidOperationException("Deferral cannot be started. It is already in progress.");
            }

            _isStarted.Set(true);
            _isInProgress.Set(true);
        }

        protected void DoComplete(T result)
        {
            if (!_isStarted.Get() || !_isInProgress.Get())
            {
                throw new InvalidOperationException("Deferral cannot be completed. It is not started or not in progress.");
            }

            _isInProgress.Set(false);

            _taskCompletition.SetResult(result);
        }

        protected Task<T> DoWaitForCompletionAsync()
        {
            return _taskCompletition.Task;
        }
    }

    public class ForegroundTaskDeferral<T> : ForegroundTaskDeferralBase<T>
    {
        public Task<T> WaitForCompletionAsync()
        {
            return DoWaitForCompletionAsync();
        }

        public void Complete(T result)
        {
            DoComplete(result);
        }
    }

    public class ForegroundTaskDeferral : ForegroundTaskDeferralBase<bool>
    {
        public void CompleteIfInProgress()
        {
            if (IsInProgress)
            {
                DoComplete(true);
            }
        }

        public void Complete()
        {
            DoComplete(true);
        }

        public Task WaitForCompletionAsync()
        {
            return DoWaitForCompletionAsync();
        }
    }
}
