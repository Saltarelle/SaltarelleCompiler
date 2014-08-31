using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionData {
		private SyntaxNode _definitionNode;
		public SyntaxNode DefinitionNode {
			get { return _definitionNode; }
			set {
				if (_frozen)
					throw new InvalidOperationException("Frozen");
				_definitionNode = value;
			}
		}

		private bool _directlyUsesThis;
		public bool DirectlyUsesThis {
			get { return _directlyUsesThis; }
			set {
				if (_frozen)
					throw new InvalidOperationException("Frozen");
				_directlyUsesThis = value;
			}
		}

		private bool _frozen;

		public ISet<ISymbol> DirectlyUsedVariables { get; private set; }
		public ISet<ISymbol> DirectlyDeclaredVariables { get; private set; }
		public IList<NestedFunctionData> NestedFunctions { get; private set; }
		public NestedFunctionData Parent { get; private set; }

		public NestedFunctionData(NestedFunctionData parent) {
			Parent                    = parent;
			DirectlyUsedVariables     = new HashSet<ISymbol>();
			DirectlyDeclaredVariables = new HashSet<ISymbol>();
			NestedFunctions           = new List<NestedFunctionData>();
		}

		public IEnumerable<NestedFunctionData> AllParents {
			get {
				for (var p = Parent; p != null; p = p.Parent)
					yield return p;
			}
		}

		public IEnumerable<ISymbol> DirectlyOrIndirectlyUsedVariables {
			get {
				return DirectlyUsedVariables.Concat(NestedFunctions.SelectMany(f => f.DirectlyOrIndirectlyUsedVariables)).Distinct();
			}
		}

		public bool DirectlyOrIndirectlyUsesThis {
			get {
				return DirectlyUsesThis || NestedFunctions.Any(f => f.DirectlyOrIndirectlyUsesThis);
			}
		}

		public IEnumerable<NestedFunctionData> DirectlyOrIndirectlyNestedFunctions {
			get {
				return NestedFunctions.SelectMany(f => new[] { f }.Concat(f.DirectlyOrIndirectlyNestedFunctions));
			}
		}

		public void Freeze() {
			_frozen = true;
			DirectlyUsedVariables     = new ReadOnlySet<ISymbol>(DirectlyUsedVariables);
			DirectlyDeclaredVariables = new ReadOnlySet<ISymbol>(DirectlyDeclaredVariables);
			NestedFunctions = NestedFunctions.AsReadOnly();
		}
	}
}
