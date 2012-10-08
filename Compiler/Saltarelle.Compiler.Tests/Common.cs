using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;

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

        private static readonly Lazy<string> _linqScriptLazy = new Lazy<string>(() => File.ReadAllText(@"..\..\..\Runtime\bin\Script\linq.js"));
		internal static string LinqScript { get { return _linqScriptLazy.Value; } }

		public static Mock<ITypeDefinition> CreateTypeMock(string fullName) {
			int dot = fullName.LastIndexOf(".", StringComparison.InvariantCulture);
			string name;
			if (dot >= 0) {
				name = fullName.Substring(dot + 1);
			}
			else {
				name = fullName;
			}

			var result = new Mock<ITypeDefinition>(MockBehavior.Strict);
			result.SetupGet(_ => _.Name).Returns(name);
			result.SetupGet(_ => _.FullName).Returns(fullName);
			result.Setup(_ => _.GetDefinition()).Returns(result.Object);
			result.SetupGet(_ => _.DeclaringTypeDefinition).Returns((ITypeDefinition)null);
			result.SetupGet(_ => _.Region).Returns(DomRegion.Empty);
			return result;
		}
		
		public static ITypeDefinition CreateMockType(string fullName) {
			return CreateTypeMock(fullName).Object;
		}
	}
}
