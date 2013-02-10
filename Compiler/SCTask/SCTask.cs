using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Saltarelle.Compiler.SCTask {
	public class SCTask : Task {
		public class AppDomainInitializer : MarshalByRefObject {
			public void DoIt() {
				// The module initializer seems to not work in all cases on mono.
				var t = typeof(SCTask).Assembly.GetType("EmbedAssemblies.EmbeddedAssemblyLoader");
				var m = t.GetMethod("Register", BindingFlags.Static | BindingFlags.Public);
				m.Invoke(null, new object[0]);
			}
		}

		private AppDomain CreateAppDomain() {
			var setup = new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(typeof(SCTask).Assembly.Location) };
			var ad = AppDomain.CreateDomain("SCTask", null, setup);
			var initializer = (AppDomainInitializer)ad.CreateInstanceAndUnwrap(typeof(AppDomainInitializer).Assembly.FullName, typeof(AppDomainInitializer).FullName);
			initializer.DoIt();
			return ad;
		}

		public override bool Execute() {
			new AppDomainInitializer().DoIt();
			var asm = Assembly.Load("SCTaskWorker");
			var worker = asm.GetType("Saltarelle.Compiler.SCTask.Worker");
			return (bool)worker.GetMethod("DoWork").Invoke(null, new object[] { this, new Func<AppDomain>(CreateAppDomain) });
		}

		public string KeyContainer { get; set; }

		public string KeyFile { get; set; }

		public string[] AdditionalLibPaths { get; set; }

		public string DefineConstants { get; set; }

		public string DisabledWarnings { get; set; }

		public string DocumentationFile { get; set; }

		public bool EmitDebugInformation { get; set; }

		public string OutputAssembly { get; set; }

		public string OutputScript { get; set; }

		public ITaskItem[] References { get; set; }

		public ITaskItem[] Sources { get; set; }

		public bool TreatWarningsAsErrors { get; set; }

		public int WarningLevel { get; set; }

		public string WarningsAsErrors { get; set; }

		public string WarningsNotAsErrors { get; set; }

		public string TargetType { get; set; }

		public string MainEntryPoint { get; set; }

		public bool AlreadyCompiled { get; set; }
	}
}
