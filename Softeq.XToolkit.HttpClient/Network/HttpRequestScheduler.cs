﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Priority_Queue;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Executor;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Extensions;
using Softeq.XToolkit.HttpClient.Infrastructure;

namespace Softeq.XToolkit.HttpClient.Network
{
    public class HttpRequestsScheduler : IHttpRequestsScheduler
    {
        private const int PriorityRange = 100000;
        private const int WorkerTaskExecutionIterationDelayInMilliseconds = 200;
        private const int HighPriorityWorkerTaskExecutionIterationDelayInMilliseconds = 100;
        private const int ErrorStatusCodeRangeMax = 599;

        private readonly ImmutableHashSet<int> _supportedStatusCodes;
        private readonly SimplePriorityQueue<HttpRequestScheduledTaskBase> _tasksQueue;
        private readonly object _queueSyncLock;

        private readonly HttpRequestPriority _highestPriority;
        private readonly HttpServiceGateConfig _config;

        private readonly IHttpClientProvider _httpClientProvider;
        private readonly IHttpClientErrorHandler _httpClientErrorHandler;

        private System.Net.Http.HttpClient _simpleHttpClient;

        private CancellationTokenSource _tasksExecutionCancellation;

        private ConcurrentDictionary<HttpRequestPriority, int> _priorityToTasksCountMap;
        private ConcurrentDictionary<Guid, HttpRequestScheduledTaskBase> _taskIdToTaskMap;
        private Dictionary<HttpRequestPriority, SimplePriorityQueue<HttpRequestScheduledTaskBase>> _perPriorityQueue;

        public HttpRequestsScheduler(
            IHttpClientProvider httpClientProvider,
            IHttpClientErrorHandler httpClientErrorHandler,
            HttpServiceGateConfig config)
        {
            _httpClientProvider = httpClientProvider;
            _config = config;
            _httpClientErrorHandler = httpClientErrorHandler;

            _supportedStatusCodes = Enum.GetValues(typeof(HttpStatusCode)).Cast<int>().ToImmutableHashSet();
            _tasksQueue = new SimplePriorityQueue<HttpRequestScheduledTaskBase>();
            _highestPriority = EnumExtensions.GetValues<HttpRequestPriority>().First();

            _queueSyncLock = new object();

            InitializeMaps();

            StartTasksExecution();
        }

        private void InitializeMaps()
        {
            _taskIdToTaskMap = new ConcurrentDictionary<Guid, HttpRequestScheduledTaskBase>();

            _perPriorityQueue =
                new Dictionary<HttpRequestPriority, SimplePriorityQueue<HttpRequestScheduledTaskBase>>();

            _priorityToTasksCountMap = new ConcurrentDictionary<HttpRequestPriority, int>();

            foreach (var priority in EnumExtensions.GetValues<HttpRequestPriority>())
            {
                _priorityToTasksCountMap.AddOrUpdate(priority, 0, (key, value) => 0);
                _perPriorityQueue.Add(priority, new SimplePriorityQueue<HttpRequestScheduledTaskBase>());
            }
        }

        public void ChangePriority(Guid taskId, HttpRequestPriority newPriority)
        {
            _taskIdToTaskMap.TryGetValue(taskId, out var task);

            if (task == null || task.Priority == newPriority)
            {
                return;
            }

            _tasksQueue.TryUpdatePriority(task, GetPriorityForNewItem(newPriority));
        }

        public Guid Schedule(
            HttpRequestPriority priority,
            HttpRequestMessage request,
            int timeout = 0,
            bool isBinaryContent = false)
        {
            return ScheduleInternal(priority, request, timeout, isBinaryContent).Id;
        }

        public async Task<HttpResponse> WaitForCompletionAsync(Guid taskId)
        {
            _taskIdToTaskMap.TryGetValue(taskId, out var task);

            if (task == null)
            {
                return null;
            }

            await WaitForTaskCompletionAsync(task).ConfigureAwait(false);

            return (task as CompleteHttpRequestScheduledTask)?.Response;
        }

        public ForegroundTaskDeferral GetTaskDeferral(Guid taskId)
        {
            _taskIdToTaskMap.TryGetValue(taskId, out var task);

            return task?.Deferral;
        }

        public async Task<HttpResponse> ExecuteAsync(
            HttpRequestPriority priority,
            HttpRequestMessage request,
            int timeout = 0,
            bool isBinaryContent = false)
        {
            var task = ScheduleInternal(priority, request, timeout, isBinaryContent);

            await WaitForTaskCompletionAsync(task).ConfigureAwait(false);

            return task.Response;
        }

        public async Task<string> ExecuteRedirectOnlyAsync(HttpRequestPriority priority, string urlWithRedirect)
        {
            var task = new RedirectHttpRequestScheduledTask(_config.DeadRequestTimeoutInMilliseconds)
            {
                RequestUrl = urlWithRedirect,
                Priority = priority
            };

            AddNewTask(priority, task);

            await WaitForTaskCompletionAsync(task).ConfigureAwait(false);

            return task.ResponseRedirectUrl;
        }

        private CompleteHttpRequestScheduledTask ScheduleInternal(
            HttpRequestPriority priority,
            HttpRequestMessage request,
            int timeout = 0,
            bool isBinaryContent = false)
        {
            var taskTimeout = timeout != 0 ? timeout : _config.DeadRequestTimeoutInMilliseconds;

            var task = new CompleteHttpRequestScheduledTask(taskTimeout)
            {
                Request = request,
                Priority = priority,
                IsBinaryContent = isBinaryContent,
            };

            AddNewTask(priority, task);

            return task;
        }

        private void AddNewTask(HttpRequestPriority priority, HttpRequestScheduledTaskBase task)
        {
            _priorityToTasksCountMap.TryGetValue(priority, out var itemsCount);

            if (itemsCount > _config.MaxHttpLoadThreads)
            {
                RemoveOldestTask(priority);
            }

            _priorityToTasksCountMap.AddOrUpdate(priority, 1, (key, value) => value + 1);
            _taskIdToTaskMap.AddOrUpdate(task.Id, task, (key, value) => task);

            lock (_queueSyncLock)
            {
                var priorityForNewItem = GetPriorityForNewItem(priority);

                _tasksQueue.Enqueue(task, priorityForNewItem);
                _perPriorityQueue[priority].Enqueue(task, priorityForNewItem);
            }
        }

        private void RemoveOldestTask(HttpRequestPriority priority)
        {
            lock (_queueSyncLock)
            {
                _perPriorityQueue[priority].TryDequeue(out var oldestTask);

                _tasksQueue.TryRemove(oldestTask);

                if (oldestTask == null)
                {
                    return;
                }

                _taskIdToTaskMap.TryRemove(oldestTask.Id, out var _);

                _priorityToTasksCountMap.AddOrUpdate(oldestTask.Priority, 0, (key, value) => value - 1);
            }
        }

        private async Task WaitForTaskCompletionAsync(HttpRequestScheduledTaskBase task)
        {
            if (!task.Deferral.IsInProgress)
            {
                task.Deferral.Begin();
            }

            await task.Deferral.WaitForCompletionAsync().ConfigureAwait(false);
        }

        private void StartTasksExecution()
        {
            if (_tasksExecutionCancellation != null && _tasksExecutionCancellation.IsCancellationRequested == false)
            {
                _tasksExecutionCancellation.Cancel();
            }

            _tasksExecutionCancellation = new CancellationTokenSource();

            for (var workerIndex = 0; workerIndex < _config.MaxHttpLoadThreads; workerIndex++)
            {
                StartTasksExecutionWorker(workerIndex, _tasksExecutionCancellation.Token);
            }
        }

        private void StartTasksExecutionWorker(int workerIndex, CancellationToken cancellationToken)
        {
            Executor.InBackgroundThread(
                async () =>
                {
                    await PerformTaskExecutionWorkerIterationAsync(workerIndex, cancellationToken)
                        .ConfigureAwait(false);
                });
        }

        private async Task PerformTaskExecutionWorkerIterationAsync(int workerIndex,
            CancellationToken cancellationToken)
        {
            Task WaitIfNeededAsync()
            {
                if (_tasksQueue.Count == 0)
                {
                    return Task.Delay(
                        IsHighPriorityOnlyWorker(workerIndex)
                            ? HighPriorityWorkerTaskExecutionIterationDelayInMilliseconds
                            : WorkerTaskExecutionIterationDelayInMilliseconds, cancellationToken);
                }

                return Task.CompletedTask;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                if (IsHighPriorityOnlyWorker(workerIndex))
                {
                    _tasksQueue.TryFirst(out var possibleTask);

                    if (possibleTask == null || possibleTask.Priority != _highestPriority)
                    {
                        await WaitIfNeededAsync().ConfigureAwait(false);
                        continue;
                    }
                }

                HttpRequestScheduledTaskBase task;

                lock (_perPriorityQueue)
                {
                    _tasksQueue.TryDequeue(out task);

                    if (task != null)
                    {
                        _perPriorityQueue[task.Priority].TryRemove(task);
                    }
                }

                if (task == null)
                {
                    await WaitIfNeededAsync().ConfigureAwait(false);
                    continue;
                }

                _taskIdToTaskMap.TryRemove(task.Id, out _);

                _priorityToTasksCountMap.AddOrUpdate(task.Priority, 0, (key, value) => value - 1);

                await Executor.ExecuteSilentlyAsync(
                    async () =>
                    {
                        if (task is CompleteHttpRequestScheduledTask scheduledTask)
                        {
                            await ExecuteAsync(scheduledTask).ConfigureAwait(false);
                        }
                        else
                        {
                            await ExecuteAsync(task as RedirectHttpRequestScheduledTask).ConfigureAwait(false);
                        }
                    }).ConfigureAwait(false);

                task.Deferral.CompleteIfInProgress();

                await WaitIfNeededAsync().ConfigureAwait(false);
            }
        }

        private bool IsHighPriorityOnlyWorker(int workerIndex)
        {
            return workerIndex < _config.HighPriorityHttpLoadThreads;
        }

        private float GetPriorityForNewItem(HttpRequestPriority priority)
        {
            _priorityToTasksCountMap.TryGetValue(priority, out var tasksCountWithTargetPriority);

            return (int)priority * PriorityRange + tasksCountWithTargetPriority;
        }

        private async Task ExecuteAsync(CompleteHttpRequestScheduledTask task)
        {
            if (task.CancellationTokenSource.IsCancellationRequested)
            {
                task.Response = new HttpResponse
                {
                    IsSuccessful = false
                };

                return;
            }

            var timeoutCancellationToken = new CancellationTokenSource(task.Timeout).Token;
            try
            {
                var client = GetSimpleHttpClient();

                using (task.Request)
                {
                    var serverResponse = await client.SendAsync(task.Request, HttpCompletionOption.ResponseContentRead, timeoutCancellationToken)
                        .ConfigureAwait(false);

                    task.Response = new HttpResponse
                    {
                        StatusCode = GetStatusCode(serverResponse.StatusCode),
                        IsSuccessful = serverResponse.IsSuccessStatusCode,
                        ResponseUri = serverResponse.RequestMessage?.RequestUri,
                        Headers = serverResponse.Headers,
                        ContentHeaders = serverResponse.Content?.Headers,
                        Expires = serverResponse.Content?.Headers.Expires
                    };

                    if (task.IsBinaryContent)
                    {
                        task.Response.BinaryContent =
                            await GetBinaryContent(serverResponse.Content).ConfigureAwait(false);
                    }
                    else
                    {
                        task.Response.Content = await GetStringContent(serverResponse.Content).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                // KA: Android HttpClient throws native exception instread of TaskCanceledException(https://github.com/xamarin/xamarin-android/issues/3216).
                // iOS works as expected. Right exception is thrown.
                // To catch TimeoutException for both platforms cancellation token is checked.
                task.Response = timeoutCancellationToken.IsCancellationRequested
                    ? new HttpResponse
                    {
                        StatusCode = HttpStatusCode.RequestTimeout,
                        IsSuccessful = false
                    }
                    : _httpClientErrorHandler.FromException(ex);
            }
        }

        private async Task ExecuteAsync(RedirectHttpRequestScheduledTask task)
        {
            await Executor.ExecuteSilentlyAsync(
                async () =>
                {
                    var client = GetSimpleHttpClient();

                    var response = await client.GetAsync(new Uri(task.RequestUrl),
                        HttpCompletionOption.ResponseHeadersRead,
                        new CancellationTokenSource(task.Timeout).Token);

                    task.ResponseRedirectUrl = response.RequestMessage.RequestUri.ToString();
                }).ConfigureAwait(false);
        }

        private System.Net.Http.HttpClient GetSimpleHttpClient()
        {
            if (_simpleHttpClient == null)
            {
                _simpleHttpClient = _httpClientProvider.CreateHttpClient();
            }

            return _simpleHttpClient;
        }

        private async Task<byte[]> GetBinaryContent(HttpContent httpContent)
        {
            await httpContent.ReadAsStreamAsync().ConfigureAwait(false);

            return await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        private async Task<string> GetStringContent(HttpContent httpContent)
        {
            var result = string.Empty;

            await Executor.ExecuteWithRetryAsync(
                async executionContext =>
                {
                    if (httpContent.Headers?.ContentType == null)
                    {
                        result = await httpContent.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        var encoding = string.IsNullOrEmpty(httpContent.Headers.ContentType.CharSet)
                            ? Encoding.UTF8
                            : Encoding.GetEncoding(httpContent.Headers.ContentType.CharSet);

                        var buffer = await httpContent.ReadAsByteArrayAsync().ConfigureAwait(false);
                        result = encoding.GetString(buffer.ToArray(), 0, buffer.Length);
                    }
                }).ConfigureAwait(false);

            return result;
        }

        private HttpStatusCode GetStatusCode(HttpStatusCode httpStatusCode)
        {
            var code = (int)httpStatusCode;

            if (code > ErrorStatusCodeRangeMax)
            {
                return HttpStatusCode.InternalServerError;
            }

            if (!_supportedStatusCodes.Contains(code))
            {
                code = code / 100;
            }

            return (HttpStatusCode)code;
        }
    }
}