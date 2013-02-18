using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported]
	[Serializable]
	public class EventInfo : MemberInfo {
		[ScriptName("adder")]
		public MethodInfo AddMethod { get; private set; }
		[ScriptName("remover")]
		public MethodInfo RemoveMethod { get; private set; }

		[InlineCode("{$System.Script}.midel({this}.adder, {target})({handler})")]
		public void AddEventHandler(object target, Delegate handler) {}

		[InlineCode("{$System.Script}.midel({this}.remover, {target})({handler})")]
		public void RemoveEventHandler(object target, Delegate handler) {}
	}
}