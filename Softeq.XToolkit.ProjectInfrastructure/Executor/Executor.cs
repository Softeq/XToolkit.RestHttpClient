﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Softeq.XToolkit.CrossCutting.Executor
{
    public static class Executor
    {
        private static readonly ConcurrentDictionary<string, ExecutionGroup<WorkflowWrappedExecutionContext>> _groupedAsyncExecutions;

        static Executor()
        {
            _groupedAsyncExecutions = new ConcurrentDictionary<string, ExecutionGroup<WorkflowWrappedExecutionContext>>();
        }

        /// <summary>
        /// Executes action multiple times and reports error.
        /// </summary>
        /// <param name="asyncAction">Action to execute</param>
        /// <param name="exceptionHandleStrategy">Handling strategy if exception has been thriown</param>
        /// <param name="allowedAttempts">Show how many times action will be executed again in case of exception</param>
        /// <param name="executionGroup">Indicates the group of actions to which current execution belongs. If something fails in group then notification will appear only once for all actions in group. Use null if action is group independent</param>
        /// <returns></returns>
        public static async Task ExecuteWithRetryAsync(Func<IAsyncExecutionContext, Task> asyncAction,
            int allowedAttempts = 1,
            string executionGroup = null,
            Action<Exception> exceptionHandler = null)
        {
            if (allowedAttempts < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(allowedAttempts));
            }

            var context = new AsyncExecutionContext();

            context.WrapAction(asyncAction);

            var workflow = new WorkflowWrappedExecutionContext
            {
                AllowedAttempts = allowedAttempts,
                Context = context,
                ExecutionGroup = executionGroup
            };

            if (executionGroup != null)
            {
                _groupedAsyncExecutions.AddOrUpdate(
                    executionGroup,
                    new ExecutionGroup<WorkflowWrappedExecutionContext> { Name = executionGroup },
                    (key, existingGroup) => existingGroup);

                _groupedAsyncExecutions[executionGroup].Workflows.AddOrUpdate(workflow.Id, workflow, (key, value) => value);
            }

            await PerformExecutionWorkflowAsync(workflow, exceptionHandler).ConfigureAwait(false);
        }

        public static async Task ExecuteSilentlyAsync(Func<Task> asyncAction, Action<Exception> exceptionCallback = null)
        {
            try
            {
                await asyncAction.Invoke().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                LogError(exception);
                exceptionCallback?.Invoke(exception);
            }
        }

        public static void InBackgroundThread(Func<Task> asyncAction)
        {
            Task.Run(() =>
                ExecuteWithRetryAsync(
                    async executionContext =>
                    {
                        await asyncAction.Invoke().ConfigureAwait(false);
                    }));
        }

        public static void InBackgroundThread(Action action)
        {
            Task.Run(() =>
                ExecuteWithRetryAsync(
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                    async executionContext =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                    {
                        action.Invoke();
                    }));
        }

        public static async Task ExecuteActionAsync(OncePerIntervalAction oncePerIntervalAction)
        {
            if (oncePerIntervalAction == null)
            {
                return;
            }

            oncePerIntervalAction.LastChangeOnTime = DateTime.Now;
            await Task.Delay(oncePerIntervalAction.Interval).ConfigureAwait(false);

            if (DateTime.Now - oncePerIntervalAction.LastChangeOnTime >= oncePerIntervalAction.Interval)
            {
                oncePerIntervalAction.Operation?.Invoke();
            }
        }

        private static async Task PerformExecutionWorkflowAsync(WorkflowWrappedExecutionContext workflow, Action<Exception> exceptionHandler = null)
        {
            workflow.Context.ExecutionsCount = 0;

            var exception = await ExecuteMultipleTimesAsync(workflow).ConfigureAwait(false);
            if (exception != null)
            {
                exceptionHandler?.Invoke(exception);
            }

            var group = GetGroupForContext(workflow);

            if (group == null)
            {
                return;
            }

            if (group.Workflows.ContainsKey(workflow.Id))
            {
                group.Workflows.TryRemove(workflow.Id, out _);
            }

            if (group.Workflows.Any())
            {
                return;
            }

            _groupedAsyncExecutions.TryRemove(group.Name, out _);
        }

        private static ExecutionGroup<WorkflowWrappedExecutionContext> GetGroupForContext(WorkflowWrappedExecutionContext workflow)
        {
            return !string.IsNullOrEmpty(workflow.ExecutionGroup) && _groupedAsyncExecutions.ContainsKey(workflow.ExecutionGroup)
                ? _groupedAsyncExecutions[workflow.ExecutionGroup]
                : null;
        }

        private static async Task<Exception> ExecuteMultipleTimesAsync(WorkflowWrappedExecutionContext workflow)
        {
            try
            {
                if (workflow.Context.TotalExecutionsCount == 0)
                {
                    await workflow.Context.ExecuteFirstAsync().ConfigureAwait(false);
                }
                else
                {
                    await workflow.Context.ExecuteAgainAsync().ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                LogError(exception);

                workflow.Context.Status = ExecutionContextStatus.Failed;

                if (workflow.Context.ExecutionsCount >= workflow.AllowedAttempts)
                {
                    return exception;
                }
                else
                {
                    return await ExecuteMultipleTimesAsync(workflow).ConfigureAwait(false);
                }
            }

            workflow.Context.Status = ExecutionContextStatus.Completed;

            return null;
        }

        private static void LogError(Exception ex)
        {
            //TODO use logger instead of this, write in console if DEBUG
            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }
}
