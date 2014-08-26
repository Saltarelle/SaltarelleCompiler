using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saltarelle.Compiler.Compiler {
	public class VariableGatherer : CSharpSyntaxWalker {
		private readonly SemanticModel _semanticModel;
		private readonly INamer _namer;
		private readonly IErrorReporter _errorReporter;
		private HashSet<string> _usedNames;
		private Dictionary<ISymbol, VariableData> _result;
		private HashSet<ISymbol> _variablesDeclaredInsideLoop;

		private SyntaxNode _currentMethod;
		private bool _isInsideLoop;

		public VariableGatherer(SemanticModel semanticModel, INamer namer, IErrorReporter errorReporter) {
			_semanticModel = semanticModel;
			_namer = namer;
			_errorReporter = errorReporter;
		}

		public Tuple<IDictionary<ISymbol, VariableData>, ISet<string>> GatherVariables(SyntaxNode node, IMethodSymbol method, ISet<string> usedNames) {
			_result = new Dictionary<ISymbol, VariableData>();
			_usedNames = new HashSet<string>(usedNames);
			_currentMethod = node;
			_variablesDeclaredInsideLoop = new HashSet<ISymbol>();

			if (method != null) {
				foreach (var p in method.Parameters) {
					AddVariable(p, p.RefKind != RefKind.None);
				}
			}

			Visit(node);
			return Tuple.Create((IDictionary<ISymbol, VariableData>)_result, (ISet<string>)_usedNames);
		}

		private void AddVariable(SyntaxNode variableNode, string variableName, bool isUsedByReference = false) {
			var symbol = _semanticModel.GetDeclaredSymbol(variableNode);
			if (symbol == null) {
				_errorReporter.InternalError("Variable " + variableName + " does not resolve to a local.");
				return;
			}
			AddVariable(symbol, isUsedByReference);
		}

		private void AddVariable(ISymbol v, bool isUsedByReference = false) {
			string n = _namer.GetVariableName(v.Name, _usedNames);
			_usedNames.Add(n);
			_result.Add(v, new VariableData(n, _currentMethod, isUsedByReference));
			if (_isInsideLoop)
				_variablesDeclaredInsideLoop.Add(v);
		}

		public override void VisitVariableDeclaration(VariableDeclarationSyntax node) {
			foreach (var varNode in node.Variables) {
				AddVariable(varNode, varNode.Identifier.Text);
			}

			base.VisitVariableDeclaration(node);
		}

		public override void VisitForEachStatement(ForEachStatementSyntax foreachStatement) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				#warning TODO
				//AddVariable(foreachStatement.Identifier, foreachStatement.Identifier.Text);
				base.VisitForEachStatement(foreachStatement);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitCatchClause(CatchClauseSyntax catchClause) {
			#warning TODO
			//if (!catchClause.VariableNameToken.IsNull)
			//	AddVariable(catchClause.VariableNameToken, catchClause.VariableName);
			//base.VisitCatchClause(catchClause);
		}

		public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax lambdaExpression) {
			SyntaxNode oldMethod = _currentMethod;
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_currentMethod = lambdaExpression;
				_isInsideLoop = false;

				AddVariable(lambdaExpression.Parameter, lambdaExpression.Parameter.Identifier.Text, lambdaExpression.Parameter.Modifiers.Any(SyntaxKind.OutKeyword) || lambdaExpression.Parameter.Modifiers.Any(SyntaxKind.RefKeyword));

				base.VisitSimpleLambdaExpression(lambdaExpression);
			}
			finally {
				_currentMethod = oldMethod;
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax lambdaExpression) {
			SyntaxNode oldMethod = _currentMethod;
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_currentMethod = lambdaExpression;
				_isInsideLoop = false;

				foreach (var p in lambdaExpression.ParameterList.Parameters)
					AddVariable(p, p.Identifier.Text, p.Modifiers.Any(SyntaxKind.OutKeyword) || p.Modifiers.Any(SyntaxKind.RefKeyword));

				base.VisitParenthesizedLambdaExpression(lambdaExpression);
			}
			finally {
				_currentMethod = oldMethod;
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax anonymousMethodExpression) {
			SyntaxNode oldMethod = _currentMethod;
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_currentMethod = anonymousMethodExpression;
				_isInsideLoop = false;

				foreach (var p in anonymousMethodExpression.ParameterList.Parameters)
					AddVariable(p, p.Identifier.Text, p.Modifiers.Any(SyntaxKind.OutKeyword) || p.Modifiers.Any(SyntaxKind.RefKeyword));

				base.VisitAnonymousMethodExpression(anonymousMethodExpression);
			}
			finally {
				_currentMethod = oldMethod;
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		private void CheckByRefArguments(IEnumerable<ArgumentSyntax> arguments) {
			foreach (var a in arguments) {
				if (a.RefOrOutKeyword.CSharpKind() != SyntaxKind.None) {
					var symbol = _semanticModel.GetSymbolInfo(a.Expression).Symbol;
					if (symbol.Kind == SymbolKind.Local || symbol.Kind == SymbolKind.Parameter) {
						var current = _result[symbol];
						if (!current.UseByRefSemantics)
							_result[symbol] = new VariableData(current.Name, current.DeclaringMethod, true);
					}
				}
			}
		}

		public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpression) {
			CheckByRefArguments(invocationExpression.ArgumentList.Arguments);
			base.VisitInvocationExpression(invocationExpression);
		}

		public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax objectCreateExpression) {
			CheckByRefArguments( objectCreateExpression.ArgumentList.Arguments);
			base.VisitObjectCreationExpression(objectCreateExpression);
		}

		public override void VisitForStatement(ForStatementSyntax forStatement) {
			foreach (var s in forStatement.Initializers)
				Visit(s);
			Visit(forStatement.Condition);
			foreach (var s in forStatement.Incrementors)
				Visit(s);

			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				Visit(forStatement.Statement);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitWhileStatement(WhileStatementSyntax whileStatement) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				base.VisitWhileStatement(whileStatement);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitDoStatement(DoStatementSyntax doStatement) {
			bool oldIsInsideLoop = _isInsideLoop;
			try {
				_isInsideLoop = true;
				base.VisitDoStatement(doStatement);
			}
			finally {
				_isInsideLoop = oldIsInsideLoop;
			}
		}

		public override void VisitIdentifierName(IdentifierNameSyntax identifierName) {
			#warning TODO check
			var symbol = _semanticModel.GetSymbolInfo(identifierName).Symbol;

			if ((symbol is ILocalSymbol || symbol is IParameterSymbol) && _variablesDeclaredInsideLoop.Contains(symbol) && _currentMethod != _result[symbol].DeclaringMethod) {
				// the variable might suffer from all variables in JS being function-scoped, so use byref semantics.
				var current = _result[symbol];
				if (!current.UseByRefSemantics)
					_result[symbol] = new VariableData(current.Name, current.DeclaringMethod, true);
			}
			base.VisitIdentifierName(identifierName);
		}
	}
}
