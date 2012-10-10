using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.BufferModule;

namespace NodeJS.HttpsModule {
	[Imported]
	[Serializable]
	public class RequestOptions : HttpModule.RequestOptions {
		public TypeOption<Buffer, string> Pfx { get; set; }

		public TypeOption<Buffer, string> Key { get; set; }

		public string Passphrase { get; set; }

		public TypeOption<Buffer, string> Cert { get; set; }

		[ScriptName("ca")]
		public TypeOption<Buffer, string>[] CA { get; set; }

		public string Ciphers { get; set; }

		public bool RejectUnauthorized { get; set; }

	}
}
