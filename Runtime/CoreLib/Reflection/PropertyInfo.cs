using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Reflection {
#warning TODO
	[Imported]
	[Serializable]
	public class PropertyInfo : MemberInfo {
		public Type PropertyType { get; private set; }
		public bool CanRead { get; private set; }
		public bool CanWrite { get; private set; }
		public MethodInfo GetMethod { get; private set; }
		public MethodInfo SetMethod { get; private set; }

		public object GetValue(object obj) { return null; }
		public void SetValue(object obj, object value) {}

		public Type[] IndexParameterTypes { get; private set; }
		public object GetValue(object obj, object[] index) { return null; }
		public void SetValue(object obj, object value, object[] index) {}
	}
}