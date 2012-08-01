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

	public interface IScriptSharpMetadataImporter : INamingConventionResolver {
		bool IsNamedValues(ITypeDefinition t);
		bool IsResources(ITypeDefinition t);
		bool IsGlobalMethods(ITypeDefinition t);
		bool IsSerializable(ITypeDefinition t);
		bool IsRealType(ITypeDefinition t);

		/// <summary>
		/// Returns the argument supplied to a [Mixin] attribute constructor, or null if no such attribute was specified.
		/// </summary>
		string GetMixinArg(ITypeDefinition t);

		bool IsTestFixture(ITypeDefinition t);

		/// <summary>
		/// Returns null for methods that are not test methods.
		/// </summary>
		TestMethodData GetTestData(IMethod m);
	}
}