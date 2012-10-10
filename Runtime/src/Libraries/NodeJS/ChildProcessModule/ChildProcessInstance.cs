using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.EventsModule;
using NodeJS.NetModule;

namespace NodeJS.ChildProcessModule {
	[Imported]
	[ModuleName("child_process")]
	[ScriptName("ChildProcess")]
	[IgnoreNamespace]
	public class ChildProcessInstance : EventEmitter {
		[IntrinsicProperty]
		public WritableStream Stdin { get; private set; }

		[IntrinsicProperty]
		public ReadableStream Stdout { get; private set; }

		[IntrinsicProperty]
		public ReadableStream Stderr { get; private set; }

		[IntrinsicProperty]
		public int Pid { get; private set; }

		public void Kill() {}

		public void Kill(string signal) {}

		public void Send(object message) {}

		public void Send(object message, Socket sendHandle) {}

		public void Disconnect() {}

		public event Action<int, string> OnExit {
			[InlineCode("{this}.addListener('exit', {value})")] add {}
			[InlineCode("{this}.removeListener('exit', {value})")] remove {}
		}

		[InlineCode("{this}.once('exit', {callback})")]
		public void OnceExit(Action<int, string> callback) {}
		

		public event Action OnClose {
			[InlineCode("{this}.addListener('close', {value})")] add {}
			[InlineCode("{this}.removeListener('close', {value})")] remove {}
		}

		[InlineCode("{this}.once('close', {callback})")]
		public void OnceClose(Action callback) {}


		public event Action OnDisconnect {
			[InlineCode("{this}.addListener('disconnect', {value})")] add {}
			[InlineCode("{this}.removeListener('disconnect', {value})")] remove {}
		}

		[InlineCode("{this}.once('disconnect', {callback})")]
		public void OnceDisconnect(Action callback) {}


		public event Action<object, Socket> OnMessage {
			[InlineCode("{this}.addListener('message', {value})")] add {}
			[InlineCode("{this}.removeListener('message', {value})")] remove {}
		}

		[InlineCode("{this}.once('message', {callback})")]
		public void OnceMessage(Action<object, Socket> callback) {}
	}
}
