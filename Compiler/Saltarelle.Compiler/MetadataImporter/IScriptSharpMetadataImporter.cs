using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.MetadataImporter {
	public interface IScriptSharpMetadataImporter : IMetadataImporter {
		bool IsNamedValues(ITypeDefinition t);
		bool IsResources(ITypeDefinition t);
		bool IsSerializable(ITypeDefinition t);
		bool DoesTypeObeyTypeSystem(ITypeDefinition t);
		bool IsImported(ITypeDefinition t);

		bool IsMixin(ITypeDefinition t);

		/// <summary>
		/// Returns the name of a module that has to be 'require'd for the type. Returns null if the type lives in the global namespace.
		/// </summary>
		string GetModuleName(ITypeDefinition t);

		bool OmitNullableChecks { get; }
		bool OmitDowncasts { get; }

		/// <summary>
		/// The value of any [assembly: ModuleName] attribute.
		/// </summary>
		string MainModuleName { get; }

		/// <summary>
		/// Whether the module being compiled is an AMD module.
		/// </summary>
		bool IsAsyncModule { get; }
	}
}