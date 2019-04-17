// Developed for PAWS-HALO by Softeq Development Corporation
// http://www.softeq.com

using System.IO;
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

        public static StreamContent CreateStreamContent(Stream stream)
        {
            var photoCopyStream = new MemoryStream();
            stream.CopyTo(photoCopyStream);
            stream.Position = 0;
            photoCopyStream.Position = 0;

            return new StreamContent(photoCopyStream);
        }

        public static HttpContent StreamContentWithType(this Stream stream, string type)
        {
            return CreateStreamContent(stream).SetContentType(type);
        }

        public static HttpContent StreamContentWithType(this byte[] byteArray, string type)
        {
            return StreamContentWithType(new MemoryStream(byteArray), type);
        }
    }
}