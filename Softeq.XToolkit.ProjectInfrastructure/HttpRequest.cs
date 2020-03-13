using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Softeq.XToolkit.CrossCutting.Content;

namespace Softeq.XToolkit.CrossCutting
{
    public class HttpRequest
    {
        public const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";
        public const string ContentTypeJson = "application/json";
        public const string FormDataContentType = "multipart/form-data";

        private string _serializedData;
        private object _data;
        private SemaphoreSlim _serializeDataLock = new SemaphoreSlim(1);
        public Dictionary<string, object> CustomHeaders { get; }
        public Dictionary<HttpRequestHeader, object> Headers { get; }
        public HttpMethod Method { get; set; }
        public Uri Uri { get; set; }
        public IHttpContentProvider FormDataProvider { get; set; }

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
            object data = null,
            string contentType = ContentTypeJson,
            IDictionary<string, object> headers = null)
            : this(uri, method)
        {
            Data = data;
            ContentType = contentType;
            CustomHeaders = new Dictionary<string, object>(headers ?? new Dictionary<string, object>());
        }

        public object Data
        {
            get => _data;
            private set
            {
                _serializeDataLock.Wait();

                _data = value;
                _serializedData = null;

                _serializeDataLock.Release();
            }
        }

        public string ContentType { get; private set; }

        public async Task<string> GetSerializedDataAsync()
        {
            try
            {
                await _serializeDataLock.WaitAsync()
                    .ConfigureAwait(false);

                if (_serializedData == null)
                {
                    _serializedData = await SerializeDataAsync(this)
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                _serializeDataLock.Release();
            }

            return _serializedData;
        }

        public HttpRequest WithHeader(string name, object value)
        {
            CustomHeaders[name] = value;

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

        public HttpRequest WithFormUrlEncodedContent(object data)
        {
            ContentType = FormUrlEncodedContentType;

            Data = data;

            return this;
        }

        public HttpRequest WithJsonData(object data)
        {
            ContentType = ContentTypeJson;

            Data = data;

            return this;
        }

        public HttpRequest WithFormDataProvider(IHttpContentProvider httpContentProvider)
        {
            ContentType = FormDataContentType;

            FormDataProvider = httpContentProvider;

            return this;
        }

        private async Task<string> SerializeDataAsync(HttpRequest request)
        {
            if (request.ContentType == ContentTypeJson)
            {
                return JsonConverter.Serialize(Data) ?? string.Empty;
            }
            else if (request.ContentType == FormUrlEncodedContentType)
            {
                var dict = Data.GetType()
                    .GetProperties()
                    .Select(x => new KeyValuePair<string, string>(ToUnderscoreCase(x.Name), x.GetValue(Data).ToString()));

                return await new FormUrlEncodedContent(dict).ReadAsStringAsync()
                    .ConfigureAwait(false);
            }

            throw new ArgumentOutOfRangeException();
        }

        private string ToUnderscoreCase(string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}