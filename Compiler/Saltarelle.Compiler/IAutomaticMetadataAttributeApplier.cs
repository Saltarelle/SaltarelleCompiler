using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler {
	/// <summary>
	/// This interface can be implemented in a plugin in order to automatically apply metadata attributes to things. Classes implementing this interface should take an <see cref="IAttributeStore"/> as a dependency in order to make changes to the attributes.
	/// Code for classes implementing this interface will be run before the code in any attribute that extends <see cref="PluginAttributeBase"/>.
	/// </summary>
	public interface IAutomaticMetadataAttributeApplier {
		void Process(IAssembly assembly);
		void Process(ITypeDefinition type);
	}
}
