using System;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;
using NodeJS.CryptoModule;

namespace NodeJS.TlsModule {
	[Imported]
	[Serializable]
	public class CreateServerOptions {
		public TypeOption<Buffer, string> Pfx { get; set; }

		public TypeOption<Buffer, string> Key { get; set; }

		public string Passphrase { get; set; }

		public TypeOption<Buffer, string> Cert { get; set; }

		[ScriptName("ca")]
		public TypeOption<Buffer, string>[] CA { get; set; }

		public TypeOption<string, string[]> Crl { get; set; }

		public string Ciphers { get; set; }

		public bool? HonorCipherOrder { get; set; }

		public bool? RequestCert { get; set; }

		public bool? RejectUnauthorized { get; set; }

		[PreserveCase]
		public TypeOption<string[], Buffer> NPNProtocols { get; set; }

		[PreserveCase]
		public Func<string, SecureContext> SNICallback { get; set; }

		public string SessionIdContext { get; set; }

	}
}