using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Softeq.XToolkit.CrossCutting
{
    public class HttpRequest
    {
        public const string ContentTypeJson = "application/json";
        public const string FormDataContentType = "multipart/form-data";

        public string ContentType { get; set; }
        public string Data { get; set; }
        public Dictionary<string, object> CustomHeaders { get; }
        public Dictionary<HttpRequestHeader, object> Headers { get; }
        public HttpMethod Method { get; set; }
        public Uri Uri { get; set; }
        public MultipartFormDataContent FormDataContent { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<HttpRequestHeader, object>();
            CustomHeaders = new Dictionary<string, object>();
        }

        public HttpRequest(Uri uri) : this()
        {
            Uri = uri;
            SetMethod(HttpMethods.Get);
        }

        public HttpRequest(Uri uri, HttpMethods method) : this(uri)
        {
            SetMethod(method);
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
            switch (newMethod)
            {
                case HttpMethods.Get:
                    Method = HttpMethod.Get;
                    break;
                case HttpMethods.Post:
                    Method = HttpMethod.Post;
                    break;
                case HttpMethods.Put:
                    Method = HttpMethod.Put;
                    break;
                case HttpMethods.Delete:
                    Method = HttpMethod.Delete;
                    break;
                case HttpMethods.Head:
                    Method = HttpMethod.Head;
                    break;
                case HttpMethods.Options:
                    Method = HttpMethod.Options;
                    break;
                case HttpMethods.Trace:
                    Method = HttpMethod.Trace;
                    break;
            }
            return this;
        }

        public HttpRequest SetUri(Uri newUri)
        {
            Uri = newUri;

            return this;
        }

        public HttpRequest SetUri(string newUri)
        {
            Uri = new Uri(newUri);

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

        public HttpRequest WithJsonData(object data)
        {
            return WithData(JsonConvert.SerializeObject(data));
        }

        public HttpRequest WithFormData(MultipartFormDataContent data)
        {
            ContentType = FormDataContentType;

            FormDataContent = data;

            return this;
        }
    }
}