using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public static class AttributeReader {
		[Obsolete("The methods in the AttributeReader class are obsolete. You should instead take a dependency on an IAttributeStore and use the AttributesFor() method.", true)]
		public static TAttribute ReadAttribute<TAttribute>(IAttribute attr) where TAttribute : Attribute {
			throw new NotSupportedException();
		}

		[Obsolete("The methods in the AttributeReader class are obsolete. You should instead take a dependency on an IAttributeStore and use the AttributesFor() method.", true)]
		public static TAttribute ReadAttribute<TAttribute>(IEntity entity) where TAttribute : Attribute {
			throw new NotSupportedException();
		}

		[Obsolete("The methods in the AttributeReader class are obsolete. You should instead take a dependency on an IAttributeStore and use the AttributesFor() method.", true)]
		public static TAttribute ReadAttribute<TAttribute>(IEnumerable<IAttribute> attributes) where TAttribute : Attribute {
			throw new NotSupportedException();
		}

		[Obsolete("The methods in the AttributeReader class are obsolete. You should instead take a dependency on an IAttributeStore and use the AttributesFor() method.", true)]
		public static IEnumerable<TAttribute> ReadAttributes<TAttribute>(IEnumerable<IAttribute> attributes) where TAttribute : Attribute {
			throw new NotSupportedException();
		}

		[Obsolete("The methods in the AttributeReader class are obsolete. You should instead take a dependency on an IAttributeStore and use the AttributesFor() method.", true)]
		public static bool HasAttribute<TAttribute>(IEntity entity) where TAttribute : Attribute {
			throw new NotSupportedException();
		}

		[Obsolete("The methods in the AttributeReader class are obsolete. You should instead take a dependency on an IAttributeStore and use the AttributesFor() method.", true)]
		public static bool HasAttribute<TAttribute>(IEnumerable<IAttribute> attributes) where TAttribute : Attribute {
			throw new NotSupportedException();
		}
	}
}
