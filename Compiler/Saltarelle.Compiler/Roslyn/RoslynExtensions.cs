using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Saltarelle.Compiler.Roslyn {
	public class CastInfo {
		public ITypeSymbol FromType { get; private set; }
		public ITypeSymbol ToType { get; private set; }
		public Conversion Conversion { get; private set; }

		public CastInfo(ITypeSymbol fromType, ITypeSymbol toType, Conversion conversion) {
			FromType = fromType;
			ToType = toType;
			Conversion = conversion;
		}
	}

	public struct ArgumentForCall {
		private readonly object _v;

		public object DefaultValue { get { return _v is ExpressionSyntax ? null : _v; } }
		public ExpressionSyntax Argument { get { return _v as ExpressionSyntax; } }
		public Tuple<ITypeSymbol, ImmutableArray<ExpressionSyntax>> ParamArray { get { return _v as Tuple<ITypeSymbol, ImmutableArray<ExpressionSyntax>>; } }
		public bool Empty { get { return _v == null; } }

		public ArgumentForCall(object v) {
			_v = v;
		}
	}

	public class ArgumentMap {
		public ImmutableArray<ArgumentForCall> ArgumentsForCall { get; private set; }
		public ImmutableArray<int> ArgumentToParameterMap { get; private set; }
		public bool IsExpandedForm { get; private set; }

		public ArgumentMap(ImmutableArray<ArgumentForCall> argumentsForCall, ImmutableArray<int> argumentToParameterMap, bool isExpandedForm) {
			ArgumentsForCall = argumentsForCall;
			ArgumentToParameterMap = argumentToParameterMap;
			IsExpandedForm = isExpandedForm;
		}

		public static readonly ArgumentMap Empty = new ArgumentMap(ImmutableArray<ArgumentForCall>.Empty, ImmutableArray<int>.Empty, false);
	}

	public static class RoslynExtensions {
		public static IEnumerable<ITypeSymbol> GetAllBaseTypes(this ITypeSymbol type) {
			foreach (var i in type.AllInterfaces)
				yield return i;

			while (type != null) {
				yield return type;
				type = type.BaseType;
			}
		}

		public static Location GetLocation(this ISymbol symbol)
		{
			if (symbol.DeclaringSyntaxReferences.Length == 0)
				return null;
			var syntax = symbol.DeclaringSyntaxReferences[0].GetSyntax();
			if (syntax == null)
				return null;
			return syntax.GetLocation();
		}

		public static bool IsNullable(this ITypeSymbol type) {
			return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
		}

		public static ITypeSymbol UnpackNullable(this ITypeSymbol type) {
			return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ? ((INamedTypeSymbol)type).TypeArguments[0] : type;
		}

		public static CastInfo GetCastInfo(this SemanticModel semanticModel, CastExpressionSyntax node) {
			var fromType = semanticModel.GetTypeInfo(node.Expression).Type;
			var toType = semanticModel.GetTypeInfo(node).Type;
			var conversion = semanticModel.ClassifyConversion(node.Expression, toType, true);
			return new CastInfo(fromType, toType, conversion);
		}

		public static bool IsLiftedOperator(this SemanticModel semanticModel, ExpressionSyntax operatorNode) {
			ExpressionSyntax input;
			if (operatorNode is BinaryExpressionSyntax)
				input = ((BinaryExpressionSyntax)operatorNode).Left;
			else if (operatorNode is PrefixUnaryExpressionSyntax)
				input = ((PrefixUnaryExpressionSyntax)operatorNode).Operand;
			else if (operatorNode is PostfixUnaryExpressionSyntax)
				input = ((PostfixUnaryExpressionSyntax)operatorNode).Operand;
			else
				return false;

			if (!semanticModel.GetTypeInfo(input).ConvertedType.IsNullable())
				return false;

			var symbol = semanticModel.GetSymbolInfo(operatorNode).Symbol as IMethodSymbol;
			return symbol != null && !symbol.Parameters[0].Type.IsNullable();
		}

		private static readonly PropertyInfo _userDefinedFromConversion = typeof(Conversion).GetProperty("UserDefinedFromConversion", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly PropertyInfo _userDefinedToConversion = typeof(Conversion).GetProperty("UserDefinedToConversion", BindingFlags.Instance | BindingFlags.NonPublic);

		public static Conversion UserDefinedFromConversion(this Conversion conversion) {
			return (Conversion)_userDefinedFromConversion.GetValue(conversion, null);
		}

		public static Conversion UserDefinedToConversion(this Conversion conversion) {
			return (Conversion)_userDefinedToConversion.GetValue(conversion, null);
		}

		public static bool IsLiftedConversion(this SemanticModel semanticModel, Conversion conversion, ExpressionSyntax input) {
			if (conversion.MethodSymbol == null)
				return false;
			var inputType = semanticModel.GetTypeInfo(input).ConvertedType;
			if (!inputType.IsNullable())
				return false;
			return !conversion.MethodSymbol.Parameters[0].Type.IsNullable();
		}

		public static bool IsNonVirtualAccess(this ExpressionSyntax expression) {
			var mae = expression as MemberAccessExpressionSyntax;
			return mae != null && mae.Expression is BaseExpressionSyntax;
		}

		public static bool IsOverridable(this ISymbol symbol) {
			return (symbol.IsVirtual || symbol.IsOverride) && !symbol.IsSealed && !symbol.IsStatic;
		}

		private static readonly ConcurrentDictionary<int, ImmutableArray<int>> ArgumentToParameterMapCache = new ConcurrentDictionary<int, ImmutableArray<int>>();
		private static ImmutableArray<int> CreateIdentityArgumentToParameterMap(int argCount) {
			ImmutableArray<int> result;
			if (ArgumentToParameterMapCache.TryGetValue(argCount, out result))
				return result;
			result = ImmutableArray.CreateRange(Enumerable.Range(0, argCount));
			ArgumentToParameterMapCache.TryAdd(argCount, result);
			return result;
		}

		private static bool IsExpandedForm(this SemanticModel semanticModel, ExpressionSyntax target, BaseArgumentListSyntax argumentList, ImmutableArray<IParameterSymbol> parameters) {
			if (parameters.Length == 0 || !parameters[parameters.Length - 1].IsParams)
				return false;	// Last parameter must be params
			if (argumentList.Arguments.Count < parameters.Length - 1)
				return false;	// No default arguments are allowed
			if (argumentList.Arguments.Any(a => a.NameColon != null))
				return false;	// No named arguments are allowed
			if (argumentList.Arguments.Count == parameters.Length - 1)
				return true;	// Empty param array

			var lastType = semanticModel.GetTypeInfo(argumentList.Arguments[argumentList.Arguments.Count - 1].Expression).ConvertedType;
			if (Equals(((IArrayTypeSymbol)parameters[parameters.Length - 1].Type).ElementType, lastType))
				return true;	// A param array needs to be created

			return false;
		}

		private static ArgumentMap GetArgumentMap(SemanticModel semanticModel, ExpressionSyntax target, BaseArgumentListSyntax argumentList, ImmutableArray<IParameterSymbol> parameters) {
			#warning TODO: Extension method (also in IsExpandedForm)
			bool isExpandedForm = semanticModel.IsExpandedForm(target, argumentList, parameters);

			var argumentToParameterMap = new int[argumentList.Arguments.Count];
			var argumentsForCall = new ArgumentForCall[parameters.Length];

			if (target != null)
				argumentsForCall[0] = new ArgumentForCall(target);

			for (int i = 0; i < argumentList.Arguments.Count; i++) {
				argumentToParameterMap[i] = -1;
				var argument = argumentList.Arguments[i];
				if (argument.NameColon == null) {
					// positional argument
					if (i < parameters.Length) {
						argumentToParameterMap[i] = i + (target != null ? 1 : 0);
						argumentsForCall[i + (target != null ? 1 : 0)] = new ArgumentForCall(argument.Expression);
					}
					else if (isExpandedForm) {
						argumentToParameterMap[i] = parameters.Length - 1;
					}
				}
				else {
					// named argument
					for (int j = 0; j < parameters.Length; j++) {
						if (argument.NameColon.Name.Identifier.Text == parameters[j].Name) {
							argumentToParameterMap[i] = j + (target != null ? 1 : 0);
							argumentsForCall[j] = new ArgumentForCall(argument.Expression);
						}
					}
				}
			}

			if (isExpandedForm) {
				var elementType = ((IArrayTypeSymbol)parameters[parameters.Length - 1].Type).ElementType;
				argumentsForCall[argumentsForCall.Length - 1] = new ArgumentForCall(Tuple.Create(elementType, ImmutableArray.CreateRange(argumentList.Arguments.Skip(parameters.Length -1).Select(a => a.Expression))));
			}

			for (int i = 0; i < parameters.Length; i++) {
				if (argumentsForCall[i].Empty)
					argumentsForCall[i] = new ArgumentForCall(parameters[i].ExplicitDefaultValue);
			}

			return new ArgumentMap(ImmutableArray.Create(argumentsForCall), ImmutableArray.Create(argumentToParameterMap), isExpandedForm);
		}

		public static ArgumentMap GetArgumentMap(this SemanticModel semanticModel, ElementAccessExpressionSyntax node) {
			var property = semanticModel.GetSymbolInfo(node).Symbol as IPropertySymbol;
			if (property == null)
				return null;
			return GetArgumentMap(semanticModel, null, node.ArgumentList, property.Parameters);
		}

		public static ArgumentMap GetArgumentMap(this SemanticModel semanticModel, InvocationExpressionSyntax node) {
			var method = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
			if (method == null)
				return null;

			ExpressionSyntax target = null;
			if (method.ReducedFrom != null) {
				method = method.ReducedFrom;
				var mae = (MemberAccessExpressionSyntax)node.Expression;
				target = mae.Expression;
			}

			return GetArgumentMap(semanticModel, target, node.ArgumentList, method.Parameters);
		}

		public static IMethodSymbol UnReduceIfExtensionMethod(this IMethodSymbol method) {
			return method.ReducedFrom ?? method;
		}

		private static string AppendTypeArguments(string localName, IReadOnlyCollection<ITypeSymbol> typeArguments) {
			if (typeArguments.Count > 0) {
				bool first = true;
				foreach (var ta in typeArguments) {
					localName += (first ? "<" : ", ") + ta.FullyQualifiedName();
					first = false;
				}
				localName += ">";
			}
			return localName;
		}

		public static string FullyQualifiedName(this ISymbol symbol) {
			var localName = symbol.Name;
			if (symbol is INamedTypeSymbol)
				localName = AppendTypeArguments(localName, ((INamedTypeSymbol)symbol).TypeArguments);
			else if (symbol is IMethodSymbol)
				localName = AppendTypeArguments(localName, ((IMethodSymbol)symbol).TypeArguments);

			if (symbol.ContainingType != null)
				return symbol.ContainingType.FullyQualifiedName() + "." + localName;
			else if (symbol.ContainingNamespace != null)
				return symbol.ContainingNamespace + "." + localName;
			else
				return localName;
		}
	}
}
