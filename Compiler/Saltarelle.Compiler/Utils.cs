using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler {
	public static class Utils {
		/// <summary>
		/// A type is externally visible if it and all its declaring types are public or protected (or protected internal).
		/// </summary>
		public static bool IsExternallyVisible(this ITypeDefinition type) {
			while (type != null) {
				bool isPublic = (type.Accessibility == Accessibility.Public || type.Accessibility == Accessibility.Protected || type.Accessibility == Accessibility.ProtectedOrInternal);
				if (!isPublic)
					return false;
				type = type.DeclaringTypeDefinition;
			}
			return true;
		}

		/// <summary>
		/// A member is externally visible if it is public or protected (or protected internal) and its declaring type is externally visible.
		/// </summary>
		public static bool IsExternallyVisible(this IMember member) {
			return IsExternallyVisible(member.DeclaringType.GetDefinition()) && (member.Accessibility == Accessibility.Public || member.Accessibility == Accessibility.Protected || member.Accessibility == Accessibility.ProtectedOrInternal);
		}

		private static void FindTypeUsageErrors(IEnumerable<IType> types, IMetadataImporter metadataImporter, HashSet<ITypeDefinition> usedUnusableTypes, HashSet<ITypeDefinition> mutableValueTypesBoundToTypeArguments) {
			foreach (var t in types) {
				if (t is ITypeDefinition) {
					if (metadataImporter.GetTypeSemantics((ITypeDefinition)t).Type == TypeScriptSemantics.ImplType.NotUsableFromScript)
						usedUnusableTypes.Add((ITypeDefinition)t);
				}
				else if (t is ParameterizedType) {
					var pt = (ParameterizedType)t;
					foreach (var ta in pt.TypeArguments) {
						if (ta.Kind == TypeKind.Struct && metadataImporter.GetTypeSemantics(ta.GetDefinition()).Type == TypeScriptSemantics.ImplType.MutableValueType)
							mutableValueTypesBoundToTypeArguments.Add(ta.GetDefinition());
					}
					FindTypeUsageErrors(new[] { pt.GetDefinition() }.Concat(pt.TypeArguments), metadataImporter, usedUnusableTypes, mutableValueTypesBoundToTypeArguments);
				}
			}
		}

		public class UnusableTypesResult {
			public IList<IType> UsedUnusableTypes { get; private set; }
			public IList<IType> MutableValueTypesBoundToTypeArguments { get; private set; }

			public bool HasErrors {
				get { return UsedUnusableTypes.Count > 0 || MutableValueTypesBoundToTypeArguments.Count > 0; }
			}

			public UnusableTypesResult(IList<IType> usedUnusableTypes, IList<IType> mutableValueTypesBoundToTypeArguments) {
				UsedUnusableTypes = usedUnusableTypes;
				MutableValueTypesBoundToTypeArguments = mutableValueTypesBoundToTypeArguments;
			}
		}

		public static UnusableTypesResult FindGenericInstantiationErrors(IEnumerable<IType> typeArguments, IMetadataImporter metadataImporter) {
			if (!(typeArguments is ICollection<IType>))
				typeArguments = typeArguments.ToList();

			var usedUnusableTypes = new HashSet<ITypeDefinition>();
			var mutableValueTypesBoundToTypeArguments = new HashSet<ITypeDefinition>();

			foreach (var ta in typeArguments) {
				if (ta.Kind == TypeKind.Struct && metadataImporter.GetTypeSemantics(ta.GetDefinition()).Type == TypeScriptSemantics.ImplType.MutableValueType)
					mutableValueTypesBoundToTypeArguments.Add(ta.GetDefinition());
			}

			FindTypeUsageErrors(typeArguments, metadataImporter, usedUnusableTypes, mutableValueTypesBoundToTypeArguments);
			return new UnusableTypesResult(usedUnusableTypes.Count > 0 ? usedUnusableTypes.ToList<IType>() : (IList<IType>)EmptyList<IType>.Instance, mutableValueTypesBoundToTypeArguments.Count > 0 ? mutableValueTypesBoundToTypeArguments.ToList<IType>() : (IList<IType>)EmptyList<IType>.Instance);
		}

		public static UnusableTypesResult FindTypeUsageErrors(IEnumerable<IType> types, IMetadataImporter metadataImporter) {
			var usedUnusableTypes = new HashSet<ITypeDefinition>();
			var mutableValueTypesBoundToTypeArguments = new HashSet<ITypeDefinition>();
			FindTypeUsageErrors(types, metadataImporter, usedUnusableTypes, mutableValueTypesBoundToTypeArguments);
			return new UnusableTypesResult(usedUnusableTypes.Count > 0 ? usedUnusableTypes.ToList<IType>() : (IList<IType>)EmptyList<IType>.Instance, mutableValueTypesBoundToTypeArguments.Count > 0 ? mutableValueTypesBoundToTypeArguments.ToList<IType>() : (IList<IType>)EmptyList<IType>.Instance);
		}

		/// <summary>
		/// For generic types, returns a ParameterizedType with each type argument being a TypeParameter with the name of the type parameter in the type definition. Returns the TypeDefinition itself for non-generic types.
		/// </summary>
		public static IType SelfParameterize(ITypeDefinition type) {
			return type.TypeParameterCount == 0 ? (IType)type : new ParameterizedType(type, type.TypeParameters.Select(tp => new DefaultTypeParameter(type, tp.Index, tp.Name)));
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

		public static JsExpression ResolveTypeParameter(ITypeParameter tp, ITypeDefinition currentType, IMethod currentMethod, IMetadataImporter metadataImporter, IErrorReporter errorReporter, INamer namer) {
			bool unusable = false;
			switch (tp.OwnerType) {
				case SymbolKind.TypeDefinition:
					unusable = metadataImporter.GetTypeSemantics(currentType).IgnoreGenericArguments;
					break;
				case SymbolKind.Method: {
					var sem = metadataImporter.GetMethodSemantics(currentMethod);
					unusable = sem.Type != MethodScriptSemantics.ImplType.InlineCode && metadataImporter.GetMethodSemantics(currentMethod).IgnoreGenericArguments;
					break;
				}
				default:
					errorReporter.InternalError("Invalid owner " + tp.OwnerType + " for type parameter " + tp);
					return JsExpression.Null;
			}
			if (unusable) {
				errorReporter.Message(Messages._7536, tp.Name, tp.OwnerType == SymbolKind.TypeDefinition ? "type" : "method", tp.OwnerType == SymbolKind.TypeDefinition ? currentType.FullName : currentMethod.FullName);
				return JsExpression.Null;
			}
			return JsExpression.Identifier(namer.GetTypeParameterName(tp));
		}

		public static JsExpression MaybeCloneValueType(JsExpression input, ResolveResult csharpInput, IType type, IMetadataImporter metadataImporter, IRuntimeLibrary runtimeLibrary, IRuntimeContext runtimeContext, bool forceClone = false) {
			if (input is JsConstantExpression)
				return input;	// Primarily the case of null for nullables. forceClone does not matter.

			if (!forceClone) {
				if (input is JsInvocationExpression)
					return input;	// The clone was already performed when the callee returned

				var actualInput = csharpInput;
				var crr = actualInput as ConversionResolveResult;
				if (crr != null) {
					actualInput = crr.Input;
				}

				if (actualInput is InvocationResolveResult) {
					return input;
				}
			}

			type = NullableType.GetUnderlyingType(type);
			if (!IsMutableValueType(type, metadataImporter))
				return input;

			return runtimeLibrary.CloneValueType(input, type, runtimeContext);
		}

		public static bool IsMutableValueType(IType type, IMetadataImporter metadataImporter) {
			return type.Kind == TypeKind.Struct && metadataImporter.GetTypeSemantics(type.GetDefinition()).Type == TypeScriptSemantics.ImplType.MutableValueType;
		}
	}
}
