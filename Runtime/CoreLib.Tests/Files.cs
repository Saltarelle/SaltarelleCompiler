using System;
using System.IO;
using ICSharpCode.NRefactory.TypeSystem;

namespace CoreLib.Tests {
	internal class Files {
		public static readonly string MscorlibPath = Path.GetFullPath(@"mscorlib.dll");

		private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(MscorlibPath));
		internal static IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }
	}
}
