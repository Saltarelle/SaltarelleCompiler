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
	public class CleartextStream : ReadWriteStream {
		public CleartextStream() {}

		[IntrinsicProperty]
		public bool Authorized { get; private set; }

		[IntrinsicProperty]
		public Error AuthorizationError { get; private set; }

		public Certificate GetPeerCertificate() { return null; }

		public CipherNameAndVersion GetCipher() { return null; }

		public SocketAddress Address { [ScriptName("address")] get; private set; }

		[IntrinsicProperty]
		public string RemoteAddress { get; private set; }

		[IntrinsicProperty]
		public int RemotePort { get; private set; }


		public event Action OnSecureConnection {
			[InlineCode("{this}.addListener('secureConnect', {value})")] add {}
			[InlineCode("{this}.removeListener('secureConnect', {value})")] remove {}
		}

		[InlineCode("{this}.once('secureConnect', {callback})")]
		public void OnceSecureConnection(Action callback) {}
	}
}
