using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionData {
		public bool DirectlyOrIndirectlyUsesThis { get; private set; }
		public ISet<ISymbol> DirectlyOrIndirectlyUsedVariables { get; private set; }
		public ISet<ISymbol> DirectlyOrIndirectlyDeclaredVariables { get; private set; }

		public NestedFunctionData(bool directlyOrIndirectlyUsesThis, ISet<ISymbol> directlyOrIndirectlyUsedVariables, ISet<ISymbol> directlyOrIndirectlyDeclaredVariables) {
			DirectlyOrIndirectlyUsesThis = directlyOrIndirectlyUsesThis;
			DirectlyOrIndirectlyUsedVariables = new ReadOnlySet<ISymbol>(directlyOrIndirectlyUsedVariables);
			DirectlyOrIndirectlyDeclaredVariables = new ReadOnlySet<ISymbol>(directlyOrIndirectlyDeclaredVariables);
		}
	}
}
