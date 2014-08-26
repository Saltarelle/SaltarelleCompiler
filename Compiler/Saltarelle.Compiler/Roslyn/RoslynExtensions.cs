using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
			var type = semanticModel.GetTypeInfo(operatorNode);
			if (!type.Type.IsNullable())
				return false;
			var symbol = semanticModel.GetSymbolInfo(operatorNode).Symbol as IMethodSymbol;
			return symbol != null && !symbol.ReturnType.IsNullable();
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
	}
}
