using System.Runtime.CompilerServices;

namespace NodeJS.CryptoModule {
	[Imported]
	[ModuleName("crypto")]
	[ScriptName("Verify")]
	[IgnoreNamespace]
	public class Verifier {
		private Verifier() {}

		public string Update(object data) { return null; }

		public bool Verify(string @object, string signature) { return false; }
		public bool Verify(string @object, string signature, Encoding signatureFormat) { return false; }
	}
}