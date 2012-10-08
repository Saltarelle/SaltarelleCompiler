using System.Runtime.CompilerServices;

namespace NodeJS.CryptoModule {
	[Imported]
	[ModuleName("crypto")]
	[IgnoreNamespace]
	public class Hash {
		private Hash() {}

		public void Update(string data) {}
		public void Update(string data, Encoding encoding) {}

		public string Digest() { return null; }
		public string Digest(Encoding encoding) { return null; }
	}
}