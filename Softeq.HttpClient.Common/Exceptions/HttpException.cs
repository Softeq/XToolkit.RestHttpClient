// Developed for LilBytes by Softeq Development Corporation
//

using System;

namespace Softeq.HttpClient.Common.Exceptions
{
    public class HttpException : Exception
    {
        public HttpResponse Response { get; set; }

        public HttpException(string message) : base(message)
        {
        }

        public HttpException(string message, HttpResponse response) : base(message)
        {
            Response = response;
        }
    }
}
