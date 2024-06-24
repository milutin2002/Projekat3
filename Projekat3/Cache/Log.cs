using System;
using System.Net;

namespace Projekat3.Cache
{
    public class Log
    {
        public HttpStatusCode statusCode { get;  }
        public long contentLength { get;  }
        public string contentType { get;  }
        public byte[] content { get; }
        public DateTime expires { get;  }

        public Log(HttpStatusCode statusCode, long contentLength, string contentType, byte[] content)
        {
            this.statusCode = statusCode;
            this.contentLength = contentLength;
            this.contentType = contentType;
            this.content = content;
            expires=DateTime.Now+TimeSpan.FromMinutes(30);
        }
    }
}