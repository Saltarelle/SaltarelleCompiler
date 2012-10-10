using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.NetModule {
	[Imported]
	[ModuleName("net")]
	[IgnoreNamespace]
	public class Socket : ReadWriteStream {
		public Socket() {
		}

		public Socket(SocketOptions options) {
		}

		public void Connect(int port) {}

		public void Connect(int port, string host) {}

		public void Connect(int port, Action<Socket> connectListener) {}

		public void Connect(int port, string host, Action<Socket> connectListener) {}

		public void Connect(string path) {}

		public void Connect(string path, Action<Socket> connectListener) {}

		
		[IntrinsicProperty]
		public int BufferSize { get; private set; }


		public void SetTimeout(int timeout) {}

		public void SetTimeout(int timeout, Action callback) {}

		public void SetNoDelay() {}

		public void SetNoDelay(bool noDelay) {}

		public void SetKeepAlive(bool enable) {}

		public void SetKeepAlive(bool enable, int initialDelay) {}

		public SocketAddress Address { [ScriptName("address")] get; private set; }

		[IntrinsicProperty]
		public string RemoteAddress { get; private set; }

		[IntrinsicProperty]
		public int RemotePort { get; private set; }

		[IntrinsicProperty]
		public int BytesRead { get; private set; }

		[IntrinsicProperty]
		public int BytesWritten { get; private set; }


		public event Action OnConnect {
			[InlineCode("{this}.addListener('connect', {value})")] add {}
			[InlineCode("{this}.removeListener('connect', {value})")] remove {}
		}

		[InlineCode("{this}.once('connect', {callback})")]
		public void OnceConnect(Action callback) {}


		public event Action OnTimeout {
			[InlineCode("{this}.addListener('timeout', {value})")] add {}
			[InlineCode("{this}.removeListener('timeout', {value})")] remove {}
		}

		[InlineCode("{this}.once('timeout', {callback})")]
		public void OnceTimeout(Action callback) {}


		public new event Action<bool> OnClose {
			[InlineCode("{this}.addListener('close', {value})")] add {}
			[InlineCode("{this}.removeListener('close', {value})")] remove {}
		}

		[InlineCode("{this}.once('close', {callback})")]
		public void OnceClose(Action<bool> callback) {}
	}
}
