using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;
using NodeJS.EventsModule;
using NodeJS.HttpModule;
using NodeJS.NetModule;

namespace NodeJS.HttpsModule {
	[Imported]
	[ModuleName("https")]
	[IgnoreNamespace]
	public class Server : TlsModule.Server {
		private Server() {}

		public event Action<ServerRequest, ServerResponse> OnRequest {
			[InlineCode("{this}.addListener('request', {value})")] add {}
			[InlineCode("{this}.removeListener('request', {value})")] remove {}
		}

		[InlineCode("{this}.once('request', {callback})")]
		public void OnceRequest(Action<ServerRequest, ServerResponse> callback) {}


		public event Action<ServerRequest, ServerResponse> OnCheckContinue {
			[InlineCode("{this}.addListener('checkContinue', {value})")] add {}
			[InlineCode("{this}.removeListener('checkContinue', {value})")] remove {}
		}

		[InlineCode("{this}.once('checkContinue', {callback})")]
		public void OnceCheckContinue(Action<ServerRequest, ServerResponse> callback) {}


		public new event Action<ServerRequest, Socket, Buffer> OnConnect {
			[InlineCode("{this}.addListener('connect', {value})")] add {}
			[InlineCode("{this}.removeListener('connect', {value})")] remove {}
		}

		[InlineCode("{this}.once('connect', {callback})")]
		public void OnceConnect(Action<ServerRequest, Socket, Buffer> callback) {}


		public event Action<ServerRequest, Socket, Buffer> OnUpgrade {
			[InlineCode("{this}.addListener('upgrade', {value})")] add {}
			[InlineCode("{this}.removeListener('upgrade', {value})")] remove {}
		}

		[InlineCode("{this}.once('upgrade', {callback})")]
		public void OnceUpgrade(Action<ServerRequest, Socket, Buffer> callback) {}

	}
}
