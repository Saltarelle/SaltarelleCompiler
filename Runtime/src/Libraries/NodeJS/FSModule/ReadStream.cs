using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NodeJS.FSModule {
	[Imported]
	[ModuleName("fs")]
	[IgnoreNamespace]
	public class ReadStream : ReadableStream {
		private ReadStream() {}
		
		public event Action<int> OnOpen {
			[InlineCode("{this}.addListener('open', {value})")] add {}
			[InlineCode("{this}.removeListener('open', {value})")] remove {}
		}

		[InlineCode("{this}.once('open', {callback})")]
		public void OnceOpen(Action<int> callback) {}
	}
}