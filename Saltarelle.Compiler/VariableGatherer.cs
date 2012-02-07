using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler
{
    public class VariableGatherer : DepthFirstAstVisitor<object, object> {
        private readonly CSharpAstResolver _resolver;
        private readonly INamingConventionResolver _namingConvention;
        private readonly IErrorReporter _errorReporter;
        private HashSet<string> _usedNames;
        private Dictionary<IVariable, string> _result;

        public VariableGatherer(CSharpAstResolver resolver, INamingConventionResolver namingConvention, IErrorReporter errorReporter) {
            _resolver = resolver;
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
        }

        public IDictionary<IVariable, string> GatherVariables(AstNode node, IMethod method, ISet<string> usedNames) {
            _result = new Dictionary<IVariable, string>();
            _usedNames = new HashSet<string>(usedNames);

			foreach (var p in method.Parameters) {
				AddVariable(p);
			}

            node.AcceptVisitor(this);
            return _result;
        }

    	private void AddVariable(AstNode variableNode, string variableName) {
			var resolveResult = _resolver.Resolve(variableNode);
            if (!(resolveResult is LocalResolveResult)) {
                _errorReporter.Error("Variable " + variableName + " does not resolve to a local (resolves to " + (resolveResult != null ? resolveResult.ToString() : "null") + ")");
                return;
            }
			AddVariable(((LocalResolveResult)resolveResult).Variable);
    	}

		private void AddVariable(IVariable v) {
    		string n = _namingConvention.GetVariableName(v, _usedNames);
    		_usedNames.Add(n);
    		_result.Add(v, n);
		}

    	public override object VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement, object data) {
            foreach (var varNode in variableDeclarationStatement.Variables) {
				AddVariable(varNode, varNode.Name);
            }

            return base.VisitVariableDeclarationStatement(variableDeclarationStatement, data);
        }

		public override object VisitForeachStatement(ForeachStatement foreachStatement, object data) {
			AddVariable(foreachStatement.VariableNameToken, foreachStatement.VariableName);
			return base.VisitForeachStatement(foreachStatement, data);
		}

		public override object VisitCatchClause(CatchClause catchClause, object data) {
			if (!catchClause.VariableNameToken.IsNull)
				AddVariable(catchClause.VariableNameToken, catchClause.VariableName);
			return base.VisitCatchClause(catchClause, data);
		}
    }
}
