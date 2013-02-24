using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class ElementInit {
		public MethodInfo AddMethod { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		internal ElementInit() {}
	}
}