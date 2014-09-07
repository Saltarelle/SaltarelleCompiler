using System;
using System.IO;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using Saltarelle.Compiler;

namespace CoreLib.Tests {
	internal static class Files {
		public static readonly string MscorlibPath = Path.GetFullPath(@"mscorlib.dll");

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }
	}
}
