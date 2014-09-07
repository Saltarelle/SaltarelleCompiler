using System;
using System.IO;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests {
	internal static class Files {
		public static readonly string MscorlibPath = Path.GetFullPath(@"mscorlib.dll");

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }

		private static readonly Lazy<IMetadataImporter> _referenceMetadataImporterLazy = new Lazy<IMetadataImporter>(() => new ReferenceMetadataImporter(new MockErrorReporter()));
		internal static IMetadataImporter ReferenceMetadataImporter { get { return _referenceMetadataImporterLazy.Value; } }
	}
}
