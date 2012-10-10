using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.HttpModule {
	[Imported]
	[ModuleName("http")]
	[IgnoreNamespace]
	public class ServerResponse : WritableStream {
		private ServerResponse() {}

		public void WriteContinue() {}

		public void WriteHead(int statusCode) {}

		public void WriteHead(int statusCode, string reasonPhrase) {}

		public void WriteHead(int statusCode, JsDictionary<string, string> headers) {}

		public void WriteHead(int statusCode, string reasonPhrase, JsDictionary<string, string> headers) {}

		[IntrinsicProperty]
		public int StatusCode { get; set; }

		public void SetHeader(string name, string value) {}

		public void SetHeader(string name, params string[] value) {}

		[IntrinsicProperty]
		public bool SendDate { get; set; }

		public string GetHeader(string name) { return null; }

		public string RemoveHeader(string name) { return null; }

		public void AddTrailers(JsDictionary<string, string> trailers) {}
	}
}
