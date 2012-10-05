using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NodeJS.EventsModule;

namespace NodeJS.FSModule {
	[Imported]
	[ModuleName("fs")]
	[IgnoreNamespace]
	public class FSWatcher : EventEmitter {
		private FSWatcher() {}

		public void Close() {}

		public event Action<WatchEventType, string> OnChange {
			[InlineCode("{this}.addListener('change', {value})")] add {}
			[InlineCode("{this}.removeListener('change', {value})")] remove {}
		}

		[InlineCode("{this}.once('change', {callback})")]
		public void OnceChange(Action<WatchEventType, string> callback) {}


		public event Action<Error> OnError {
			[InlineCode("{this}.addListener('error', {value})")] add {}
			[InlineCode("{this}.removeListener('error', {value})")] remove {}
		}

		[InlineCode("{this}.once('error', {callback})")]
		public void OnceError(Action<Error> callback) {}
	}
}