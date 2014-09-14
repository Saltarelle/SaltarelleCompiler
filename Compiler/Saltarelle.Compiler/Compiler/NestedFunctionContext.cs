using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionContext {
		public ReadOnlySet<ISymbol> CapturedByRefVariables { get; private set; }

		public NestedFunctionContext(IEnumerable<ISymbol> capturedByRefVariables) {
			var crv = new HashSet<ISymbol>();
			foreach (var v in capturedByRefVariables)
				crv.Add(v);

			CapturedByRefVariables = new ReadOnlySet<ISymbol>(crv);
		}
	}
}