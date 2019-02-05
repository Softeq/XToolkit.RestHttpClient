// Developed for PAWS-HALO by Softeq Development Corporation
// http://www.softeq.com

using System.Net.Http;

namespace Softeq.XToolkit.HttpClient.Extensions
{
    public static class HttpContentExtensions
    {
        private const string ContentType = "Content-Type";

        public static HttpContent SetContentType(this HttpContent content, string type)
        {
            content.Headers.Add(ContentType, type);
            return content;
        }
    }
}