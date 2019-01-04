using System.Net;

namespace Softeq.XToolkit.CrossCutting.Exceptions
{
    public class NoServiceErrorMessage
    {
        public string Host { get; }
        public string Path { get; }
        public HttpStatusCode StatusCode { get; }
        public string Response { get; }

        public NoServiceErrorMessage(string host, string path, HttpStatusCode code, string response)
        {
            Host = host;
            Path = path;
            StatusCode = code;
            Response = response;
        }
    }
}
