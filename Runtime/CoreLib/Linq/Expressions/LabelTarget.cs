using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class LabelTarget {
		public string Name { get { return null; } }
		public Type Type { get { return null; } }

		internal LabelTarget() {}
	}
}