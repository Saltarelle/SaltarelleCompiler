using System.Runtime.CompilerServices;

namespace NodeJS.FSModule {
	[Imported]
	[NamedValues]
	[IgnoreNamespace]
	public enum OpenFlags {
		[ScriptName("r")]   Read,
		[ScriptName("r+")]  ReadWriteRequireExists,
		[ScriptName("rs")]  ReadSynchronous,
		[ScriptName("rs+")] ReadWriteSynchronous,
		[ScriptName("w")]   Write,
		[ScriptName("wx")]  WriteExclusive,
		[ScriptName("w+")]  ReadWriteCreateIfNotExists,
		[ScriptName("wx+")] ReadWriteExclusiveCreateIfNotExists,
		[ScriptName("a")]   Append,
		[ScriptName("ax")]  AppendExclusive,
		[ScriptName("a+")]  AppendRead,
		[ScriptName("ax+")] AppendReadExclusive,
	}
}