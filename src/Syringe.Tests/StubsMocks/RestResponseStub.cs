using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace Syringe.Tests.StubsMocks
{
	public class RestResponseStub : IRestResponse
	{
		public IRestRequest Request { get; set; }
		public string ContentType { get; set; }
		public long ContentLength { get; set; }
		public string ContentEncoding { get; set; }
		public string Content { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string StatusDescription { get; set; }
		public byte[] RawBytes { get; set; }
		public Uri ResponseUri { get; set; }
		public string Server { get; set; }
		public IList<RestResponseCookie> Cookies { get; set; }
		public IList<Parameter> Headers { get; set; }
		public ResponseStatus ResponseStatus { get; set; }
		public string ErrorMessage { get; set; }
		public Exception ErrorException { get; set; }
	}
}