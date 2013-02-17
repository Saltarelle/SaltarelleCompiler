using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported]
	[Serializable]
	public class MethodInfo : MethodBase {
		public Type ReturnType { get; private set; }
		[ScriptName("params")]
		public Type[] ParameterTypes { get; private set; }
		public bool IsConstructor { [InlineCode("{this}.type === 1")] get; [InlineCode("0")] private set; }

		[InlineCode("{$System.Script}.midel({this})")]
		public Delegate CreateDelegate(Type delegateType) { return null; }
		[InlineCode("{$System.Script}.midel({this}, {target})")]
		public Delegate CreateDelegate(Type delegateType, object target) { return null; }

		[InlineCode("{$System.Script}.midel({this})")]
		public Delegate CreateDelegate() { return null; }
		[InlineCode("{$System.Script}.midel({this}, {target})")]
		public Delegate CreateDelegate(object target) { return null; }

		[InlineCode("{$System.Script}.midel({this}, null, {typeArguments})")]
		public Delegate CreateDelegate(Type[] typeArguments) { return null; }
		[InlineCode("{$System.Script}.midel({this}, {target}, {typeArguments})")]
		public Delegate CreateDelegate(object target, Type[] typeArguments) { return null; }

		public int TypeParameterCount { [InlineCode("{this}.tpcount || 0")] get; [InlineCode("X")] private set; }
		public bool IsGenericMethodDefinition { [InlineCode("!!{this}.tpcount")] get; [InlineCode("X")] private set; }

		[InlineCode("{$System.Script}.midel({this}, {obj})({*arguments})")]
		public object Invoke(object obj, params object[] arguments) { return null; }
		[InlineCode("{$System.Script}.midel({this}, {obj}, {typeArguments})({*arguments})")]
		public object Invoke(object obj, Type[] typeArguments, params object[] arguments) { return null; }
	}
}