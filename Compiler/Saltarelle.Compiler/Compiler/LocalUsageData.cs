using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
	public class LocalUsageData {
		public bool DirectlyOrIndirectlyUsesThis { get; private set; }
		public ISet<ISymbol> DirectlyOrIndirectlyUsedLocals { get; private set; }

		public LocalUsageData(bool directlyOrIndirectlyUsesThis, ISet<ISymbol> directlyOrIndirectlyUsedVariables) {
			DirectlyOrIndirectlyUsesThis = directlyOrIndirectlyUsesThis;
			DirectlyOrIndirectlyUsedLocals = new ReadOnlySet<ISymbol>(directlyOrIndirectlyUsedVariables);
		}
	}
}
