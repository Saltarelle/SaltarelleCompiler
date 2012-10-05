using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;

namespace NodeJS.VMModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("vm")]
	public static class VM {
		public static void RunInThisContext(string code) {}
		public static void RunInThisContext(string code, string filename) {}

		public static void RunInNewContext(string code) {}
		public static void RunInNewContext(string code, dynamic sandbox) {}
		public static void RunInNewContext(string code, dynamic sandbox, string filename) {}

		public static void RunInContext(string code, Context context) {}
		public static void RunInContext(string code, Context context, string filename) {}

		public static Context CreateContext(dynamic initContext) {}

		public static Script CreateScript(string code) {}
		public static Script CreateScript(string code, string filename) {}
	}
}
