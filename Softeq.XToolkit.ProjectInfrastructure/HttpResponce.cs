using System;
using System.Collections.Generic;
using System.Net;

namespace Softeq.XToolkit.CrossCutting
{
    public class HttpResponse
    {
        private bool _isNoInternetConnection;

        public string Content { get; set; }
        public byte[] BinaryContent { get; set; }
        public bool IsSuccessful { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public bool IsPoorConnection
        {
            get => _isNoInternetConnection ||
                    StatusCode == HttpStatusCode.GatewayTimeout ||
                    StatusCode == HttpStatusCode.RequestTimeout ||
                    StatusCode == HttpStatusCode.BadGateway;
            set => _isNoInternetConnection = value;
        }

        public Uri ResponseUri { get; set; }
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ContentHeaders { get; set; }
        public DateTimeOffset? Expires { get; set; }

        public T ParseContentAsJson<T>()
        {
            return JsonConverter.Deserialize<T>(Content);
        }

        public bool TryParseContentAsJson<T>(out T result)
        {
            return JsonConverter.TryDeserialize(Content, out result);
        }
    }
}