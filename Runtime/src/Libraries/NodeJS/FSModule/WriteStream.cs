using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NodeJS.FSModule {
	[Imported]
	[ModuleName("fs")]
	[IgnoreNamespace]
	public class WriteStream : WritableStream {
		private WriteStream() {}

		[IntrinsicProperty]
		public int BytesWritten { get; private set; }
		
		public event Action<int> OnOpen {
			[InlineCode("{this}.addListener('open', {value})")] add {}
			[InlineCode("{this}.removeListener('open', {value})")] remove {}
		}

		[InlineCode("{this}.once('open', {callback})")]
		public void OnceOpen(Action<int> callback) {}
	}
}