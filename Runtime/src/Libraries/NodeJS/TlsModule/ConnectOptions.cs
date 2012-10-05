using System;
using System.Runtime.CompilerServices;
using NodeJS.BufferModule;
using NodeJS.CryptoModule;
using NodeJS.NetModule;

namespace NodeJS.TlsModule {
	[Imported]
	[Serializable]
	public class ConnectOptions {
		public string Host { get; set; }

		public int? Port { get; set; }

		public Socket Socket { get; set; }

		public TypeOption<Buffer, string> Pfx { get; set; }

		public TypeOption<Buffer, string> Key { get; set; }

		public string Passphrase { get; set; }

		public TypeOption<Buffer, string> Cert { get; set; }

		[ScriptName("ca")]
		public TypeOption<Buffer, string>[] CA { get; set; }

		public bool? RejectUnauthorized { get; set; }

		[PreserveCase]
		public TypeOption<object[], Buffer> NPNProtocols { get; set; }

		public string Servername { get; set; }
	}
}