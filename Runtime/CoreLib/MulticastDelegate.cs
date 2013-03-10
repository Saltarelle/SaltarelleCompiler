using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[IgnoreNamespace]
	[ScriptName("Function")]
	public abstract class MulticastDelegate : Delegate {
		protected MulticastDelegate(object target, string method) : base(target, method) {
		}

		protected MulticastDelegate(Type target, string method) : base(target, method) {
		}

		[InlineCode("{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator==(MulticastDelegate a, MulticastDelegate b) { return false; }

		[InlineCode("!{$System.Script}.staticEquals({a}, {b})")]
		public static bool operator!=(MulticastDelegate a, MulticastDelegate b) { return false; }

		[InlineCode("{$System.Script}.getInvocationList({this})")]
		public Delegate[] GetInvocationList() {
			return null;
		}
	}
}