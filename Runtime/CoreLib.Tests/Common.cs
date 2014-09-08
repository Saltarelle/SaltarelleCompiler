using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests {
	internal static class Common {
		public static readonly string MscorlibPath = Path.GetFullPath(@"mscorlib.dll");

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }

		private static readonly Lazy<IMetadataImporter> _referenceMetadataImporterLazy = new Lazy<IMetadataImporter>(() => new ReferenceMetadataImporter(new MockErrorReporter()));
		internal static IMetadataImporter ReferenceMetadataImporter { get { return _referenceMetadataImporterLazy.Value; } }

		public static CSharpCompilation CreateCompilation(string source, IEnumerable<MetadataReference> references = null, IList<string> defineConstants = null) {
			return CreateCompilation(new[] { source }, references, defineConstants);
		}

		public static CSharpCompilation CreateCompilation(IEnumerable<string> sources, IEnumerable<MetadataReference> references = null, IList<string> defineConstants = null) {
			references = references ?? new[] { Common.Mscorlib };
			var defineConstantsArr = ImmutableArray.CreateRange(defineConstants ?? new string[0]);
			var syntaxTrees = sources.Select((s, i) => SyntaxFactory.ParseSyntaxTree(s, new CSharpParseOptions(LanguageVersion.CSharp5, DocumentationMode.None, SourceCodeKind.Regular, defineConstantsArr), "File" + i.ToString(CultureInfo.InvariantCulture) + ".cs"));
			var compilation = CSharpCompilation.Create("Test", syntaxTrees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var diagnostics = string.Join(Environment.NewLine, compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage()));
			if (!string.IsNullOrEmpty(diagnostics))
				Assert.Fail("Errors in source:" + Environment.NewLine + diagnostics);
			return compilation;
		}
	}
}
