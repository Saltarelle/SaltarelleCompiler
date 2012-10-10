using System.Runtime.CompilerServices;

namespace NodeJS.CryptoModule {
	[Imported]
	[ModuleName("crypto")]
	[IgnoreNamespace]
	public class DiffieHellman {
		private DiffieHellman() {}

		public string GenerateKeys() { return null; }
		public string GenerateKeys(Encoding encoding) { return null; }

		public string ComputeSecret(string otherPublicKey) { return null; }
		public string ComputeSecret(string otherPublicKey, Encoding inputEncoding) { return null; }
		public string ComputeSecret(string otherPublicKey, Encoding inputEncoding, Encoding outputEncoding) { return null; }

		public string GetPrime() { return null; }
		public string GetPrime(Encoding encoding) { return null; }

		public string GetGenerator() { return null; }
		public string GetGenerator(Encoding encoding) { return null; }

		public string GetPublicKey() { return null; }
		public string GetPublicKey(Encoding encoding) { return null; }

		public string GetPrivateKey() { return null; }
		public string GetPrivateKey(Encoding encoding) { return null; }

		public string SetPublicKey(string key) { return null; }
		public string SetPublicKey(string key, Encoding encoding) { return null; }

		public string SetPrivateKey(string key) { return null; }
		public string SetPrivateKey(string key, Encoding encoding) { return null; }
	}
}