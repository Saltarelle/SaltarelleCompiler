using System.Runtime.CompilerServices;

namespace NodeJS.CryptoModule {
	[Imported]
	[ModuleName("crypto")]
	[IgnoreNamespace]
	public class Hmac {
		private Hmac() {}

		public void Update(string data) {}

		public string Digest() { return null; }
		public string Digest(Encoding encoding) { return null; }
	}
}