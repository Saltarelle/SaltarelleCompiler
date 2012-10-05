using System;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;

namespace NodeJS.CryptoModule {
	[Imported]
	[Serializable]
	public class CredentialsDetails {
		public TypeOption<Buffer, string> Pfx { get; set; }

		public TypeOption<Buffer, string> Key { get; set; }

		public string Passphrase { get; set; }

		public TypeOption<Buffer, string> Cert { get; set; }

		[ScriptName("ca")]
		public TypeOption<Buffer, string>[] CA { get; set; }

		public TypeOption<string, string[]> Crl { get; set; }

		public string Ciphers { get; set; }
	}
}