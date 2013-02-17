using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public abstract class Attribute {
		protected Attribute() {
		}
	}
}