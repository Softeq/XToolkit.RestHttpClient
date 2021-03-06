﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting;
using Softeq.XToolkit.CrossCutting.Exceptions;
using Softeq.XToolkit.HttpClient.Abstract;
using Softeq.XToolkit.HttpClient.Infrastructure;
using Softeq.XToolkit.HttpClient.Network;

namespace Softeq.XToolkit.HttpClient
{
    public class HttpServiceGate : IHttpServiceGate
    {
        private readonly ModifiedHttpClient _client;

        public HttpServiceGate(
            IHttpClientProvider httpClientProvider,
            IHttpClientErrorHandler httpClientErrorHandler,
            HttpServiceGateConfig httpConfig)
        {
            _client = new ModifiedHttpClient(
                new HttpRequestsScheduler(httpClientProvider, httpClientErrorHandler, httpConfig));
        }

        async Task<HttpResponse> IHttpServiceGate.ExecuteApiCallAsync(
            HttpRequest request,
            int timeout,
            HttpRequestPriority priority,
            bool isBinaryContent,
            params HttpStatusCode[] ignoreErrorCodes)
        {
            HttpResponse response;
            if (isBinaryContent)
            {
                response = await _client.ExecuteAsBinaryResponseAsync(request, priority, timeout)
                    .ConfigureAwait(false);
            }
            else
            {
                response = await _client.ExecuteAsStringResponseAsync(request, priority, timeout)
                    .ConfigureAwait(false);
            }

            if (response == null)
            {
                HandleInvalidResponse(response);
                return response;
            }
            else if (response.IsSuccessful || ignoreErrorCodes.Contains(response.StatusCode))
            {
                return response;
            }
            else
            {
                HandleInvalidResponse(response);
                return response;
            }
        }

        protected virtual void HandleInvalidResponse(HttpResponse response)
        {
            if (response == null)
            {
                throw new HttpException("Response is null!");
            }
            if (HttpStatusCodes.IsErrorStatus(response.StatusCode))
            {
                throw new HttpException($"Error status code received {response.StatusCode}", response);
            }
        }
    }
}