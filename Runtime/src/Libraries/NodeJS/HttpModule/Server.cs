using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;
using NodeJS.EventsModule;
using NodeJS.NetModule;

namespace NodeJS.HttpModule {
	[Imported]
	[ModuleName("http")]
	[IgnoreNamespace]
	public class Server : EventEmitter {
		public Server() {}

		public Server(Action<ServerRequest, ServerResponse> requestListener) {}

		public void Listen(int port) {}

		public void Listen(int port, string hostname) {}

		public void Listen(int port, string hostname, int backlog) {}

		public void Listen(int port, Action callback) {}

		public void Listen(int port, string hostname, Action callback) {}

		public void Listen(int port, string hostname, int backlog, Action callback) {}

        [InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({this}, 'listen', {port})")]
		public Task ListenTask(int port) { return null; }

        [InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({this}, 'listen', {port}, {hostname})")]
		public Task ListenTask(int port, string hostname) { return null; }

        [InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({this}, 'listen', {port}, {hostname}, {backlog})")]
		public Task ListenTask(int port, string hostname, int backlog) { return null; }

		public void Listen(string path) {}

		public void Listen(string path, Action callback) {}

        [InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({this}, 'listen', {path})")]
		public Task ListenTask(string path) { return null; }

		public void Listen(object handle) {}

		public void Listen(object handle, Action callback) {}

        [InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({this}, 'listen', {handle})")]
		public Task ListenTask(object handle) { return null; }


		public void Close() {}

		public void Close(Action callback) {}

        [InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({this}, 'close')")]
		public Task CloseTask() { return null; }

		[IntrinsicProperty]
		public int MaxHeadersCount { get; set; }


		public event Action<ServerRequest, ServerResponse> OnRequest {
			[InlineCode("{this}.addListener('request', {value})")] add {}
			[InlineCode("{this}.removeListener('request', {value})")] remove {}
		}

		[InlineCode("{this}.once('request', {callback})")]
		public void OnceRequest(Action<ServerRequest, ServerResponse> callback) {}


		public event Action<Socket> OnConnection {
			[InlineCode("{this}.addListener('connection', {value})")] add {}
			[InlineCode("{this}.removeListener('connection', {value})")] remove {}
		}

		[InlineCode("{this}.once('connection', {callback})")]
		public void OnceConnection(Action<Socket> callback) {}


		public event Action OnClose {
			[InlineCode("{this}.addListener('close', {value})")] add {}
			[InlineCode("{this}.removeListener('close', {value})")] remove {}
		}

		[InlineCode("{this}.once('close', {callback})")]
		public void OnceClose(Action callback) {}


		public event Action<ServerRequest, ServerResponse> OnCheckContinue {
			[InlineCode("{this}.addListener('checkContinue', {value})")] add {}
			[InlineCode("{this}.removeListener('checkContinue', {value})")] remove {}
		}

		[InlineCode("{this}.once('checkContinue', {callback})")]
		public void OnceCheckContinue(Action<ServerRequest, ServerResponse> callback) {}


		public event Action<ServerRequest, Socket, Buffer> OnConnect {
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


		public event Action<Error> ClientError {
			[InlineCode("{this}.addListener('clientError', {value})")] add {}
			[InlineCode("{this}.removeListener('clientError', {value})")] remove {}
		}

		[InlineCode("{this}.once('clientError', {callback})")]
		public void OnceClientError(Action<Error> callback) {}
	}
}
