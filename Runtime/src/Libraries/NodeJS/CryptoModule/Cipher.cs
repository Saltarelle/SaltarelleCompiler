using System.Runtime.CompilerServices;

namespace NodeJS.CryptoModule {
	[Imported]
	[ModuleName("crypto")]
	[IgnoreNamespace]
	public class Cipher {
		private Cipher() {}

		public string Update(string data) { return null; }
		public string Update(string data, Encoding inputEncoding) { return null; }
		public string Update(string data, Encoding inputEncoding, Encoding outputEncoding) { return null; }

		public string Final() { return null; }
		public string Final(Encoding encoding) { return null; }

		public void SetAutoPadding() {}
		public void SetAutoPadding(bool autoPadding) {}
	}
}