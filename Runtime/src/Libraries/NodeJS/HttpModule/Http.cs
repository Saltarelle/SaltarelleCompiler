using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.HttpModule {
	[ModuleName("http")]
	[GlobalMethods]
	[Imported]
	public static class Http {
		// TODO: [ScriptName("STATUS_CODES")] public static JsDictionary<int, string> StatusCodes;

		public static Server CreateServer(Action<ServerRequest, ServerResponse> requestListener) { return null; }

		public static ClientRequest Request(RequestOptions options, Action<ClientResponse> callback) { return null; }

		public static ClientRequest Get(RequestOptions options, Action<ClientResponse> callback) { return null; }

		// TODO: public static Agent GlobalAgent
	}
}
