using System;
using System.IO;
using System.Reflection;

namespace Saltarelle.Compiler {
	public static class Program {
		private static void RegisterAssemblyLoader() {
			// The module initializer seems to not work in all cases on mono.
			var t = typeof(Program).Assembly.GetType("EmbedAssemblies.EmbeddedAssemblyLoader");
			var m = t.GetMethod("Register", BindingFlags.Static | BindingFlags.Public);
			m.Invoke(null, new object[0]);
		}

		public static int Main(string[] args) {
			RegisterAssemblyLoader();
			var asm = Assembly.Load("SCExeWorker");
			var worker = asm.GetType("Saltarelle.Compiler.SCExe.Worker");
			return (int)worker.GetMethod("DoWork").Invoke(null, new object[] { args });
		}
	}
}
