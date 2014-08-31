using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Plugin {
	/// <summary>
	/// Used to deterministically order members. It is assumed that all members belong to the same type.
	/// </summary>
	public class MemberOrderer : IComparer<ISymbol> {
		public static readonly MemberOrderer Instance = new MemberOrderer();

		private MemberOrderer() {
		}

		private int CompareMethods(IMethodSymbol x, IMethodSymbol y) {
			int result = String.CompareOrdinal(x.Name, y.Name);
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

			if (x.TypeParameters.Length > y.TypeParameters.Length)
				return 1;
			else if (x.TypeParameters.Length < y.TypeParameters.Length)
				return -1;
				
			return 0;
		}

		public int Compare(ISymbol x, ISymbol y) {
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
					return String.CompareOrdinal(x.Name, y.Name);
				}
				else 
					return -1;
			}
			else if (y is IPropertySymbol) {
				return 1;
			}

			if (x is IFieldSymbol) {
				if (y is IFieldSymbol) {
					return String.CompareOrdinal(x.Name, y.Name);
				}
				else 
					return -1;
			}
			else if (y is IFieldSymbol) {
				return 1;
			}

			if (x is IEventSymbol) {
				if (y is IEventSymbol) {
					return String.CompareOrdinal(x.Name, y.Name);
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