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
        private Dictionary<IVariable, VariableData> _result;
		private HashSet<IVariable> _variablesDeclaredInsideLoop;

		private bool _isInsideNestedFunction;
		private bool _isInsideLoop;

        public VariableGatherer(CSharpAstResolver resolver, INamingConventionResolver namingConvention, IErrorReporter errorReporter) {
            _resolver = resolver;
            _namingConvention = namingConvention;
            _errorReporter = errorReporter;
        }

        public IDictionary<IVariable, VariableData> GatherVariables(AstNode node, IMethod method, ISet<string> usedNames) {
            _result = new Dictionary<IVariable, VariableData>();
            _usedNames = new HashSet<string>(usedNames);
			_isInsideNestedFunction = false;
			_isInsideNestedFunction = false;
			_variablesDeclaredInsideLoop = new HashSet<IVariable>();

			foreach (var p in method.Parameters) {
				AddVariable(p, p.IsRef || p.IsOut);
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

		private void AddVariable(IVariable v, bool isUsedByReference = false) {
    		string n = _namingConvention.GetVariableName(v, _usedNames);
    		_usedNames.Add(n);
    		_result.Add(v, new VariableData(n, isUsedByReference));
			if (_isInsideLoop)
				_variablesDeclaredInsideLoop.Add(v);
		}

    	public override object VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement, object data) {
            foreach (var varNode in variableDeclarationStatement.Variables) {
				AddVariable(varNode, varNode.Name);
            }

            return base.VisitVariableDeclarationStatement(variableDeclarationStatement, data);
        }

		public override object VisitForeachStatement(ForeachStatement foreachStatement, object data) {
			AddVariable(foreachStatement.VariableNameToken, foreachStatement.VariableName);

			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				return base.VisitForeachStatement(foreachStatement, data);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override object VisitCatchClause(CatchClause catchClause, object data) {
			if (!catchClause.VariableNameToken.IsNull)
				AddVariable(catchClause.VariableNameToken, catchClause.VariableName);
			return base.VisitCatchClause(catchClause, data);
		}

		public override object VisitLambdaExpression(LambdaExpression lambdaExpression, object data) {
			bool oldIsInsideNestedFunction = _isInsideNestedFunction;
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideNestedFunction = true;
				_isInsideLoop = false;

				foreach (var p in lambdaExpression.Parameters)
					AddVariable(p, p.Name);

				return base.VisitLambdaExpression(lambdaExpression, data);
			}
			finally {
				_isInsideNestedFunction = oldIsInsideNestedFunction;
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override object VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression, object data) {
			bool oldIsInsideNestedFunction = _isInsideNestedFunction;
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideNestedFunction = true;
				_isInsideLoop = false;

				foreach (var p in anonymousMethodExpression.Parameters)
					AddVariable(p, p.Name);

				return base.VisitAnonymousMethodExpression(anonymousMethodExpression, data);
			}
			finally {
				_isInsideNestedFunction = oldIsInsideNestedFunction;
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		private void CheckByRefArguments(IEnumerable<AstNode> arguments) {
			foreach (var a in arguments) {
				if (a is DirectionExpression) {
					var resolveResult = _resolver.Resolve(((DirectionExpression)a).Expression);
					if (resolveResult is LocalResolveResult) {
						var v = ((LocalResolveResult)resolveResult).Variable;
						var current = _result[v];
						if (!current.UseByRefSemantics)
							_result[v] = new VariableData(current.Name, true);
					}
					else {
						_errorReporter.Error("Implementation limitation: only locals can be passed by reference");
					}
				}
			}
		}

		public override object VisitInvocationExpression(InvocationExpression invocationExpression, object data) {
			CheckByRefArguments(invocationExpression.Arguments);
			return base.VisitInvocationExpression(invocationExpression, data);
		}

		public override object VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression, object data) {
			CheckByRefArguments( objectCreateExpression.Arguments);
			return base.VisitObjectCreateExpression(objectCreateExpression, data);
		}

		public override object VisitForStatement(ForStatement forStatement, object data) {
			foreach (var s in forStatement.Initializers)
				s.AcceptVisitor(this);
			forStatement.Condition.AcceptVisitor(this);
			foreach (var s in forStatement.Iterators)
				s.AcceptVisitor(this);

			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				forStatement.EmbeddedStatement.AcceptVisitor(this);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
			return null;
		}

		public override object VisitWhileStatement(WhileStatement whileStatement, object data) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				return base.VisitWhileStatement(whileStatement, data);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override object VisitDoWhileStatement(DoWhileStatement doWhileStatement, object data) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				return base.VisitDoWhileStatement(doWhileStatement, data);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override object VisitIdentifierExpression(IdentifierExpression identifierExpression, object data) {
			if (_isInsideNestedFunction) {
				var rr = _resolver.Resolve(identifierExpression) as LocalResolveResult;
				if (rr != null && _variablesDeclaredInsideLoop.Contains(rr.Variable)) {
					// the variable might suffer from all variables in JS being function-scoped, so use byref semantics.
					var current = _result[rr.Variable];
					if (!current.UseByRefSemantics)
						_result[rr.Variable] = new VariableData(current.Name, true);
				}
			}
			return base.VisitIdentifierExpression(identifierExpression, data);
		}
    }
}
