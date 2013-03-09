using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported(TypeCheckCode = "{this}.type === 8")]
	[Serializable]
	public class MethodInfo : MethodBase {
		public Type ReturnType { get; private set; }

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

		/// <summary>
		/// Script name of the method. Null if the method has a special implementation.
		/// </summary>
		[ScriptName("sname")]
		public string ScriptName { get; private set;}

		/// <summary>
		/// If true, this method should be invoked as a static method with the 'this' reference as the first argument. Note that this property does not affect the Invoke and CreateDelegate methods.
		/// </summary>
		[ScriptName("sm")]
		public bool IsStaticMethodWithThisAsFirstArgument { [InlineCode("{this}.sm || false")] get; [InlineCode("{this}.sm = {value}")] private set; }

		/// <summary>
		/// For methods with a special implementation (eg. [InlineCode]), contains a delegate that represents the method. Null for normal methods.
		/// </summary>
		[ScriptName("def")]
		public Delegate SpecialImplementation { get; private set; }

		internal MethodInfo() {}
	}
}