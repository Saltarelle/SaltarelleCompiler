using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Reflection {
	[Imported(TypeCheckCode = "{this}.type === 16")]
	[Serializable]
	public class PropertyInfo : MemberInfo {
		[ScriptName("returnType")]
		public Type PropertyType { get; private set; }
		public Type[] IndexParameterTypes { [InlineCode("{this}.params || []")] get; [InlineCode("0")] private set; }

		public bool CanRead  { [InlineCode("!!{this}.getter")] get; [InlineCode("0")] private set; }
		public bool CanWrite { [InlineCode("!!{this}.setter")] get; [InlineCode("0")] private set; }

		[ScriptName("getter")]
		public MethodInfo GetMethod { get; private set; }
		[ScriptName("setter")]
		public MethodInfo SetMethod { get; private set; }

		[InlineCode("{$System.Script}.midel({this}.getter, {obj})()")]
		public object GetValue(object obj) { return null; }
		[InlineCode("{$System.Script}.midel({this}.getter, {obj}).apply(null, {index})")]
		public object GetValue(object obj, object[] index) { return null; }

		[InlineCode("{$System.Script}.midel({this}.setter, {obj})({value})")]
		public void SetValue(object obj, object value) {}
		[InlineCode("{$System.Script}.midel({this}.setter, {obj}).apply(null, {index}.concat({value}))")]
		public void SetValue(object obj, object value, object[] index) {}

		/// <summary>
		/// For properties implemented as fields, contains the name of the field. Null for properties implemented as get and set methods.
		/// </summary>
		[ScriptName("fname")]
		public string ScriptFieldName { get; private set; }
	}
}