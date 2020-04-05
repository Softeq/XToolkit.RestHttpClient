﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Softeq.XToolkit.CrossCutting
{
    public class HttpResponse : ICloneable
    {
        public string Content { get; set; }
        public byte[] BinaryContent { get; set; }
        public bool IsSuccessful { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public bool IsNoInternet { get; set; }
        public Uri ResponseUri { get; set; }
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ContentHeaders { get; set; }
        public DateTimeOffset? Expires { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

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
