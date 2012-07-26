using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.Tests {
	internal class Common {
		public static readonly string MscorlibPath = Path.GetFullPath(@"..\..\..\Runtime\bin\mscorlib.dll");
		public static readonly string LinqPath = Path.GetFullPath(@"..\..\..\Runtime\bin\Script.Linq.dll");

        private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(MscorlibPath));
        internal static IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

        private static readonly Lazy<IAssemblyReference> _linqLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(LinqPath));
        internal static IAssemblyReference Linq { get { return _linqLazy.Value; } }

        private static readonly Lazy<string> _mscorlibScriptLazy = new Lazy<string>(() => File.ReadAllText(@"..\..\..\Runtime\bin\Script\mscorlib.debug.js"));
		internal static string MscorlibScript { get { return _mscorlibScriptLazy.Value; } }
	}
}
