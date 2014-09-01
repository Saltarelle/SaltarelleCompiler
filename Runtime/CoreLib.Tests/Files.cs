using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace CoreLib.Tests {
	internal static class Files {
		public static readonly string MscorlibPath = Path.GetFullPath(@"mscorlib.dll");

		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => LoadAssemblyFile(MscorlibPath));
		internal static MetadataReference Mscorlib { get { return _mscorlibLazy.Value; } }

		public static MetadataReference LoadAssemblyFile(string path) {
			return new MetadataFileReference(path);
		}
	}
}
