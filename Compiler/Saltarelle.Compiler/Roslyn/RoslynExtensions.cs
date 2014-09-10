using System;
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

		public Tuple<ITypeSymbol, object> Constant { get { return _v as Tuple<ITypeSymbol, object>; } }
		public ExpressionSyntax Argument { get { return _v as ExpressionSyntax; } }
		public Tuple<ITypeSymbol, ImmutableArray<ExpressionSyntax>> ParamArray { get { return _v as Tuple<ITypeSymbol, ImmutableArray<ExpressionSyntax>>; } }
		public bool Empty { get { return _v == null; } }

		public ArgumentForCall(Tuple<ITypeSymbol, object> constant) {
			_v = constant;
		}

		public ArgumentForCall(ExpressionSyntax argument) {
			_v = argument;
		}

		public ArgumentForCall(Tuple<ITypeSymbol, ImmutableArray<ExpressionSyntax>> paramArray) {
			_v = paramArray;
		}
	}

	public class ArgumentMap {
		public ImmutableArray<ArgumentForCall> ArgumentsForCall { get; private set; }
		public ImmutableArray<int> ArgumentToParameterMap { get; private set; }

		public ArgumentMap(ImmutableArray<ArgumentForCall> argumentsForCall, ImmutableArray<int> argumentToParameterMap) {
			ArgumentsForCall = argumentsForCall;
			ArgumentToParameterMap = argumentToParameterMap;
		}

		public bool IsExpandedForm {
			get {
				return ArgumentsForCall.Length > 0 && ArgumentsForCall[ArgumentsForCall.Length - 1].ParamArray != null;
			}
		}

		public bool CanBeTreatedAsExpandedForm {
			get {
				if (ArgumentsForCall.Length == 0)
					return false;
				var last = ArgumentsForCall[ArgumentsForCall.Length - 1];
				return last.ParamArray != null || last.Argument is ArrayCreationExpressionSyntax || last.Argument is ImplicitArrayCreationExpressionSyntax;
			}
		}

		public static readonly ArgumentMap Empty = new ArgumentMap(ImmutableArray<ArgumentForCall>.Empty, ImmutableArray<int>.Empty);

		public static ArgumentMap CreateIdentity(params ExpressionSyntax[] arguments) {
			var argumentsForCall = ImmutableArray.CreateRange(arguments.Select(a => new ArgumentForCall(a)));
			return new ArgumentMap(argumentsForCall, ImmutableArray.CreateRange(Enumerable.Range(0, argumentsForCall.Length)));
		}

		public static ArgumentMap CreateIdentity(IEnumerable<ExpressionSyntax> arguments) {
			var argumentsForCall = ImmutableArray.CreateRange(arguments.Select(a => new ArgumentForCall(a)));
			return new ArgumentMap(argumentsForCall, ImmutableArray.CreateRange(Enumerable.Range(0, argumentsForCall.Length)));
		}
	}

	public static class RoslynExtensions {
		public static IEnumerable<ITypeSymbol> GetAllBaseTypes(this ITypeSymbol type) {
			foreach (var i in type.AllInterfaces)
				yield return i;

			type = type.BaseType;
			while (type != null) {
				yield return type;
				type = type.BaseType;
			}
		}

		public static IEnumerable<ITypeSymbol> GetSelfAndAllBaseTypes(this ITypeSymbol type) {
			yield return type;
			foreach (var t in type.GetAllBaseTypes())
				yield return t;
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
		private static readonly Conversion _identityConversion = (Conversion)typeof(Conversion).GetField("Identity", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

		public static Conversion UserDefinedFromConversion(this Conversion conversion) {
			var result = (Conversion)_userDefinedFromConversion.GetValue(conversion, null);
			return result.Exists ? result : _identityConversion;
		}

		public static Conversion UserDefinedToConversion(this Conversion conversion) {
			var result = (Conversion)_userDefinedToConversion.GetValue(conversion, null);
			return result.Exists ? result : _identityConversion;
		}

		public static bool IsLiftedConversion(this SemanticModel semanticModel, Conversion conversion, ITypeSymbol inputType) {
			if (conversion.MethodSymbol == null)
				return false;
			if (!inputType.IsNullable())
				return false;
			return !conversion.MethodSymbol.Parameters[0].Type.IsNullable();
		}

		public static bool IsNonVirtualAccess(this ExpressionSyntax expression) {
			var mae = expression as MemberAccessExpressionSyntax;
			if (mae != null && mae.Expression is BaseExpressionSyntax)
				return true;
			var eae = expression as ElementAccessExpressionSyntax;
			if (eae != null && eae.Expression is BaseExpressionSyntax)
				return true;
			return false;
		}

		public static bool IsOverridable(this ISymbol symbol) {
			return (symbol.IsVirtual || symbol.IsOverride || symbol.IsAbstract) && !symbol.IsSealed && !symbol.IsStatic;
		}

		private static bool IsExpandedForm(this SemanticModel semanticModel, bool isReducedExtensionMethod, IReadOnlyList<ArgumentSyntax> arguments, ImmutableArray<IParameterSymbol> parameters) {
			if (parameters.Length == 0 || !parameters[parameters.Length - 1].IsParams)
				return false;	// Last parameter must be params

			int actualArgumentCount = arguments.Count + (isReducedExtensionMethod ? 1 : 0);

			if (actualArgumentCount < parameters.Length - 1)
				return false;	// No default arguments are allowed

			if (arguments.Any(a => a.NameColon != null))
				return false;	// No named arguments are allowed

			if (actualArgumentCount == parameters.Length - 1)
				return true;	// Empty param array

			var lastType = semanticModel.GetTypeInfo(arguments[arguments.Count - 1].Expression).ConvertedType;
			if (Equals(((IArrayTypeSymbol)parameters[parameters.Length - 1].Type).ElementType, lastType))
				return true;	// A param array needs to be created

			return false;
		}

		private static ArgumentMap GetArgumentMap(SemanticModel semanticModel, ExpressionSyntax target, IReadOnlyList<ArgumentSyntax> arguments, ImmutableArray<IParameterSymbol> parameters) {
			bool isExpandedForm = semanticModel.IsExpandedForm(target != null, arguments, parameters);

			var argumentToParameterMap = new int[arguments.Count];
			var argumentsForCall = new ArgumentForCall[parameters.Length];

			if (target != null)
				argumentsForCall[0] = new ArgumentForCall(target);

			for (int i = 0; i < arguments.Count; i++) {
				argumentToParameterMap[i] = -1;
				var argument = arguments[i];
				if (argument.NameColon == null) {
					// positional argument
					if (i < parameters.Length - (target != null ? 1 : 0)) {
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
				argumentsForCall[argumentsForCall.Length - 1] = new ArgumentForCall(Tuple.Create(elementType, ImmutableArray.CreateRange(arguments.Skip(parameters.Length - (target != null ? 2 : 1)).Select(a => a.Expression))));
			}

			for (int i = 0; i < parameters.Length; i++) {
				if (argumentsForCall[i].Empty)
					argumentsForCall[i] = new ArgumentForCall(Tuple.Create(parameters[i].Type, parameters[i].ExplicitDefaultValue));
			}

			return new ArgumentMap(ImmutableArray.Create(argumentsForCall), ImmutableArray.Create(argumentToParameterMap));
		}

		public static ArgumentMap GetArgumentMap(this SemanticModel semanticModel, ElementAccessExpressionSyntax node) {
			var property = semanticModel.GetSymbolInfo(node).Symbol as IPropertySymbol;
			if (property == null)
				return null;
			return GetArgumentMap(semanticModel, null, node.ArgumentList.Arguments, property.Parameters);
		}

		public static ArgumentMap GetArgumentMap(this SemanticModel semanticModel, ObjectCreationExpressionSyntax node) {
			var method = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
			if (method == null)
				return null;
			return GetArgumentMap(semanticModel, null, node.ArgumentList != null ? node.ArgumentList.Arguments : (IReadOnlyList<ArgumentSyntax>)ImmutableArray<ArgumentSyntax>.Empty, method.Parameters);
		}

		public static ArgumentMap GetArgumentMap(this SemanticModel semanticModel, InvocationExpressionSyntax node) {
			var method = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
			if (method == null)
				return null;

			ExpressionSyntax target = null;
			if (method.ReducedFrom != null) {
				method = method.UnReduceIfExtensionMethod();
				var mae = (MemberAccessExpressionSyntax)node.Expression;
				target = mae.Expression;
			}

			return GetArgumentMap(semanticModel, target, node.ArgumentList.Arguments, method.Parameters);
		}

		public static ArgumentMap GetArgumentMap(this SemanticModel semanticModel, ConstructorInitializerSyntax node) {
			var method = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
			if (method == null)
				return null;

			return GetArgumentMap(semanticModel, null, node.ArgumentList.Arguments, method.Parameters);
		}

		private static Tuple<ITypeSymbol, object> ConvertToTuple(TypedConstant value) {
			return Tuple.Create(value.Type, value.Kind == TypedConstantKind.Array ? ImmutableArray.CreateRange(value.Values.Select(ConvertToTuple)) : value.Value);
		}

		public static ArgumentMap GetConstructorArgumentMap(this AttributeData attribute) {
			var argumentsForCall = new ArgumentForCall[attribute.ConstructorArguments.Length];
			for (int i = 0; i < attribute.ConstructorArguments.Length; i++) {
				var argument = attribute.ConstructorArguments[i];
				argumentsForCall[i] = new ArgumentForCall(ConvertToTuple(argument));
			}
			return new ArgumentMap(ImmutableArray.Create(argumentsForCall), ImmutableArray.CreateRange(Enumerable.Range(0, argumentsForCall.Length)));
		}

		private static ISymbol GetMember(INamedTypeSymbol type, string name) {
			while (type != null) {
				var current = type.GetMembers(name).FirstOrDefault();
				if (current != null)
					return current;
				type = type.BaseType;
			}
			return null;
		}

		public static IReadOnlyList<Tuple<ISymbol, Tuple<ITypeSymbol, object>>> GetNamedArgumentMap(this AttributeData attribute) {
			return ImmutableArray.CreateRange(attribute.NamedArguments.Select(a => Tuple.Create(GetMember(attribute.AttributeClass, a.Key), ConvertToTuple(a.Value))));
		}

		public static IMethodSymbol UnReduceIfExtensionMethod(this IMethodSymbol method) {
			if (method.ReducedFrom != null) {
				var typeArguments = method.TypeArguments;
				method = method.ReducedFrom;
				if (method.TypeParameters.Length > 0)
					method = method.Construct(typeArguments.ToArray());
			}
			return method;
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
			if (symbol is ITypeParameterSymbol)
				return localName;

			if (symbol is INamedTypeSymbol)
				localName = AppendTypeArguments(localName, ((INamedTypeSymbol)symbol).TypeArguments);
			else if (symbol is IMethodSymbol)
				localName = AppendTypeArguments(localName, ((IMethodSymbol)symbol).TypeArguments);

			if (symbol.ContainingType != null)
				return symbol.ContainingType.FullyQualifiedName() + "." + localName;
			else if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
				return symbol.ContainingNamespace.FullyQualifiedName() + "." + localName;
			else
				return localName;
		}

		public static IReadOnlyList<ITypeParameterSymbol> GetAllTypeParameters(this INamedTypeSymbol type) {
			var result = new List<ITypeParameterSymbol>();
			for (; type != null; type = type.ContainingType) {
				result.InsertRange(0, type.TypeParameters);
			}
			return result;
		}

		public static IReadOnlyList<ITypeSymbol> GetAllTypeArguments(this INamedTypeSymbol type) {
			var result = new List<ITypeSymbol>();
			for (; type != null; type = type.ContainingType) {
				result.InsertRange(0, type.TypeArguments);
			}
			return result;
		}

		public static bool IsAccessor(this ISymbol member) {
			var method = member as IMethodSymbol;
			return method != null && (method.MethodKind == MethodKind.PropertyGet || method.MethodKind == MethodKind.PropertySet || method.MethodKind == MethodKind.EventAdd || method.MethodKind == MethodKind.EventRemove || method.MethodKind == MethodKind.EventRaise);
		}

		public static bool CallsAreOmitted(this IMethodSymbol method, SyntaxTree syntaxTree) {
			var mi = method.GetType().GetMethod("CallsAreOmitted", BindingFlags.Instance | BindingFlags.NonPublic);
			return (bool)mi.Invoke(method, new object[] { syntaxTree });
		}

		private static IEnumerable<INamedTypeSymbol> SelfAndNestedTypes(this INamedTypeSymbol type) {
			yield return type;
			foreach (var nested in type.GetTypeMembers().SelectMany(SelfAndNestedTypes))
				yield return nested;
		}

		public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol symbol) {
			foreach (var t in symbol.GetTypeMembers().SelectMany(SelfAndNestedTypes))
				yield return t;
			foreach (var nested in symbol.GetNamespaceMembers().SelectMany(GetAllTypes))
				yield return nested;
		}

		public static IEnumerable<INamedTypeSymbol> GetAllTypes(this IAssemblySymbol asm) {
			return GetAllTypes(asm.GlobalNamespace);
		}

		public static IEnumerable<INamedTypeSymbol> GetAllTypes(this Compilation c) {
			return c.References.Select(r => (IAssemblySymbol)c.GetAssemblyOrModuleSymbol(r)).Concat(new[] { c.Assembly }).SelectMany(GetAllTypes);
		}

		public static IEnumerable<IMethodSymbol> GetNonConstructorNonAccessorMethods(this ITypeSymbol type) {
			return type.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind != MethodKind.Constructor && m.MethodKind != MethodKind.StaticConstructor && !m.IsAccessor());
		}

		public static IEnumerable<IMethodSymbol> GetConstructors(this ITypeSymbol type) {
			return type.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Constructor);
		}

		public static IEnumerable<IPropertySymbol> GetProperties(this ITypeSymbol type) {
			return type.GetMembers().OfType<IPropertySymbol>();
		}

		public static IEnumerable<IFieldSymbol> GetFields(this ITypeSymbol type) {
			return type.GetMembers().OfType<IFieldSymbol>();
		}

		public static IEnumerable<IEventSymbol> GetEvents(this ITypeSymbol type) {
			return type.GetMembers().OfType<IEventSymbol>();
		}

		public static IEnumerable<ISymbol> GetNonAccessorNonTypeMembers(this ITypeSymbol type) {
			return type.GetMembers().Where(m => m is IMethodSymbol || m is IPropertySymbol || m is IFieldSymbol || m is IEventSymbol);
		}

		public static IEnumerable<ISymbol> FindImplementedInterfaceMembers(this ISymbol symbol) {
			return symbol.FindImplementedInterfaceMembers(symbol.ContainingType);
		}

		public static IEnumerable<ISymbol> FindImplementedInterfaceMembers(this ISymbol symbol, ITypeSymbol type) {
			IEnumerable<ISymbol> candidates;

			if (symbol is IMethodSymbol && ((IMethodSymbol)symbol).ExplicitInterfaceImplementations.Length > 0) {
				candidates = ((IMethodSymbol)symbol).ExplicitInterfaceImplementations;
			}
			else if (symbol is IPropertySymbol && ((IPropertySymbol)symbol).ExplicitInterfaceImplementations.Length > 0) {
				candidates = ((IPropertySymbol)symbol).ExplicitInterfaceImplementations;
			}
			else if (symbol is IEventSymbol && ((IEventSymbol)symbol).ExplicitInterfaceImplementations.Length > 0) {
				candidates = ((IEventSymbol)symbol).ExplicitInterfaceImplementations;
			}
			else {
				candidates = type.AllInterfaces.SelectMany(i => i.GetMembers(symbol.Name));
			}

			return candidates.Where(m => Equals(type.FindImplementationForInterfaceMember(m), symbol));
		}

		/// <summary>
		/// Returns the virtual or abstract method that ultimately declares a method (walks the OverriddenMethod chain until its end)
		/// </summary>
		public static IMethodSymbol DeclaringMethod(this IMethodSymbol method) {
			while (method.OverriddenMethod != null)
				method = method.OverriddenMethod;
			return method;
		}

		/// <summary>
		/// Returns the virtual or abstract property that ultimately declares a method (walks the OverriddenProperty chain until its end)
		/// </summary>
		public static IPropertySymbol DeclaringProperty(this IPropertySymbol property) {
			while (property.OverriddenProperty != null)
				property = property.OverriddenProperty;
			return property;
		}

		/// <summary>
		/// Returns the virtual or abstract event that ultimately declares a method (walks the OverriddenEvent chain until its end)
		/// </summary>
		public static IEventSymbol DeclaringEvent(this IEventSymbol evt) {
			while (evt.OverriddenEvent != null)
				evt = evt.OverriddenEvent;
			return evt;
		}

		public static ITypeSymbol ReturnType(this ISymbol symbol) {
			if (symbol is IEventSymbol)
				return ((IEventSymbol)symbol).Type;
			else if (symbol is IFieldSymbol)
				return ((IFieldSymbol)symbol).Type;
			else if (symbol is IPropertySymbol)
				return ((IPropertySymbol)symbol).Type;
			else if (symbol is IMethodSymbol)
				return ((IMethodSymbol)symbol).ReturnType;
			else
				return null;
		}

		private static IMethodSymbol FindAddMethod(ITypeSymbol type, IList<ITypeSymbol> parameterTypes) {
			while (type != null) {
				var result = type.GetMembers("Add").OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.Select(p => p.Type).SequenceEqual(parameterTypes));
				if (result != null)
					return result;
				type = type.BaseType;
			}
			return null;
		}

		#warning This method is hacky and should be removed once https://roslyn.codeplex.com/discussions/562451 is fixed
		public static IMethodSymbol GetCollectionInitializerSymbolInfoWorking(this SemanticModel semanticModel, ExpressionSyntax expression) {
			if (expression.Parent.CSharpKind() != SyntaxKind.CollectionInitializerExpression)
				return null;

			var orig = semanticModel.GetCollectionInitializerSymbolInfo(expression);
			if (orig.Symbol != null)
				return (IMethodSymbol)orig.Symbol;

			if (expression.Parent.Parent.CSharpKind() == SyntaxKind.SimpleAssignmentExpression) {
				var be = (BinaryExpressionSyntax)expression.Parent.Parent;
				var type = semanticModel.GetTypeInfo(be).ConvertedType;
				var arguments = (expression is InitializerExpressionSyntax ? ((InitializerExpressionSyntax)expression).Expressions.Select(x => semanticModel.GetTypeInfo(x).ConvertedType).ToArray() : new[] { semanticModel.GetTypeInfo(expression).ConvertedType });

				return FindAddMethod(type, arguments);
			}

			return null;
		}
	}
}
