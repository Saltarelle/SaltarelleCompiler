using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Reflection {
#warning TODO
	[Imported]
	[Serializable]
	public class PropertyInfo : MemberInfo {
		public Type PropertyType { get; private set; }
		public Type[] IndexParameterTypes { [InlineCode("{this}.params || []")] get; [InlineCode("0")] private set; }

		// TODO all below
		public bool CanRead { [InlineCode("!!{this}.getter")] get; [InlineCode("0")] private set; }
		public bool CanWrite { [InlineCode("!!{this}.setter")]get; [InlineCode("0")] private set; }

		[ScriptName("getter")]
		public MethodInfo GetMethod { get; private set; }
		[ScriptName("setter")]
		public MethodInfo SetMethod { get; private set; }

		public object GetValue(object obj) { return null; }
		public object GetValue(object obj, object[] index) { return null; }

		public void SetValue(object obj, object value) {}
		public void SetValue(object obj, object value, object[] index) {}
	}
}