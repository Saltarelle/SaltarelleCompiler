using System.Runtime.CompilerServices;

namespace NodeJS.CryptoModule {
	[Imported]
	[ModuleName("crypto")]
	[IgnoreNamespace]
	public class Signer {
		private Signer() {}

		public string Update(object data) { return null; }

		public string Sign(string privateKey) { return null; }
		public string Sign(string privateKey, Encoding encoding) { return null; }
	}
}