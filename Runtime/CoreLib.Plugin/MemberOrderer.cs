using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler;

namespace CoreLib.Plugin {
	/// <summary>
	/// Used to deterministically order members. It is assumed that all members belong to the same type.
	/// </summary>
	public class MemberOrderer : IComparer<ISymbol> {
		public static readonly MemberOrderer Instance = new MemberOrderer();

		private MemberOrderer() {
		}

		private int CompareMethods(IMethodSymbol x, IMethodSymbol y) {
			if (x.MethodKind == MethodKind.Ordinary && y.MethodKind != MethodKind.Ordinary)
				return -1;
			if (x.MethodKind != MethodKind.Ordinary && y.MethodKind == MethodKind.Ordinary)
				return 1;

			if (x.Arity < y.Arity)
				return -1;
			if (x.Arity > y.Arity)
				return 1;

			int result = String.CompareOrdinal(x.MetadataName, y.MetadataName);
			if (result != 0)
				return result;
			if (x.Parameters.Length > y.Parameters.Length)
				return 1;
			else if (x.Parameters.Length < y.Parameters.Length)
				return -1;

			var xparms = String.Join(",", x.Parameters.Select(p => p.Type.FullyQualifiedName()));
			var yparms = String.Join(",", y.Parameters.Select(p => p.Type.FullyQualifiedName()));

			var presult = String.CompareOrdinal(xparms, yparms);
			if (presult != 0)
				return presult;

			var rresult = String.CompareOrdinal(x.ReturnType.FullyQualifiedName(), y.ReturnType.FullyQualifiedName());
			if (rresult != 0)
				return rresult;

			return 0;
		}

		private int CompareProperties(IPropertySymbol x, IPropertySymbol y) {
			if (x.Parameters.Length > y.Parameters.Length)
				return 1;
			else if (x.Parameters.Length < y.Parameters.Length)
				return -1;

			int result = String.CompareOrdinal(x.MetadataName, y.MetadataName);
			if (result != 0)
				return result;

			var xparms = String.Join(",", x.Parameters.Select(p => p.Type.FullyQualifiedName()));
			var yparms = String.Join(",", y.Parameters.Select(p => p.Type.FullyQualifiedName()));

			var presult = String.CompareOrdinal(xparms, yparms);
			if (presult != 0)
				return presult;

			return 0;
		}

		private int Publicity(ISymbol s) {
			switch (s.DeclaredAccessibility) {
				case Accessibility.Public:
					return 1;
				case Accessibility.Protected:
				case Accessibility.ProtectedOrInternal:
					return 2;
				case Accessibility.Internal:
				case Accessibility.ProtectedAndInternal:
					return 3;
				default:
					return 4;
			}
		}

		public int Compare(ISymbol x, ISymbol y) {
			var px = Publicity(x);
			var py = Publicity(y);
			if (px < py)
				return -1;
			if (px > py)
				return 1;

			if (x is IMethodSymbol) {
				if (y is IMethodSymbol) {
					return CompareMethods((IMethodSymbol)x, (IMethodSymbol)y);
				}
				else
					return -1;
			}
			else if (y is IMethodSymbol) {
				return 1;
			}

			if (x is IPropertySymbol) {
				if (y is IPropertySymbol) {
					return CompareProperties((IPropertySymbol)x, (IPropertySymbol)y);
				}
				else 
					return -1;
			}
			else if (y is IPropertySymbol) {
				return 1;
			}

			if (x is IFieldSymbol) {
				if (y is IFieldSymbol) {
					return String.CompareOrdinal(x.MetadataName, y.MetadataName);
				}
				else 
					return -1;
			}
			else if (y is IFieldSymbol) {
				return 1;
			}

			if (x is IEventSymbol) {
				if (y is IEventSymbol) {
					return String.CompareOrdinal(x.MetadataName, y.MetadataName);
				}
				else 
					return -1;
			}
			else if (y is IEventSymbol) {
				return 1;
			}

			throw new ArgumentException("Invalid member type" + x.GetType().FullName);
		}
	}
}