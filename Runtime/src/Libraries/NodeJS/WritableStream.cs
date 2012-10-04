using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.BufferModule;
using NodeJS.EventsModule;

namespace NodeJS {
	[Imported]
	public class WritableStream : EventEmitter {
		[NonScriptable]
		public WritableStream() {}

		[IntrinsicProperty]
		public bool Writable { get; private set; }

		public bool Write(string data) { return false; }

		public bool Write(string data, Encoding encoding) { return false; }

		public bool Write(Buffer data) { return false; }

		public void End() {}

		public void End(string data) {}

		public void End(string data, Encoding encoding) {}

		public void End(Buffer data) {}

		public void Destroy() {}

		public void DestroySoon() {}


		public event Action OnDrain {
			[InlineCode("{this}.addListener('drain', {value})")] add {}
			[InlineCode("{this}.removeListener('drain', {value})")] remove {}
		}

		[InlineCode("{this}.once('drain', {callback})")]
		public void OnceDrain(Action callback) {}


		public event Action<Error> OnError {
			[InlineCode("{this}.addListener('error', {value})")] add {}
			[InlineCode("{this}.removeListener('error', {value})")] remove {}
		}

		[InlineCode("{this}.once('error', {callback})")]
		public void OnceError(Action<Error> callback) {}


		public event Action OnClose {
			[InlineCode("{this}.addListener('close', {value})")] add {}
			[InlineCode("{this}.removeListener('close', {value})")] remove {}
		}

		[InlineCode("{this}.once('close', {callback})")]
		public void OnceClose(Action callback) {}


		public event Action OnPipe {
			[InlineCode("{this}.addListener('pipe', {value})")] add {}
			[InlineCode("{this}.removeListener('pipe', {value})")] remove {}
		}

		[InlineCode("{this}.once('pipe', {callback})")]
		public void OncePipe(Action callback) {}
	}
}
