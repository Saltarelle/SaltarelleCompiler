using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.MetadataImporter {
	public interface IScriptSharpMetadataImporter : INamingConventionResolver {
		bool IsNamedValues(ITypeDefinition t);
		bool IsResources(ITypeDefinition t);
		bool IsGlobalMethods(ITypeDefinition t);
		bool IsRecord(ITypeDefinition t);
		bool IsImported(ITypeDefinition t);
		string GetMixinArg(ITypeDefinition t);
	}
}