using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;
using NodeJS.EventsModule;
using NodeJS.NetModule;

namespace NodeJS.TlsModule {
	[Imported]
	[ModuleName("tls")]
	[IgnoreNamespace]
	public class SecurePair : EventEmitter {
		public SecurePair() {}

		[IntrinsicProperty]
		public CleartextStream Cleartext { get; set; }


		public event Action OnSecure {
			[InlineCode("{this}.addListener('secure', {value})")] add {}
			[InlineCode("{this}.removeListener('secure', {value})")] remove {}
		}

		[InlineCode("{this}.once('secure', {callback})")]
		public void OnceSecure(Action callback) {}
	}
}
