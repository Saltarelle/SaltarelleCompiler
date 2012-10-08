using System;
using System.Runtime.CompilerServices;

namespace NodeJS.CryptoModule {
	[Imported]
	[Serializable]
	public class Credentials {
		public SecureContext Context { get; set; }
	}
}