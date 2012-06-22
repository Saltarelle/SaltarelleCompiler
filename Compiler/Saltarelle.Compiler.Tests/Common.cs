using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;

namespace Saltarelle.Compiler.Tests {
	internal class Common {
		public static readonly string SSMscorlibPath = Path.GetFullPath(@"..\..\..\ScriptSharp\bin\mscorlib.dll");

        private static readonly Lazy<IAssemblyReference> _ssMscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(SSMscorlibPath));
        internal static IAssemblyReference SSMscorlib { get { return _ssMscorlibLazy.Value; } }

    	private static readonly Lazy<IAssemblyReference> _mscorlibLazy = new Lazy<IAssemblyReference>(() => new CecilLoader() { IncludeInternalMembers = true }.LoadAssemblyFile(typeof(object).Assembly.Location));
        internal static IAssemblyReference Mscorlib { get { return _mscorlibLazy.Value; } }

        private static readonly Lazy<string> _ssMscorlibScriptLazy = new Lazy<string>(() => File.ReadAllText(@"..\..\..\ScriptSharp\bin\mscorlib.debug.js"));
		internal static string SSMscorlibScript { get { return _ssMscorlibScriptLazy.Value; } }
	}
}
