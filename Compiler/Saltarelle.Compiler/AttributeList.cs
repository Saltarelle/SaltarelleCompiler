using System;
using System.Collections.Generic;

namespace Saltarelle.Compiler {
	public class AttributeList : List<Attribute> {
		public void ReplaceAttribute<TAttribute>(TAttribute attribute) where TAttribute : Attribute {
			RemoveAll(a => a is TAttribute);
			Add(attribute);
		}

		public void RemoveAttribute<TAttribute>() where TAttribute : Attribute {
			RemoveAll(a => a is TAttribute);
		}

		public bool HasAttribute<TAttribute>() where TAttribute : Attribute {
			foreach (var a in this) {
				if (a is TAttribute)
					return true;
			}
			return false;
		}

		public TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute {
			foreach (var a in this) {
				var ta = a as TAttribute;
				if (ta != null)
					return ta;
			}
			return null;
		}

		public IEnumerable<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute {
			foreach (var a in this) {
				var ta = a as TAttribute;
				if (ta != null)
					yield return ta;
			}
		}
	}
}