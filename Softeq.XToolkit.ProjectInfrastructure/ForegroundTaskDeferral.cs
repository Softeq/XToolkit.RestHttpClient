﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Softeq.XToolkit.CrossCutting
{
    public class ForegroundTaskDeferral<T>
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

        public ForegroundTaskDeferral()
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

        public void Complete(T result)
        {
            if (!_isStarted.Get() || !_isInProgress.Get())
            {
                throw new InvalidOperationException("Deferral cannot be completed. It is not started or not in progress.");
            }

            _isInProgress.Set(false);

            _taskCompletition.SetResult(result);
        }

        public Task<T> WaitForCompletionAsync()
        {
            return _taskCompletition.Task;
        }
    }

    public class ForegroundTaskDeferral : ForegroundTaskDeferral<bool>
    {
        public void CompleteIfInProgress()
        {
            if (IsInProgress)
            {
                Complete(true);
            }
        }
    }
}
