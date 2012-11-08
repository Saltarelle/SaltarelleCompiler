using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.MetadataImporter {
	public class TestMethodData {
		public string Description { get; private set; }
		public string Category { get; private set; }
		public int? ExpectedAssertionCount { get; private set; }
		public bool IsAsync { get; private set; }

		public TestMethodData(string description, string category, bool isAsync, int? expectedAssertionCount) {
			Description = description;
			Category = category;
			IsAsync = isAsync;
			ExpectedAssertionCount = expectedAssertionCount;
		}
	}

	public interface IScriptSharpMetadataImporter : IMetadataImporter {
		bool IsNamedValues(ITypeDefinition t);
		bool IsResources(ITypeDefinition t);
		bool IsSerializable(ITypeDefinition t);
		bool DoesTypeObeyTypeSystem(ITypeDefinition t);
		bool IsImported(ITypeDefinition t);

		bool IsMixin(ITypeDefinition t);

		bool IsTestFixture(ITypeDefinition t);

		/// <summary>
		/// Returns the name of a module that has to be 'require'd for the type. Returns null if the type lives in the global namespace.
		/// </summary>
		string GetModuleName(ITypeDefinition t);

		/// <summary>
		/// Returns null for methods that are not test methods.
		/// </summary>
		TestMethodData GetTestData(IMethod m);

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