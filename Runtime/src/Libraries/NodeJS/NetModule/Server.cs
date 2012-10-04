using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NodeJS.NetModule {
	[Imported]
	[ModuleName("net")]
	[IgnoreNamespace]
	public class Server : Socket {
		public Server() {}

		public Server(ServerOptions options) {}

		public Server(Action<Socket> connectionListener) {}

		public Server(ServerOptions options, Action<Socket> connectionListener) {}


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
		public int MaxConnections { get; set; }

		[IntrinsicProperty]
		public int? Connections { get; set; }


		public event Action OnListening {
			[InlineCode("{this}.addListener('listening', {value})")] add {}
			[InlineCode("{this}.removeListener('listening', {value})")] remove {}
		}

		[InlineCode("{this}.once('listening', {callback})")]
		public void OnceListening(Action callback) {}


		public event Action<Socket> OnConnection {
			[InlineCode("{this}.addListener('connection', {value})")] add {}
			[InlineCode("{this}.removeListener('connection', {value})")] remove {}
		}

		[InlineCode("{this}.once('connection', {callback})")]
		public void OnceConnection(Action<Socket> callback) {}
	}
}
