using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.JSModel;

namespace Saltarelle.Compiler {
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
        public IList<NestedFunctionData> NestedFunctions { get; private set; }

		public NestedFunctionData() {
            DirectlyUsedVariables = new HashSet<IVariable>();
            NestedFunctions       = new List<NestedFunctionData>();
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

        public IEnumerable<NestedFunctionData> SelfAndDirectlyOrIndirectlyNestedFunctions {
            get {
                return new[] { this }.Concat(NestedFunctions.SelectMany(f => f.SelfAndDirectlyOrIndirectlyNestedFunctions));
            }
        }

        public void Freeze() {
            _frozen = true;
            DirectlyUsedVariables = new ReadOnlySet<IVariable>(DirectlyUsedVariables);
            NestedFunctions = NestedFunctions.AsReadOnly();
        }
	}
}
