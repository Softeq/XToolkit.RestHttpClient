﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Infrastructure;

namespace Softeq.XToolkit.HttpClient.Network
{
    public class ModifiedHttpClient : IHttpClient
    {
        private readonly IHttpRequestsScheduler _httpRequestsScheduler;

        public ModifiedHttpClient(IHttpRequestsScheduler httpRequestsScheduler)
        {
            _httpRequestsScheduler = httpRequestsScheduler;
        }

        public Task<HttpResponse> ExecuteAsStringResponseAsync(HttpRequest request,
            HttpRequestPriority priority = HttpRequestPriority.Normal,
            int timeout = 0)
        {
            return ExecuteHttpRequestInternal(priority, request, timeout: timeout);
        }

        public Task<HttpResponse> ExecuteAsBinaryResponseAsync(HttpRequest request,
            HttpRequestPriority priority = HttpRequestPriority.Normal,
            int timeout = 0)
        {
            return ExecuteHttpRequestInternal(priority, request, true, timeout);
        }

        private Task<HttpResponse> ExecuteHttpRequestInternal(
            HttpRequestPriority priority,
            HttpRequest request,
            bool isBinaryContent = false,
            int timeout = 0)
        {
            var message = new HttpRequestMessage(request.Method, request.Uri);

            foreach (var header in request.Headers)
            {
                ApplyHeader(message, header);
            }

            foreach (var header in request.CustomHeaders)
            {
                message.Headers.Add(header.Key, header.Value.ToString());
            }

            if (!string.IsNullOrEmpty(request.Data) && !string.IsNullOrEmpty(request.ContentType))
            {
                message.Content = new StringContent(request.Data ?? string.Empty);
                message.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
            }
            else if (request.FormDataContent != null)
            {
                message.Content = request.FormDataContent;
            }

            return _httpRequestsScheduler.ExecuteAsync(
                priority,
                message,
                timeout,
                isBinaryContent);
        }

        public async Task<string> GetRedirectedUrlAsync(string urlWithRedirect,
            HttpRequestPriority priority = HttpRequestPriority.Normal)
        {
            return await _httpRequestsScheduler.ExecuteRedirectOnlyAsync(priority, urlWithRedirect)
                .ConfigureAwait(false);
        }

        private void ApplyHeader(HttpRequestMessage message, KeyValuePair<HttpRequestHeader, object> header)
        {
            switch (header.Key)
            {
                case HttpRequestHeader.IfModifiedSince:
                    message.Headers.IfModifiedSince = (DateTime) header.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(header),
                        "Header cannot be applied using our HttpClient implementation");
            }
        }
    }
}