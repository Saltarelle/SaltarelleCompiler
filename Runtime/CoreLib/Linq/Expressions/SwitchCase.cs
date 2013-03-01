using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class SwitchCase {
		public ReadOnlyCollection<Expression> TestValues { get; private set; }
		public Expression Body { get; private set; }

		internal SwitchCase() {}
	}
}
