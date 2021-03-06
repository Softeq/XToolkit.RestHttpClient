﻿namespace Softeq.XToolkit.CrossCutting.Exceptions
{
    public class PoorInternetException : HttpException
    {
        public PoorInternetException(string message) : base(message)
        {
        }

        public PoorInternetException(string message, HttpResponse response)
            : base(message, response)
        {
        }
    }
}
