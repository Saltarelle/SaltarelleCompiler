using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported(TypeCheckCode = "{this}.type === 1")]
	[Serializable]
	public class ConstructorInfo : MethodBase {
		[InlineCode("{$System.Script}.invokeCI({this}, {arguments})")]
		public object Invoke(params object[] arguments) { return null; }

		/// <summary>
		/// Script name of the constructor. Null for the unnamed constructor and for constructors with special implementations
		/// </summary>
		[ScriptName("sname")]
		public string ScriptName { get; private set; }

		/// <summary>
		/// True if the constructor is a normal method that returns the created instance and should be invoked without the 'new' operator
		/// </summary>
		public bool IsStaticMethod { [InlineCode("{this}.sm || false")] get; [InlineCode("{this}.sm = {value}")] private set; }

		/// <summary>
		/// For constructors with a special implementation (eg. [ObjectLiteral] or [InlineCode]), contains a delegate that can be invoked to create an instance.
		/// </summary>
		[ScriptName("def")]
		public Delegate SpecialImplementation { get; private set; }
	}
}