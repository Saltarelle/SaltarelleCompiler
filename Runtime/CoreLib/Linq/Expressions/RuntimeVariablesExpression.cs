using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions {
	[Imported, Serializable]
	public sealed class RuntimeVariablesExpression : Expression {
		public ReadOnlyCollection<ParameterExpression> Variables { get; private set; }

		public RuntimeVariablesExpression Update(IEnumerable<ParameterExpression> variables) { return null; }

		internal RuntimeVariablesExpression() {}
	}
}