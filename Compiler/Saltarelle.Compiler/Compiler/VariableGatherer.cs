using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.Compiler {
    public class VariableGatherer : DepthFirstAstVisitor {
        private readonly CSharpAstResolver _resolver;
        private readonly INamer _namer;
        private readonly IErrorReporter _errorReporter;
        private HashSet<string> _usedNames;
        private Dictionary<IVariable, VariableData> _result;
		private HashSet<IVariable> _variablesDeclaredInsideLoop;

        private AstNode _currentMethod;
		private bool _isInsideLoop;

        public VariableGatherer(CSharpAstResolver resolver, INamer namer, IErrorReporter errorReporter) {
            _resolver = resolver;
            _namer = namer;
            _errorReporter = errorReporter;
        }

        public Tuple<IDictionary<IVariable, VariableData>, ISet<string>> GatherVariables(AstNode node, IMethod method, ISet<string> usedNames) {
            _result = new Dictionary<IVariable, VariableData>();
            _usedNames = new HashSet<string>(usedNames);
            _currentMethod = node;
			_variablesDeclaredInsideLoop = new HashSet<IVariable>();

			if (method != null) {
				foreach (var p in method.Parameters) {
					AddVariable(p, p.IsRef || p.IsOut);
				}
			}

            node.AcceptVisitor(this);
            return Tuple.Create((IDictionary<IVariable, VariableData>)_result, (ISet<string>)_usedNames);
        }

    	private void AddVariable(AstNode variableNode, string variableName) {
			var resolveResult = _resolver.Resolve(variableNode);
            if (!(resolveResult is LocalResolveResult)) {
                _errorReporter.InternalError("Variable " + variableName + " does not resolve to a local (resolves to " + (resolveResult != null ? resolveResult.ToString() : "null") + ")");
                return;
            }
			AddVariable(((LocalResolveResult)resolveResult).Variable);
    	}

		private void AddVariable(IVariable v, bool isUsedByReference = false) {
    		string n = _namer.GetVariableName(v.Name, _usedNames);
    		_usedNames.Add(n);
    		_result.Add(v, new VariableData(n, _currentMethod, isUsedByReference));
			if (_isInsideLoop)
				_variablesDeclaredInsideLoop.Add(v);
		}

    	public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement) {
            foreach (var varNode in variableDeclarationStatement.Variables) {
				AddVariable(varNode, varNode.Name);
            }

            base.VisitVariableDeclarationStatement(variableDeclarationStatement);
        }

		public override void VisitForeachStatement(ForeachStatement foreachStatement) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				AddVariable(foreachStatement.VariableNameToken, foreachStatement.VariableName);
				base.VisitForeachStatement(foreachStatement);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitCatchClause(CatchClause catchClause) {
			if (!catchClause.VariableNameToken.IsNull)
				AddVariable(catchClause.VariableNameToken, catchClause.VariableName);
			base.VisitCatchClause(catchClause);
		}

		public override void VisitLambdaExpression(LambdaExpression lambdaExpression) {
            AstNode oldMethod = _currentMethod;
			bool oldIsInsideLoop = _isInsideLoop;
			try {
                _currentMethod = lambdaExpression;
				_isInsideLoop = false;

				foreach (var p in lambdaExpression.Parameters)
					AddVariable(p, p.Name);

				base.VisitLambdaExpression(lambdaExpression);
			}
			finally {
                _currentMethod = oldMethod;
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression) {
            AstNode oldMethod = _currentMethod;
			bool oldIsInsideLoop = _isInsideLoop;
			try {
                _currentMethod = anonymousMethodExpression;
				_isInsideLoop = false;

				foreach (var p in anonymousMethodExpression.Parameters)
					AddVariable(p, p.Name);

				base.VisitAnonymousMethodExpression(anonymousMethodExpression);
			}
			finally {
                _currentMethod = oldMethod;
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		private void CheckByRefArguments(IEnumerable<AstNode> arguments) {
			foreach (var a in arguments) {
				var actual = (a is NamedArgumentExpression ? ((NamedArgumentExpression)a).Expression : a);
				if (actual is DirectionExpression) {
					var resolveResult = _resolver.Resolve(((DirectionExpression)actual).Expression);
					if (resolveResult is LocalResolveResult) {
						var v = ((LocalResolveResult)resolveResult).Variable;
						var current = _result[v];
						if (!current.UseByRefSemantics)
							_result[v] = new VariableData(current.Name, current.DeclaringMethod, true);
					}
				}
			}
		}

		public override void VisitInvocationExpression(InvocationExpression invocationExpression) {
			CheckByRefArguments(invocationExpression.Arguments);
			base.VisitInvocationExpression(invocationExpression);
		}

		public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression) {
			CheckByRefArguments( objectCreateExpression.Arguments);
			base.VisitObjectCreateExpression(objectCreateExpression);
		}

		public override void VisitForStatement(ForStatement forStatement) {
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
		}

		public override void VisitWhileStatement(WhileStatement whileStatement) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				base.VisitWhileStatement(whileStatement);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitDoWhileStatement(DoWhileStatement doWhileStatement) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				base.VisitDoWhileStatement(doWhileStatement);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitIdentifierExpression(IdentifierExpression identifierExpression) {
			var rr = _resolver.Resolve(identifierExpression) as LocalResolveResult;
			if (rr != null && _variablesDeclaredInsideLoop.Contains(rr.Variable) && _currentMethod != _result[rr.Variable].DeclaringMethod) {
				// the variable might suffer from all variables in JS being function-scoped, so use byref semantics.
				var current = _result[rr.Variable];
				if (!current.UseByRefSemantics)
					_result[rr.Variable] = new VariableData(current.Name, current.DeclaringMethod, true);
			}
			base.VisitIdentifierExpression(identifierExpression);
		}
    }
}
