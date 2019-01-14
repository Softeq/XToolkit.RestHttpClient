using System;
using System.Collections.Generic;
using System.Net;

namespace Softeq.XToolkit.CrossCutting
{
    public class HttpRequest
    {
        public const string ContentTypeJson = "application/json";

        public string ContentType { get; set; }
        public string Data { get; set; }
        public Dictionary<string, object> CustomHeaders { get; }
        public Dictionary<HttpRequestHeader, object> Headers { get; }
        public string Method { get; set; }
        public Uri Uri { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<HttpRequestHeader, object>();
            CustomHeaders = new Dictionary<string, object>();
        }

        public HttpRequest(Uri uri) : this()
        {
            Uri = uri;
            Method = HttpMethods.Get.ToString();
        }

        public HttpRequest(Uri uri, HttpMethods method) : this(uri)
        {
            Method = method.ToString();
        }

        public HttpRequest(Uri uri,
            HttpMethods method,
            string data = null,
            string contentType = ContentTypeJson,
            IDictionary<string, object> headers = null)
            : this(uri, method)
        {
            Data = data;
            ContentType = contentType;
            CustomHeaders = new Dictionary<string, object>(headers ?? new Dictionary<string, object>());
        }

        public HttpRequest WithHeader(string name, object value)
        {
            CustomHeaders.Add(name, value);

            return this;
        }

        public HttpRequest WithHeader(HttpRequestHeader header, object value)
        {
            Headers.Add(header, value);

            return this;
        }

        public HttpRequest SetMethod(HttpMethods newMethod)
        {
            Method = newMethod.ToString();

            return this;
        }

        public HttpRequest SetUri(Uri newUri)
        {
            Uri = newUri;

            return this;
        }

        public HttpRequest DisableCaching()
        {
            CustomHeaders.Add("cache-control", "no-cache");

            return this;
        }

        public HttpRequest WithData(string data)
        {
            ContentType = ContentTypeJson;

            Data = data;

            return this;
        }
    }
}