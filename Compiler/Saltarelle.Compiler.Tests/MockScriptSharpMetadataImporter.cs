using System;
using ICSharpCode.NRefactory.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;

namespace Saltarelle.Compiler.Tests {
	public class MockScriptSharpMetadataImporter : MockNamingConventionResolver, IScriptSharpMetadataImporter {
		public MockScriptSharpMetadataImporter() {
			IsNamedVaules   = t => false;
			IsResources     = t => false;
			IsGlobalMethods = t => false;
			IsRecord        = t => false;
			IsImported      = t => false;
			GetMixinArg     = t => null;
		}

		public Func<ITypeDefinition, bool> IsNamedVaules { get; set; }
		public Func<ITypeDefinition, bool> IsResources { get; set; }
		public Func<ITypeDefinition, bool> IsGlobalMethods { get; set; }
		public Func<ITypeDefinition, bool> IsRecord { get; set; }
		public Func<ITypeDefinition, bool> IsImported { get; set; }
		public Func<ITypeDefinition, string> GetMixinArg { get; set; }

		bool IScriptSharpMetadataImporter.IsNamedValues(ITypeDefinition t) {
			return IsNamedVaules(t);
		}

		bool IScriptSharpMetadataImporter.IsResources(ITypeDefinition t) {
			return IsResources(t);
		}

		bool IScriptSharpMetadataImporter.IsGlobalMethods(ITypeDefinition t) {
			return IsGlobalMethods(t);
		}

		bool IScriptSharpMetadataImporter.IsRecord(ITypeDefinition t) {
			return IsRecord(t);
		}

		bool IScriptSharpMetadataImporter.IsImported(ITypeDefinition t) {
			return IsImported(t);
		}

		string IScriptSharpMetadataImporter.GetMixinArg(ITypeDefinition t) {
			return GetMixinArg(t);
		}
	}
}
