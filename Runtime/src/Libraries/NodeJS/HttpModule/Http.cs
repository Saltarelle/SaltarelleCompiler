using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.HttpModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("http")]
	public static class Http {
		[IntrinsicProperty, ScriptName("STATUS_CODES")] public static JsDictionary<int, string> StatusCodes { get; private set; }

		public static Server CreateServer(Action<ServerRequest, ServerResponse> requestListener) { return null; }

		public static ClientRequest Request(RequestOptions options, Action<ClientResponse> callback) { return null; }

		public static ClientRequest Get(RequestOptions options, Action<ClientResponse> callback) { return null; }

		[IntrinsicProperty] public static Agent GlobalAgent { get; private set; }
	}
}
