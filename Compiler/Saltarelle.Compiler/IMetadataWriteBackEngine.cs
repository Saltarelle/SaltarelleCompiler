using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	public interface IMetadataWriteBackEngine {
		/// <summary>
		/// Get the attributes collection for a type. This collection can be modified.
		/// </summary>
		ICollection<IAttribute> GetAttributes(ITypeDefinition type);

		/// <summary>
		/// Get the attributes collection for a member. This collection can be modified.
		/// </summary>
		ICollection<IAttribute> GetAttributes(IMember member);

		/// <summary>
		/// Create a new attribute. This attribute can then be added to the collections returned by either <c>GetAttributes</c> member.
		/// </summary>
		/// <param name="attributeAssembly">Assembly containing the attribute. Can be null, in which case all assemblies are searched for a matching attribute.</param>
		/// <param name="attributeTypeName">Name of the attribute type to create an instance of (reflection name).</param>
		/// <param name="positionalArguments">Positional arguments to the constructor, and their corresponding types.</param>
		/// <param name="namedArguments">Named arguments for the attribute.</param>
		IAttribute CreateAttribute(IAssembly attributeAssembly, string attributeTypeName, IList<Tuple<IType, object>> positionalArguments, IList<Tuple<string, object>> namedArguments);
	}
}