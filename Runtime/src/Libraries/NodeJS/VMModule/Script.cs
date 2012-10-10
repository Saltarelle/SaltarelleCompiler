using System.Runtime.CompilerServices;

namespace NodeJS.VMModule {
	[Imported]
	[ModuleName("vm")]
	[IgnoreNamespace]
	public class Script {
		private Script() {}

		public void RunInThisContext() {}

		public void RunInNewContext() {}

		public void RunInNewContext(dynamic sandbox) {}
	}
}