using System;
using System.Net;
using System.Runtime.Serialization;

namespace Arrba.Parser.Services
{
    public class HttpWebException : WebException
    {
        public HttpWebException(HttpStatusCode httpStatusCode)
        {
            this.HttpStatusCode = httpStatusCode;
        }
        public HttpWebException(HttpStatusCode httpStatusCode, string message) 
            : base(message)
        {
            this.HttpStatusCode = httpStatusCode;
        }
        public HttpWebException(HttpStatusCode httpStatusCode, string message, Exception innerException) 
            : base(message, innerException)
        {
            this.HttpStatusCode = httpStatusCode;
        }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}