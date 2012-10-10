using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.BufferModule;
using NodeJS.EventsModule;

namespace NodeJS.ReplModule {
	[Imported]
	[IgnoreNamespace]
	[ModuleName("repl")]
	[ScriptName("REPLServer")]
	public class ReplServer : EventEmitter {
		private ReplServer() {}


		public event Action OnExit {
			[InlineCode("{this}.addListener('exit', {value})")] add {}
			[InlineCode("{this}.removeListener('exit', {value})")] remove {}
		}

		[InlineCode("{this}.once('exit', {callback})")]
		public void OnceExit(Action callback) {}
	}
}
