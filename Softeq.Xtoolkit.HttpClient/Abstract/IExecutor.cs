// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Threading.Tasks;

namespace Softeq.XToolkit.HttpClient.Abstract
{
    public interface IExecutor
    {
        Task ExecuteWithRetryAsync(
            Func<IAsyncExecutionContext, Task> asyncAction,
            int allowedAttempts = 1,
            string executionGroup = null);

        Task ExecuteSilentlyAsync(Func<Task> asyncAction, Action<Exception> exceptionCallback = null);

        void InBackgroundThread(Func<Task> asyncAction);
        void InBackgroundThread(Action action);
        Task ExecuteActionAsync(OncePerIntervalAction oncePerIntervalAction);
    }
}
