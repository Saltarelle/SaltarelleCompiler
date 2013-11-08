using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;

namespace CoreLib.Plugin {
	/// <summary>
	/// Used to deterministically order members. It is assumed that all members belong to the same type.
	/// </summary>
	public class MemberOrderer : IComparer<IMember> {
		public static readonly MemberOrderer Instance = new MemberOrderer();

		private MemberOrderer() {
		}

		private int CompareMethods(IMethod x, IMethod y) {
			int result = String.CompareOrdinal(x.Name, y.Name);
			if (result != 0)
				return result;
			if (x.Parameters.Count > y.Parameters.Count)
				return 1;
			else if (x.Parameters.Count < y.Parameters.Count)
				return -1;

			var xparms = String.Join(",", x.Parameters.Select(p => p.Type.FullName));
			var yparms = String.Join(",", y.Parameters.Select(p => p.Type.FullName));

			var presult = String.CompareOrdinal(xparms, yparms);
			if (presult != 0)
				return presult;

			var rresult = String.CompareOrdinal(x.ReturnType.FullName, y.ReturnType.FullName);
			if (rresult != 0)
				return rresult;

			if (x.TypeParameters.Count > y.TypeParameters.Count)
				return 1;
			else if (x.TypeParameters.Count < y.TypeParameters.Count)
				return -1;
				
			return 0;
		}

		public int Compare(IMember x, IMember y) {
			if (x is IMethod) {
				if (y is IMethod) {
					return CompareMethods((IMethod)x, (IMethod)y);
				}
				else
					return -1;
			}
			else if (y is IMethod) {
				return 1;
			}

			if (x is IProperty) {
				if (y is IProperty) {
					return String.CompareOrdinal(x.Name, y.Name);
				}
				else 
					return -1;
			}
			else if (y is IProperty) {
				return 1;
			}

			if (x is IField) {
				if (y is IField) {
					return String.CompareOrdinal(x.Name, y.Name);
				}
				else 
					return -1;
			}
			else if (y is IField) {
				return 1;
			}

			if (x is IEvent) {
				if (y is IEvent) {
					return String.CompareOrdinal(x.Name, y.Name);
				}
				else 
					return -1;
			}
			else if (y is IEvent) {
				return 1;
			}

			throw new ArgumentException("Invalid member type" + x.GetType().FullName);
		}
	}
}