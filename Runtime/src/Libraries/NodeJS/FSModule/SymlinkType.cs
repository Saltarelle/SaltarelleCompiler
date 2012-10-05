using System.Runtime.CompilerServices;

namespace NodeJS.FSModule {
	[NamedValues]
	[Imported]
	public enum SymlinkType {
		Dir,
		File,
		Junction,
	}
}