using System.Runtime.CompilerServices;

namespace System {
	[Imported]
	[Serializable]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public abstract class Attribute {
		[ScriptSkip]
		protected Attribute() {
		}
	}
}