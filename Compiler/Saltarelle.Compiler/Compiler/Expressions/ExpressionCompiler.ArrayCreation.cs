using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;

namespace Saltarelle.Compiler.Compiler.Expressions {
	partial class ExpressionCompiler {
		private IEnumerable<ExpressionSyntax> FlattenArrayInitializer(InitializerExpressionSyntax initializer) {
			foreach (var init in initializer.Expressions) {
				if (init.CSharpKind() == SyntaxKind.ArrayInitializerExpression) {
					foreach (var expr in FlattenArrayInitializer((InitializerExpressionSyntax)init))
						yield return expr;
				}
				else {
					yield return init;
				}
			}
		}

		private JsExpression HandleArrayCreation(IArrayTypeSymbol arrayType, InitializerExpressionSyntax initializer, IReadOnlyList<ArrayRankSpecifierSyntax> rankSpecifiers) {
			if (arrayType.Rank == 1) {
				if (initializer != null && initializer.Expressions.Count > 0) {
					var expressions = new List<JsExpression>();
					foreach (var init in initializer.Expressions)
						expressions.Add(MaybeCloneValueType(InnerCompile(init, false, expressions), init, arrayType.ElementType));
					return JsExpression.ArrayLiteral(expressions);
				}
				else {
					var rank = _semanticModel.GetConstantValue(rankSpecifiers[0].Sizes[0]);
					if ((initializer != null && initializer.Expressions.Count == 0) || (rank.HasValue && Convert.ToInt64(rank.Value) == 0)) {
						return JsExpression.ArrayLiteral();
					}
					else {
						return _runtimeLibrary.CreateArray(arrayType.ElementType, new[] { InnerCompile(rankSpecifiers[0].Sizes[0], false) }, this);
					}
				}
			}
			else {
				if (initializer != null) {
					var sizes = new List<long>();
					for (int i = 0; i < arrayType.Rank; i++) {
						var currentInit = initializer;
						for (int j = 0; j < sizes.Count; j++)
							currentInit = (InitializerExpressionSyntax)currentInit.Expressions[0];
						sizes.Add(currentInit.Expressions.Count);
					}
					var result = _runtimeLibrary.CreateArray(arrayType.ElementType, sizes.Select(s => JsExpression.Number(s)), this);

					var temp = _createTemporaryVariable();
					_additionalStatements.Add(JsStatement.Var(_variables[temp].Name, result));
					result = JsExpression.Identifier(_variables[temp].Name);

					var indices = new JsExpression[sizes.Count];

					int index = 0;
					foreach (var elem in FlattenArrayInitializer(initializer)) {
						int remainder = index;
						for (int j = indices.Length - 1; j >= 0; j--) {
							int arg = Convert.ToInt32(sizes[j]);
							indices[j] = JsExpression.Number(remainder % arg);
							remainder /= arg;
						}

						var jsElem = InnerCompile(elem, false);
						_additionalStatements.Add(_runtimeLibrary.SetMultiDimensionalArrayValue(result, indices, MaybeCloneValueType(jsElem, elem, arrayType.ElementType), this));

						index++;
					}

					return result;
				}
				else {
					var sizes = new List<JsExpression>();
					foreach (var a in rankSpecifiers[0].Sizes) {
						sizes.Add(InnerCompile(a, false, sizes));
					}
					return _runtimeLibrary.CreateArray(arrayType.ElementType, sizes, this);
				}
			}
		}
	}
}
