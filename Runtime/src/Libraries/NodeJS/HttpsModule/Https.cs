using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.HttpModule;

namespace NodeJS.HttpsModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("https")]
	public static class Https {
		public static Server CreateServer(Action<ServerRequest, ServerResponse> requestListener) { return null; }

		public static ClientRequest Request(RequestOptions options, Action<ClientResponse> callback) { return null; }

		public static ClientRequest Get(RequestOptions options, Action<ClientResponse> callback) { return null; }

		[IntrinsicProperty] public static Agent GlobalAgent { get; private set; }
	}
}
