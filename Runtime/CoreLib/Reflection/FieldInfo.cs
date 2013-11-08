using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported(TypeCheckCode = "{this}.type === 4")]
	[Serializable]
	public class FieldInfo : MemberInfo {
		[ScriptName("returnType")]
		public Type FieldType { get; private set; }

		[InlineCode("{$System.Script}.fieldAccess({this}, {obj})")]
		public object GetValue(object obj) { return null; }
		[InlineCode("{$System.Script}.fieldAccess({this}, {obj}, {value})")]
		public void SetValue(object obj, object value) {}

		/// <summary>
		/// Script name of the field
		/// </summary>
		[ScriptName("sname")]
		public string ScriptName { get; private set; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle h) { return null; }

		[NonScriptable, EditorBrowsable(EditorBrowsableState.Never)]
		public static FieldInfo GetFieldFromHandle(RuntimeFieldHandle h, RuntimeTypeHandle x) { return null; }

		internal FieldInfo() {}
	}
}