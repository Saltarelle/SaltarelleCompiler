using System;
using System.Runtime.CompilerServices;

namespace NodeJS.TlsModule {
	[Imported]
	[Serializable]
	public class CipherNameAndVersion {
		public string Name { get; set; }
		public string Version { get; set; }
	}
}