using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported(TypeCheckCode = "{this}.type === 1")]
	[Serializable]
	public class ConstructorInfo : MethodBase {
		[InlineCode("{$System.Script}.invokeCI({this}, {arguments})")]
		public object Invoke(params object[] arguments) { return null; }
	}
}