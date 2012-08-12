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
		bool IsRealType(ITypeDefinition t);

		/// <summary>
		/// If the type has a [MixinAttribute], returns the argument to that attribute.
		/// Otherwise, if the type has a [GlobalMethodsAttribute], returns an empty string.
		/// Otherwise returns null.
		/// </summary>
		string GetGlobalMethodsPrefix(ITypeDefinition t);

		bool IsTestFixture(ITypeDefinition t);

		/// <summary>
		/// Returns null for methods that are not test methods.
		/// </summary>
		TestMethodData GetTestData(IMethod m);
	}
}