// Developed for LilBytes by Softeq Development Corporation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Softeq.XToolkit.HttpClient.Extensions;

namespace Softeq.XToolkit.HttpClient.Network
{
    public static class HttpStatusCodes
    {
        private const int ErrorStatusCodeRangeMin = 400;
        private const int ServerErrorStatusCodeRangeMin = 500;
        private const int ErrorStatusCodeRangeMax = 599;

        private static readonly HashSet<HttpStatusCode> ServerErrorCodes =
            new HashSet<HttpStatusCode>(
                Enum.GetValues(typeof(HttpStatusCode)).Cast<HttpStatusCode>().Where(x => (int) x >= ServerErrorStatusCodeRangeMin));

        private static readonly HashSet<HttpStatusCode> ErrorCodes = new HashSet<HttpStatusCode>(Enum.GetValues(typeof(HttpStatusCode))
            .Cast<HttpStatusCode>()
            .Where(x => ((int) x).IsIntInRange(ErrorStatusCodeRangeMin, ErrorStatusCodeRangeMax)));

        public static HashSet<HttpStatusCode> GetServerErrors()
        {
            return ServerErrorCodes;
        }

        public static HashSet<HttpStatusCode> GetErrorCodes()
        {
            return ErrorCodes;
        }

        public static bool IsErrorStatus(HttpStatusCode httpStatusCode)
        {
            return ErrorCodes.Contains(httpStatusCode);
        }
    }
}