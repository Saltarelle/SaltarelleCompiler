using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmbedAssemblies {
	/// <summary>
	/// This class is not used in the embedder program, but it is embedded into the output assembly. Don't add anything but static methods and fields to it.
	/// </summary>
	static class EmbeddedAssemblyLoader {
		private static bool trace;
		private static bool registered;

		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args) {
			var asmName = new AssemblyName(args.Name);
			if (trace)
				Console.WriteLine("Resolving " + asmName.Name + " requested by " + (args.RequestingAssembly != null ? args.RequestingAssembly.GetName().Name : "unknown assembly"));
			foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies()) {
				if (loaded.GetName().Name == asmName.Name) {
					if (trace)
						Console.WriteLine("... already loaded");
					return loaded;
				}
			}

			using (var asmStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(asmName.Name + ".dll")) {
				if (asmStream != null) {
					byte[] asmContent = new BinaryReader(asmStream).ReadBytes((int)asmStream.Length), pdbContent = null;
					using (var pdbStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(asmName + ".pdb")) {
						if (pdbStream != null) {
							pdbContent = new BinaryReader(pdbStream).ReadBytes((int)pdbStream.Length);
						}
					}
					if (trace)
						Console.WriteLine("... loading from resource");
					return Assembly.Load(asmContent, pdbContent);
				}
			}
			if (trace)
				Console.WriteLine("... not found");
			return null;
		}

		public static void Register() {
			if (!registered) {
				var ad = AppDomain.CurrentDomain;
				trace = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("trace_assembly_load"));
				if (trace)
					Console.WriteLine("Initializing importer for AppDomain" + ad.FriendlyName + ", current assembly: " + Assembly.GetExecutingAssembly().GetName().Name);
				ad.AssemblyResolve += ResolveAssembly;

				registered = true;
			}
		}
	}
}
