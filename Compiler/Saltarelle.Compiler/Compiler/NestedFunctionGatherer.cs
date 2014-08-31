using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionGatherer {
		private class StructureGatherer : CSharpSyntaxWalker {
			private NestedFunctionData currentFunction;

			public NestedFunctionData GatherNestedFunctions(SyntaxNode node) {
				currentFunction = new NestedFunctionData(null) { DefinitionNode = node };
				Visit(node);
				return currentFunction;
			}

			private void VisitNestedFunction(SyntaxNode node, SyntaxNode body) {
				var parentFunction = currentFunction;

				if (parentFunction.DefinitionNode != node) {
					currentFunction = new NestedFunctionData(parentFunction) { DefinitionNode = node };
					Visit(body);

					parentFunction.NestedFunctions.Add(currentFunction);
					currentFunction = parentFunction;
				}
				else {
					Visit(body);
				}
			}

			public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax lambdaExpression) {
				VisitNestedFunction(lambdaExpression, lambdaExpression.Body);
			}

			public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax lambdaExpression) {
				VisitNestedFunction(lambdaExpression, lambdaExpression.Body);
			}

			public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax anonymousMethodExpression) {
				VisitNestedFunction(anonymousMethodExpression, anonymousMethodExpression.Block);
			}
		}

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
				if (symbol is ILocalSymbol || symbol is IParameterSymbol)
					_usedVariables.Add(symbol);
				else if ((symbol is IFieldSymbol || symbol is IEventSymbol || symbol is IPropertySymbol || symbol is IMethodSymbol) && !symbol.IsStatic)
					_usesThis = true;
			}

			public override void VisitGenericName(GenericNameSyntax node) {
				#warning TODO: Test!
				var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
				if ((symbol is IFieldSymbol || symbol is IEventSymbol || symbol is IPropertySymbol || symbol is IMethodSymbol) && !symbol.IsStatic)
					_usesThis = true;
			}

			public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) {
			}

			public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) {
			}

			public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) {
			}
		}

		private readonly SemanticModel _semanticModel;

		public NestedFunctionGatherer(SemanticModel semanticModel) {
			_semanticModel = semanticModel;
		}

		public NestedFunctionData GatherNestedFunctions(SyntaxNode node, IDictionary<ISymbol, VariableData> allVariables) {
			var result = new StructureGatherer().GatherNestedFunctions(node);

			var allNestedFunctions = new[] { result }.Concat(result.DirectlyOrIndirectlyNestedFunctions).ToDictionary(f => f.DefinitionNode);
			foreach (var v in allVariables) {
				allNestedFunctions[v.Value.DeclaringMethod].DirectlyDeclaredVariables.Add(v.Key);
			}

			var analyzer = new CaptureAnalyzer(_semanticModel);
			foreach (var f in allNestedFunctions.Values) {
				analyzer.Analyze(f.DefinitionNode);
				f.DirectlyUsesThis = analyzer.UsesThis;
				foreach (var v in analyzer.UsedVariables)
					f.DirectlyUsedVariables.Add(v);
				f.Freeze();
			}

			return result;
		}
	}
}
