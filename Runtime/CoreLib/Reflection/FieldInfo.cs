using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported]
	[Serializable]
	public class FieldInfo : MemberInfo {
		public Type FieldType { get; private set; }
		public bool IsStatic { [InlineCode("{this}.isStatic || false")] get; [InlineCode("0")] private set; }

		[InlineCode("{$System.Script}.fieldAccess({this}, {obj})")]
		public object GetValue(object obj) { return null; }
		[InlineCode("{$System.Script}.fieldAccess({this}, {obj}, {value})")]
		public void SetValue(object obj, object value) {}

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle h) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle h, RuntimeTypeHandle x) { return null; }
	}
}