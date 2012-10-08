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
	public class Server : NetModule.Server {
		public Server() {}

		public void AddContext(string hostname, CredentialsContext credentials) {}

		public event Action<CleartextStream> OnSecureConnection {
			[InlineCode("{this}.addListener('secureConnection', {value})")] add {}
			[InlineCode("{this}.removeListener('secureConnection', {value})")] remove {}
		}

		[InlineCode("{this}.once('secureConnection', {callback})")]
		public void OnceSecureConnection(Action<CleartextStream> callback) {}


		public event Action<Error> OnClientError {
			[InlineCode("{this}.addListener('clientError', {value})")] add {}
			[InlineCode("{this}.removeListener('clientError', {value})")] remove {}
		}

		[InlineCode("{this}.once('clientError', {callback})")]
		public void OnceClientError(Action<Error> callback) {}
	}
}
