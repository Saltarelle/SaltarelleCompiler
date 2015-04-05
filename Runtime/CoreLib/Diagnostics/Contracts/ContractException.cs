using System.Runtime.CompilerServices;

namespace System.Diagnostics.Contracts {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public sealed class ContractException : Exception {
		public ContractFailureKind Kind { get { return default(ContractFailureKind); } }
		public string Failure { get { return null; } }
		public string UserMessage { get { return null; } }
		public string Condition { get { return null; } }

		public ContractException(ContractFailureKind kind, string failure, string userMessage, string condition, Exception innerException) { }
	}
}