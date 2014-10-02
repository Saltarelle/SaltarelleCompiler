using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler {
	public static class Utils {
		/// <summary>
		/// A type is externally visible if it and all its declaring types are public or protected (or protected internal).
		/// A member is externally visible if it is public or protected (or protected internal) and its declaring type is externally visible.
		/// </summary>
		public static bool IsExternallyVisible(this ISymbol symbol) {
			while (symbol != null) {
				if (symbol is INamespaceSymbol)
					return true;

				bool isPublic = (symbol.DeclaredAccessibility == Accessibility.NotApplicable || symbol.DeclaredAccessibility == Accessibility.Public || symbol.DeclaredAccessibility == Accessibility.Protected || symbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal);
				if (!isPublic)
					return false;
				symbol = symbol.ContainingSymbol;
			}
			return true;
		}

		private static void FindTypeUsageErrors(IEnumerable<ITypeSymbol> types, IMetadataImporter metadataImporter, HashSet<INamedTypeSymbol> usedUnusableTypes, HashSet<INamedTypeSymbol> mutableValueTypesBoundToTypeArguments) {
			foreach (var t in types) {
				var nt = t as INamedTypeSymbol;
				if (nt != null && !nt.IsAnonymousType && nt.TypeKind != TypeKind.Delegate) {
					if (metadataImporter.GetTypeSemantics(nt.OriginalDefinition).Type == TypeScriptSemantics.ImplType.NotUsableFromScript)
						usedUnusableTypes.Add((INamedTypeSymbol)t);

					if (!nt.IsUnboundGenericType && nt.TypeArguments.Length > 0) {
						foreach (var ta in nt.TypeArguments) {
							if (ta.TypeKind == TypeKind.Struct && metadataImporter.GetTypeSemantics((INamedTypeSymbol)ta.OriginalDefinition).Type == TypeScriptSemantics.ImplType.MutableValueType)
								mutableValueTypesBoundToTypeArguments.Add((INamedTypeSymbol)ta.OriginalDefinition);
						}

						FindTypeUsageErrors(nt.GetAllTypeArguments(), metadataImporter, usedUnusableTypes, mutableValueTypesBoundToTypeArguments);
					}
				}
			}
		}

		public class UnusableTypesResult {
			public IList<ITypeSymbol> UsedUnusableTypes { get; private set; }
			public IList<ITypeSymbol> MutableValueTypesBoundToTypeArguments { get; private set; }

			public bool HasErrors {
				get { return UsedUnusableTypes.Count > 0 || MutableValueTypesBoundToTypeArguments.Count > 0; }
			}

			public UnusableTypesResult(IList<ITypeSymbol> usedUnusableTypes, IList<ITypeSymbol> mutableValueTypesBoundToTypeArguments) {
				UsedUnusableTypes = usedUnusableTypes;
				MutableValueTypesBoundToTypeArguments = mutableValueTypesBoundToTypeArguments;
			}
		}

		public static UnusableTypesResult FindGenericInstantiationErrors(ICollection<ITypeSymbol> typeArguments, IMetadataImporter metadataImporter) {
			var usedUnusableTypes = new HashSet<INamedTypeSymbol>();
			var mutableValueTypesBoundToTypeArguments = new HashSet<INamedTypeSymbol>();

			foreach (var ta in typeArguments) {
				if (ta.TypeKind == TypeKind.Struct && metadataImporter.GetTypeSemantics((INamedTypeSymbol)ta.OriginalDefinition).Type == TypeScriptSemantics.ImplType.MutableValueType)
					mutableValueTypesBoundToTypeArguments.Add((INamedTypeSymbol)ta.OriginalDefinition);
			}

			FindTypeUsageErrors(typeArguments, metadataImporter, usedUnusableTypes, mutableValueTypesBoundToTypeArguments);
			return new UnusableTypesResult(usedUnusableTypes.Count > 0 ? usedUnusableTypes.ToList<ITypeSymbol>() : (IList<ITypeSymbol>)ImmutableArray<ITypeSymbol>.Empty, mutableValueTypesBoundToTypeArguments.Count > 0 ? mutableValueTypesBoundToTypeArguments.ToList<ITypeSymbol>() : (IList<ITypeSymbol>)ImmutableArray<ITypeSymbol>.Empty);
		}

		public static UnusableTypesResult FindTypeUsageErrors(IEnumerable<ITypeSymbol> types, IMetadataImporter metadataImporter) {
			var usedUnusableTypes = new HashSet<INamedTypeSymbol>();
			var mutableValueTypesBoundToTypeArguments = new HashSet<INamedTypeSymbol>();
			FindTypeUsageErrors(types, metadataImporter, usedUnusableTypes, mutableValueTypesBoundToTypeArguments);
			return new UnusableTypesResult(usedUnusableTypes.Count > 0 ? usedUnusableTypes.ToList<ITypeSymbol>() : (IList<ITypeSymbol>)ImmutableArray<ITypeSymbol>.Empty, mutableValueTypesBoundToTypeArguments.Count > 0 ? mutableValueTypesBoundToTypeArguments.ToList<ITypeSymbol>() : (IList<ITypeSymbol>)ImmutableArray<ITypeSymbol>.Empty);
		}

		public static void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsStatement> statementList, IList<JsExpression> expressions, JsExpression newExpression, Func<string> createTemporaryVariable) {
			CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(statementList, expressions, new ExpressionCompileResult(newExpression, new JsStatement[0]), createTemporaryVariable);
		}

		public static void CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(IList<JsStatement> statementList, IList<JsExpression> expressions, ExpressionCompileResult newExpressions, Func<string> createTemporaryVariable) {
			for (int i = 0; i < expressions.Count; i++) {
				if (ExpressionOrderer.DoesOrderMatter(expressions[i], newExpressions)) {
					var temp = createTemporaryVariable();
					statementList.Add(JsStatement.Var(temp, expressions[i]));
					expressions[i] = JsExpression.Identifier(temp);
				}
			}
		}

		public static JsExpression EnsureCanBeEvaluatedMultipleTimes(IList<JsStatement> statementList, JsExpression expression, IList<JsExpression> expressionsThatMustBeEvaluatedBefore, Func<string> createTemporaryVariable) {
			if (IsJsExpressionComplexEnoughToGetATemporaryVariable.Analyze(expression)) {
				CreateTemporariesForAllExpressionsThatHaveToBeEvaluatedBeforeNewExpression(statementList, expressionsThatMustBeEvaluatedBefore, expression, createTemporaryVariable);
				var temp = createTemporaryVariable();
				statementList.Add(JsStatement.Var(temp, expression));
				return JsExpression.Identifier(temp);
			}
			else
				return expression;
		}

		public static JsExpression ResolveTypeParameter(ITypeParameterSymbol tp, IMetadataImporter metadataImporter, IErrorReporter errorReporter, INamer namer) {
			bool unusable;
			switch (tp.TypeParameterKind) {
				case TypeParameterKind.Type:
					unusable = metadataImporter.GetTypeSemantics(tp.DeclaringType).IgnoreGenericArguments;
					break;
				case TypeParameterKind.Method: {
					var sem = metadataImporter.GetMethodSemantics(tp.DeclaringMethod);
					unusable = sem.Type != MethodScriptSemantics.ImplType.InlineCode && sem.IgnoreGenericArguments;
					break;
				}
				default:
					errorReporter.InternalError("Invalid owner " + tp.TypeParameterKind + " for type parameter " + tp);
					return JsExpression.Null;
			}
			if (unusable) {
				errorReporter.Message(Messages._7536, tp.Name, tp.TypeParameterKind == TypeParameterKind.Type ? "type" : "method", tp.TypeParameterKind == TypeParameterKind.Type ? tp.DeclaringType.FullyQualifiedName() : tp.DeclaringMethod.FullyQualifiedName());
				return JsExpression.Null;
			}
			return JsExpression.Identifier(namer.GetTypeParameterName(tp));
		}

		public static JsExpression MaybeCloneValueType(JsExpression input, ExpressionSyntax csharpInput, ITypeSymbol type, IMetadataImporter metadataImporter, IRuntimeLibrary runtimeLibrary, IRuntimeContext runtimeContext, bool forceClone = false) {
			if (input is JsConstantExpression)
				return input;	// Primarily the case of null for nullables. forceClone does not matter.

			if (!forceClone) {
				if (input is JsInvocationExpression)
					return input;	// The clone was already performed when the callee returned

				if (csharpInput is InvocationExpressionSyntax || csharpInput is AnonymousObjectCreationExpressionSyntax || csharpInput is ObjectCreationExpressionSyntax) {
					return input;
				}
			}
			
			type = type.UnpackNullable();
			if (!IsMutableValueType(type, metadataImporter))
				return input;
			
			return runtimeLibrary.CloneValueType(input, type, runtimeContext);
		}

		public static bool IsMutableValueType(ITypeSymbol type, IMetadataImporter metadataImporter) {
			return type.TypeKind == TypeKind.Struct && metadataImporter.GetTypeSemantics((INamedTypeSymbol)type.OriginalDefinition).Type == TypeScriptSemantics.ImplType.MutableValueType;
		}
	}
}
