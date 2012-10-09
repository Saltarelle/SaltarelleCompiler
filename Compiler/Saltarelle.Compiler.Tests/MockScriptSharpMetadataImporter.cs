using System;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;

namespace Saltarelle.Compiler.Tests {
	public class MockScriptSharpMetadataImporter : MockMetadataImporter, IScriptSharpMetadataImporter {
		public MockScriptSharpMetadataImporter() {
			IsNamedVaules          = t => false;
			IsResources            = t => false;
			IsSerializable         = t => false;
			IsRealType             = t => true;
			IsImported             = t => false;
			IsMixin                = t => false;
			IsTestFixture          = t => false;
			GetTestData            = m => null;
			GetModuleName          = t => null;
		}

		public Func<ITypeDefinition, bool> IsNamedVaules { get; set; }
		public Func<ITypeDefinition, bool> IsResources { get; set; }
		public Func<ITypeDefinition, bool> IsSerializable { get; set; }
		public Func<ITypeDefinition, bool> IsRealType { get; set; }
		public Func<ITypeDefinition, bool> IsImported { get; set; }
		public Func<ITypeDefinition, bool> IsMixin { get; set; }
		public Func<ITypeDefinition, bool> IsTestFixture { get; set; }
		public Func<IMethod, TestMethodData> GetTestData { get; set; }
		public Func<ITypeDefinition, string> GetModuleName { get; set; }

		bool IScriptSharpMetadataImporter.IsNamedValues(ITypeDefinition t) {
			return IsNamedVaules(t);
		}

		bool IScriptSharpMetadataImporter.IsResources(ITypeDefinition t) {
			return IsResources(t);
		}

		bool IScriptSharpMetadataImporter.IsSerializable(ITypeDefinition t) {
			return IsSerializable(t);
		}

		bool IScriptSharpMetadataImporter.IsRealType(ITypeDefinition t) {
			return IsRealType(t);
		}

		bool IScriptSharpMetadataImporter.IsImported(ITypeDefinition t) {
			return IsImported(t);
		}

		bool IScriptSharpMetadataImporter.IsMixin(ITypeDefinition t) {
			return IsMixin(t);
		}

		bool IScriptSharpMetadataImporter.IsTestFixture(ITypeDefinition t) {
			return IsTestFixture(t);
		}

		TestMethodData IScriptSharpMetadataImporter.GetTestData(IMethod m) {
			return GetTestData(m);
		}

		string IScriptSharpMetadataImporter.GetModuleName(ITypeDefinition t) {
			return GetModuleName(t);
		}

		public bool OmitDowncasts { get; set; }
		public bool OmitNullableChecks { get; set; }
		public string MainModuleName { get; set; }
	}
}
