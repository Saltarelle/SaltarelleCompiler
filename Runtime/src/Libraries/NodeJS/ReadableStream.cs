using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.BufferModule;
using NodeJS.EventsModule;

namespace NodeJS {
	[Imported]
	public class ReadableStream : EventEmitter {
		[IntrinsicProperty]
		public bool Readable { get; private set; }

		public void SetEncoding(Encoding encoding) {}

		public void Pause() {}

		public void Resume() {}

		public void Destroy() {}

		public void Pipe(WritableStream dest) {}

		public void Pipe(WritableStream dest, PipeOptions options) {}


		public event Action<Buffer> OnData {
			[InlineCode("{this}.addListener('data', {value}')")] add {}
			[InlineCode("{this}.removeListener('data', {value}')")] remove {}
		}

		[InlineCode("{this}.once('data', {callback})")]
		public void OnceData(Action<Buffer> callback) {}


		public event Action<string> OnEncodedData {
			[InlineCode("{this}.addListener('data', {value}')")] add {}
			[InlineCode("{this}.removeListener('data', {value}')")] remove {}
		}

		[InlineCode("{this}.once('data', {callback})")]
		public void OnceEncodedData(Action<string> callback) {}


		public event Action OnEnd {
			[InlineCode("{this}.addListener('end', {value}')")] add {}
			[InlineCode("{this}.removeListener('end', {value}')")] remove {}
		}

		[InlineCode("{this}.once('end', {callback})")]
		public void OnceEnd(Action callback) {}


		public event Action<Error> OnError {
			[InlineCode("{this}.addListener('error', {value}')")] add {}
			[InlineCode("{this}.removeListener('error', {value}')")] remove {}
		}

		[InlineCode("{this}.once('error', {callback})")]
		public void OnceError(Action<Error> callback) {}


		public event Action OnClose {
			[InlineCode("{this}.addListener('close', {value}')")] add {}
			[InlineCode("{this}.removeListener('close', {value}')")] remove {}
		}

		[InlineCode("{this}.once('close', {callback})")]
		public void OnceClose(Action callback) {}
	}
}
