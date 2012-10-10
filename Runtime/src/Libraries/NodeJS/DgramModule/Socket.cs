using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;
using NodeJS.EventsModule;

namespace NodeJS.DgramModule {
	[Imported]
	[ModuleName("dgram")]
	[IgnoreNamespace]
	public class Socket : EventEmitter {
		private Socket() {}

		public void Send(Buffer buf, int offset, int length, int port, string address) {}
		public void Send(Buffer buf, int offset, int length, int port, string address, Action<Error, int> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({this}, 'send', {offset}, {length}, {port}, {address})")]
		public Task<int> SendTask(Buffer buf, int offset, int length, int port, string address) { return null; }

		public void Bind(int port) {}
		public void Bind(int port, string address) {}

		public void Close() {}

		public NetModule.SocketAddress Address { [ScriptName("address")] get { return null; } }

		public void SetBroadcast(bool flag) {}

		[ScriptName("setTTL")]
		public void SetTTL(int ttl) {}

		[ScriptName("setMulticastTTL")]
		public void SetMulticastTTL(int ttl) {}

		public void SetMulticastLoopback(bool flag) {}

		public void AddMembership(string multicastAddress) {}
		public void AddMembership(string multicastAddress, string multicastInterface) {}

		public void DropMembership(string multicastAddress) {}
		public void DropMembership(string multicastAddress, string multicastInterface) {}


		public event Action<Buffer, object> OnMessage {
			[InlineCode("{this}.addListener('message', {value})")] add {}
			[InlineCode("{this}.removeListener('message', {value})")] remove {}
		}

		[InlineCode("{this}.once('message', {callback})")]
		public void OnceMessage(Action<Buffer, object> callback) {}


		public event Action OnListening {
			[InlineCode("{this}.addListener('listening', {value})")] add {}
			[InlineCode("{this}.removeListener('listening', {value})")] remove {}
		}

		[InlineCode("{this}.once('listening', {callback})")]
		public void OnceListening(Action callback) {}


		public event Action OnClose {
			[InlineCode("{this}.addListener('close', {value})")] add {}
			[InlineCode("{this}.removeListener('close', {value})")] remove {}
		}

		[InlineCode("{this}.once('close', {callback})")]
		public void OnceClose(Action callback) {}


		public event Action<Error> OnError {
			[InlineCode("{this}.addListener('error', {value})")] add {}
			[InlineCode("{this}.removeListener('error', {value})")] remove {}
		}

		[InlineCode("{this}.once('error', {callback})")]
		public void OnceError(Action<Error> callback) {}
	}
}
