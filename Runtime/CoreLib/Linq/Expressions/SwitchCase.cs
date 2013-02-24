using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class SwitchCase {
		public ReadOnlyCollection<Expression> TestValues { get; private set; }
		public Expression Body { get; private set; }

		public SwitchCase Update(IEnumerable<Expression> testValues, Expression body) { return null; }

		internal SwitchCase() {}
	}
}