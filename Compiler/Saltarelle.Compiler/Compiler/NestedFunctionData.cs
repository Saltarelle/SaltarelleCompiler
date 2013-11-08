using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.ExtensionMethods;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionData {
		private AstNode _definitionNode;
		public AstNode DefinitionNode {
			get { return _definitionNode; }
			set {
				if (_frozen)
					throw new InvalidOperationException("Frozen");
				_definitionNode = value;
			}
		}

		private AstNode _bodyNode;
		public AstNode BodyNode {
			get { return _bodyNode; }
			set {
				if (_frozen)
					throw new InvalidOperationException("Frozen");
				_bodyNode = value;
			}
		}

		private LambdaResolveResult _resolveResult;
		public LambdaResolveResult ResolveResult {
			get { return _resolveResult; }
			set {
				if (_frozen)
					throw new InvalidOperationException("Frozen");
				_resolveResult = value;
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

		public ISet<IVariable> DirectlyUsedVariables { get; private set; }
		public ISet<IVariable> DirectlyDeclaredVariables { get; private set; }
		public IList<NestedFunctionData> NestedFunctions { get; private set; }
		public NestedFunctionData Parent { get; private set; }

		public NestedFunctionData(NestedFunctionData parent) {
			Parent                    = parent;
			DirectlyUsedVariables     = new HashSet<IVariable>();
			DirectlyDeclaredVariables = new HashSet<IVariable>();
			NestedFunctions           = new List<NestedFunctionData>();
		}

		public IEnumerable<NestedFunctionData> AllParents {
			get {
				for (var p = Parent; p != null; p = p.Parent)
					yield return p;
			}
		}

		public IEnumerable<IVariable> DirectlyOrIndirectlyUsedVariables {
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
			DirectlyUsedVariables     = new ReadOnlySet<IVariable>(DirectlyUsedVariables);
			DirectlyDeclaredVariables = new ReadOnlySet<IVariable>(DirectlyDeclaredVariables);
			NestedFunctions = NestedFunctions.AsReadOnly();
		}
	}
}
