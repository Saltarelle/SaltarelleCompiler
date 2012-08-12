using System;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;

namespace Saltarelle.Compiler.Tests {
	public class MockScriptSharpMetadataImporter : MockMetadataImporter, IScriptSharpMetadataImporter {
		public MockScriptSharpMetadataImporter() {
			IsNamedVaules          = t => false;
			IsResources            = t => false;
			GetGlobalMethodsPrefix = t => null;
			IsSerializable         = t => false;
			IsRealType             = t => true;
			IsTestFixture          = t => false;
			GetTestData            = m => null;
		}

		public Func<ITypeDefinition, bool> IsNamedVaules { get; set; }
		public Func<ITypeDefinition, bool> IsResources { get; set; }
		public Func<ITypeDefinition, string> GetGlobalMethodsPrefix { get; set; }
		public Func<ITypeDefinition, bool> IsSerializable { get; set; }
		public Func<ITypeDefinition, bool> IsRealType { get; set; }
		public Func<ITypeDefinition, bool> IsTestFixture { get; set; }
		public Func<IMethod, TestMethodData> GetTestData { get; set; }

		bool IScriptSharpMetadataImporter.IsNamedValues(ITypeDefinition t) {
			return IsNamedVaules(t);
		}

		bool IScriptSharpMetadataImporter.IsResources(ITypeDefinition t) {
			return IsResources(t);
		}

		string IScriptSharpMetadataImporter.GetGlobalMethodsPrefix(ITypeDefinition t) {
			return GetGlobalMethodsPrefix(t);
		}

		bool IScriptSharpMetadataImporter.IsSerializable(ITypeDefinition t) {
			return IsSerializable(t);
		}

		bool IScriptSharpMetadataImporter.IsRealType(ITypeDefinition t) {
			return IsRealType(t);
		}

		bool IScriptSharpMetadataImporter.IsTestFixture(ITypeDefinition t) {
			return IsTestFixture(t);
		}

		TestMethodData IScriptSharpMetadataImporter.GetTestData(IMethod m) {
			return GetTestData(m);
		}
	}
}
