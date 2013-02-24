using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public abstract class MemberBinding {
		public MemberBindingType BindingType { get; private set; }
		public MemberInfo Member { get; private set; }

		internal MemberBinding() {}
	}
}