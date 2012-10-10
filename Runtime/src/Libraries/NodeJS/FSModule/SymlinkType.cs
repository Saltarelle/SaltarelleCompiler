using System.Runtime.CompilerServices;

namespace NodeJS.FSModule {
	[Imported]
	[NamedValues]
	public enum SymlinkType {
		Dir,
		File,
		Junction,
	}
}