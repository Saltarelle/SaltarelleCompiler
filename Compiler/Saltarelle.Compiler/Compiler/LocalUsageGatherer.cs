using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saltarelle.Compiler.Compiler {
	public static class LocalUsageGatherer {
		private class Analyzer : CSharpSyntaxWalker {
			private bool _usesThis;
			private readonly HashSet<ISymbol> _usedVariables = new HashSet<ISymbol>();
			private readonly SemanticModel _semanticModel;

			public bool UsesThis { get { return _usesThis; } }
			public HashSet<ISymbol> UsedVariables { get { return _usedVariables; } }

			public Analyzer(SemanticModel semanticModel) {
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
			}

			public override void VisitNameColon(NameColonSyntax node) {
			}

			public override void VisitGenericName(GenericNameSyntax node) {
				var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
				if ((symbol is IFieldSymbol || symbol is IEventSymbol || symbol is IPropertySymbol || symbol is IMethodSymbol) && !symbol.IsStatic)
					_usesThis = true;
			}

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
				base.Visit(node.Expression);
			}
		}

		public static LocalUsageData GatherInfo(SemanticModel semanticModel, SyntaxNode node) {
			var analyzer = new Analyzer(semanticModel);
			analyzer.Analyze(node);
			return new LocalUsageData(analyzer.UsesThis, analyzer.UsedVariables);
		}
	}
}
