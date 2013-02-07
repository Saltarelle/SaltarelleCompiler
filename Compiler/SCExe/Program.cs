using System;
using System.IO;
using System.Reflection;

namespace Saltarelle.Compiler {
	public static class Program {
		public class AppDomainInitializer : MarshalByRefObject {
			public void DoIt() {
				// We need not do anything because we have a module initializer that will do the work for us.
			}
		}

		private static AppDomain CreateAppDomain() {
			var setup = new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(typeof(Program).Assembly.Location) };
			var ad = AppDomain.CreateDomain("SCTask", null, setup);
			var initializer = (AppDomainInitializer)ad.CreateInstanceAndUnwrap(typeof(AppDomainInitializer).Assembly.FullName, typeof(AppDomainInitializer).FullName);
			initializer.DoIt();
			return ad;
		}

		public static int Main(string[] args) {
			new AppDomainInitializer().DoIt();
			var asm = Assembly.Load("SCExeWorker");
			var worker = asm.GetType("Saltarelle.Compiler.SCExe.Worker");
			return (int)worker.GetMethod("DoWork").Invoke(null, new object[] { args, new Func<AppDomain>(CreateAppDomain) });
		}
	}
}
