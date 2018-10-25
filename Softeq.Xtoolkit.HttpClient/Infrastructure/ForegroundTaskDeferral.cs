// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public class ForegroundTaskDeferral
    {
        private readonly TaskCompletionSource<bool> _taskCompletition;

        private readonly ThreadSafe<bool> _isCancelRequested;
        private readonly ThreadSafe<bool> _isInProgress;
        private readonly ThreadSafe<bool> _isStarted;
        private readonly CancellationTokenSource _cancellationToken;

        public bool IsCancelRequested => _isCancelRequested.Get();

        public bool IsInProgress => _isInProgress.Get();

        public EventHandler TaskCancelationRequested;
        public CancellationToken Token => _cancellationToken.Token;

        public ForegroundTaskDeferral()
        {
            _isStarted = new ThreadSafe<bool>(false);
            _isCancelRequested = new ThreadSafe<bool>(false);
            _isInProgress = new ThreadSafe<bool>(false);

            _taskCompletition = new TaskCompletionSource<bool>();

            _cancellationToken = new CancellationTokenSource();
        }

        public void RequestTaskCancel()
        {
            _isCancelRequested.Set(true);
            _cancellationToken.Cancel();

            TaskCancelationRequested?.Invoke(this, System.EventArgs.Empty);
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

        public void Complete()
        {
            if (!_isStarted.Get() || !_isInProgress.Get())
            {
                throw new InvalidOperationException("Deferral cannot be completed. It is not started or not in progress.");
            }

            _isInProgress.Set(false);

            _taskCompletition.SetResult(true);
        }

        public void CompleteIfInProgress()
        {
            if (_isInProgress.Get())
            {
                Complete();
            }
        }

        public async Task WaitForCompletionAsync()
        {
            await _taskCompletition.Task.ConfigureAwait(false);
        }
    }
}
