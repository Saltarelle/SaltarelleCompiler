using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saltarelle.Compiler.Compiler {
	public class NestedFunctionGatherer {
		private class StructureGatherer : CSharpSyntaxWalker {
			private readonly SemanticModel _semanticModel;
			private NestedFunctionData currentFunction;

			public StructureGatherer(SemanticModel semanticModel) {
				_semanticModel = semanticModel;
			}

			private SyntaxNode GetBodyNode(SyntaxNode methodNode) {
				if (methodNode is AnonymousMethodExpressionSyntax)
					return ((AnonymousMethodExpressionSyntax)methodNode).Block;
				else if (methodNode is SimpleLambdaExpressionSyntax)
					return ((SimpleLambdaExpressionSyntax)methodNode).Body;
				else if (methodNode is ParenthesizedLambdaExpressionSyntax)
					return ((ParenthesizedLambdaExpressionSyntax)methodNode).Body;
				else if (methodNode is MethodDeclarationSyntax)
					return ((MethodDeclarationSyntax)methodNode).Body;
				else
					return methodNode;
			}

			public NestedFunctionData GatherNestedFunctions(SyntaxNode node) {
				currentFunction = new NestedFunctionData(null) { DefinitionNode = node, BodyNode = GetBodyNode(node), SyntaxNode = node };
				Visit(node);
				return currentFunction;
			}

			private void VisitNestedFunction(SyntaxNode node, SyntaxNode body) {
				var parentFunction = currentFunction;

				currentFunction = new NestedFunctionData(parentFunction) { DefinitionNode = node, BodyNode = body, SyntaxNode = node };
				Visit(body);

				parentFunction.NestedFunctions.Add(currentFunction);
				currentFunction = parentFunction;
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
				Visit(node);
			}

			public override void VisitThisExpression(ThisExpressionSyntax syntax) {
				_usesThis = true;
			}

			public override void VisitBaseExpression(BaseExpressionSyntax syntax) {
				_usesThis = true;
			}

			public override void VisitIdentifierName(IdentifierNameSyntax syntax) {
				var symbol = _semanticModel.GetSymbolInfo(syntax);
				if (symbol.Symbol is ILocalSymbol || symbol.Symbol is IParameterSymbol)
					_usedVariables.Add(symbol.Symbol);
				else if (symbol.Symbol is IFieldSymbol || symbol.Symbol is IEventSymbol || symbol.Symbol is IPropertySymbol || symbol.Symbol is IMethodSymbol)
					_usesThis = true;
			}
		}

		private readonly SemanticModel _semanticModel;

		public NestedFunctionGatherer(SemanticModel semanticModel) {
			_semanticModel = semanticModel;
		}

		public NestedFunctionData GatherNestedFunctions(SyntaxNode node, IDictionary<ISymbol, VariableData> allVariables) {
			var result = new StructureGatherer(_semanticModel).GatherNestedFunctions(node);

			var allNestedFunctions = new[] { result }.Concat(result.DirectlyOrIndirectlyNestedFunctions).ToDictionary(f => f.DefinitionNode);
			foreach (var v in allVariables) {
				allNestedFunctions[v.Value.DeclaringMethod].DirectlyDeclaredVariables.Add(v.Key);
			}

			var analyzer = new CaptureAnalyzer(_semanticModel);
			foreach (var f in allNestedFunctions.Values) {
				analyzer.Analyze(f.BodyNode);
				f.DirectlyUsesThis = analyzer.UsesThis;
				foreach (var v in analyzer.UsedVariables)
					f.DirectlyUsedVariables.Add(v);
				f.Freeze();
			}

			return result;
		}
	}
}
