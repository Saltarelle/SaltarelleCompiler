using System;
using System.Runtime.CompilerServices;

namespace NodeJS.TlsModule {
	[Imported]
	[Serializable]
	public class CredentialsContext {
		public string Key { get; set; }
		public string Cert { get; set; }
		[ScriptName("ca")]
		public string CA { get; set; }
	}
}