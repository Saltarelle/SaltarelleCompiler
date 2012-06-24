using System.Collections.Generic;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public interface IMetadataWriteBackEngine {
		ICollection<IAttribute> GetAttributes(ITypeDefinition type);
		ICollection<IAttribute> GetAttributes(IMember member);
		IAttribute CreateAttribute(IAssembly attributeAssembly, string attributeTypeName, IEnumerable<object> positionalArguments, IEnumerable<object> namedArguments);
	}
}