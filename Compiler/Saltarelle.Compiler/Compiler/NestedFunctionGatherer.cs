using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saltarelle.Compiler.Compiler {
#warning TODO: Rename to something better
	public class NestedFunctionGatherer {
		private class CaptureAnalyzer : CSharpSyntaxWalker {
			private bool _usesThis;
			private readonly HashSet<ISymbol> _usedVariables = new HashSet<ISymbol>();
			private readonly SemanticModel _semanticModel;

			public bool UsesThis { get { return _usesThis; } }
			public HashSet<ISymbol> UsedVariables { get { return _usedVariables; } }

			public CaptureAnalyzer(SemanticModel semanticModel) {
				_semanticModel = semanticModel;
			}

			public void Analyze(SyntaxNode node) {
				_usesThis = false;
				_usedVariables.Clear();
				if (node is SimpleLambdaExpressionSyntax) {
					Visit(((SimpleLambdaExpressionSyntax)node).Body);
				}
				else if (node is ParenthesizedLambdaExpressionSyntax) {
					Visit(((ParenthesizedLambdaExpressionSyntax)node).Body);
				}
				else if (node is AnonymousMethodExpressionSyntax) {
					Visit(((AnonymousMethodExpressionSyntax)node).Block);
				}
				else {
					Visit(node);
				}
			}

			public override void VisitThisExpression(ThisExpressionSyntax syntax) {
				_usesThis = true;
			}

			public override void VisitBaseExpression(BaseExpressionSyntax syntax) {
				_usesThis = true;
			}

			public override void VisitIdentifierName(IdentifierNameSyntax syntax) {
				var symbol = _semanticModel.GetSymbolInfo(syntax).Symbol;
				if (symbol is ILocalSymbol || symbol is IParameterSymbol || symbol is IRangeVariableSymbol)
					_usedVariables.Add(symbol);
				else if ((symbol is IFieldSymbol || symbol is IEventSymbol || symbol is IPropertySymbol || symbol is IMethodSymbol) && !symbol.IsStatic)
					_usesThis = true;
			}

			public override void VisitNameEquals(NameEqualsSyntax node) {
				#warning TODO test
			}

			public override void VisitNameColon(NameColonSyntax node) {
				#warning TODO test
			}

			public override void VisitGenericName(GenericNameSyntax node) {
				#warning TODO: Test!
				var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
				if ((symbol is IFieldSymbol || symbol is IEventSymbol || symbol is IPropertySymbol || symbol is IMethodSymbol) && !symbol.IsStatic)
					_usesThis = true;
			}

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
				base.Visit(node.Expression);
			}
		}

		private readonly SemanticModel _semanticModel;

		public NestedFunctionGatherer(SemanticModel semanticModel) {
			_semanticModel = semanticModel;
		}

		public NestedFunctionData GatherInfo(SyntaxNode node, IDictionary<ISymbol, VariableData> allVariables) {
			var analyzer = new CaptureAnalyzer(_semanticModel);
			analyzer.Analyze(node);
			return new NestedFunctionData(analyzer.UsesThis, analyzer.UsedVariables, new HashSet<ISymbol>(allVariables.Keys.Where(v => v.DeclaringSyntaxReferences.Length > 0 && v.DeclaringSyntaxReferences[0].GetSyntax().Ancestors(true).Contains(node))));
		}
	}
}
