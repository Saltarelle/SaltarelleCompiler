using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.NetModule;

namespace NodeJS.HttpModule {
	[Imported]
	[ModuleName("http")]
	[IgnoreNamespace]
	public class ServerRequest : ReadableStream {
		private ServerRequest() {}

		[IntrinsicProperty]
		public string Method { get; private set; }

		[IntrinsicProperty]
		public string Url { get; private set; }

		[IntrinsicProperty]
		public JsDictionary<string, string> Headers { get; private set; }

		[IntrinsicProperty]
		public JsDictionary<string, string> Trailers { get; private set; }

		[IntrinsicProperty]
		public string HttpVersion { get; private set; }

		[IntrinsicProperty]
		public Socket Connection { get; private set; }
	}
}
