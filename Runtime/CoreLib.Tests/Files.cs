using System;
using System.IO;
using ICSharpCode.NRefactory.TypeSystem;

namespace CoreLib.Tests {
	internal class Files {
		public static readonly string MscorlibPath = Path.GetFullPath(@"mscorlib.dll");

		private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => LoadAssembly(MscorlibPath));
		internal static IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

		public static IAssemblyReference LoadAssembly(string path) {
			var l = new IkvmLoader { IncludeInternalMembers = true };
			return l.LoadAssemblyFile(path);
		}
	}
}
