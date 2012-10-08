using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.BufferModule;
using NodeJS.NetModule;

namespace NodeJS.HttpModule {
	[Imported]
	[ModuleName("http")]
	[IgnoreNamespace]
	public class ClientRequest : WritableStream {
		private ClientRequest() {}

		public void Abort() {}

		public void SetTimeout(int timeout) {}

		public void SetTimeout(int timeout, Action timeoutListener) {}

		public void SetNoDelay(bool noDelay) {}

		public void SetSocketKeepAlive(bool enable) {}

		public void SetSocketKeepAlive(bool enable, int initialDelay) {}


		public event Action<ClientResponse> OnResponse {
			[InlineCode("{this}.addListener('response', {value})")] add {}
			[InlineCode("{this}.removeListener('response', {value})")] remove {}
		}

		[InlineCode("{this}.once('response', {callback})")]
		public void OnceRequest(Action<ClientResponse> callback) {}


		public event Action<ClientResponse> OnSocket {
			[InlineCode("{this}.addListener('socket', {value})")] add {}
			[InlineCode("{this}.removeListener('socket', {value})")] remove {}
		}

		[InlineCode("{this}.once('socket', {callback})")]
		public void OnceSocket(Action<ClientResponse> callback) {}


		public event Action<ClientResponse, Socket, Buffer> OnConnect {
			[InlineCode("{this}.addListener('connect', {value})")] add {}
			[InlineCode("{this}.removeListener('connect', {value})")] remove {}
		}

		[InlineCode("{this}.once('connect', {callback})")]
		public void OnceConnect(Action<ClientResponse, Socket, Buffer> callback) {}


		public event Action<ClientResponse, Socket, Buffer> OnUpgrade {
			[InlineCode("{this}.addListener('upgrade', {value})")] add {}
			[InlineCode("{this}.removeListener('upgrade', {value})")] remove {}
		}

		[InlineCode("{this}.once('upgrade', {callback})")]
		public void OnceUpgrade(Action<ClientResponse, Socket, Buffer> callback) {}


		public event Action OnContinue {
			[InlineCode("{this}.addListener('continue', {value})")] add {}
			[InlineCode("{this}.removeListener('continue', {value})")] remove {}
		}

		[InlineCode("{this}.once('continue', {callback})")]
		public void OnceContinue(Action callback) {}
	}
}
