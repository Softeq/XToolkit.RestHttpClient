// Developed for PAWS-HALO by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public class HttpClientNoInternetHandler : IHttpClientErrorHandler
    {
        public virtual HttpResponse FromException(Exception e)
        {
            return new HttpResponse
            {
                IsSuccessful = false,
                IsNoInternet = true
            };
        }
    }
}