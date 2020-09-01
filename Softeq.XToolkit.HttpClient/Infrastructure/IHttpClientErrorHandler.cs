// Developed for PAWS-HALO by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.CrossCutting;

namespace Softeq.XToolkit.HttpClient.Infrastructure
{
    public interface IHttpClientErrorHandler
    {
        HttpResponse HandleException(Exception e);
    }
}