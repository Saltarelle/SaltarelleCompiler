using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported(TypeCheckCode = "{this}.type === 1 || {this}.type === 8")]
	[Serializable]
	public class MethodBase : MemberInfo {
		[ScriptName("params")]
		public Type[] ParameterTypes { get; private set; }
		public bool IsConstructor { [InlineCode("{this}.type === 1")] get; [InlineCode("0")] private set; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MethodBase GetMethodFromHandle(RuntimeMethodHandle h) {
			return null;
		}

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static MethodBase GetMethodFromHandle(RuntimeMethodHandle h, RuntimeTypeHandle x) {
			return null;
		}
	}
}