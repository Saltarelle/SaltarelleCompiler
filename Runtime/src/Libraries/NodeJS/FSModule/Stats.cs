using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NodeJS.FSModule {
	[Imported]
	[ModuleName("fs")]
	[IgnoreNamespace]
	public class Stats {
		private Stats() {}
		
		public bool IsFile() { return false; }
		public bool IsDirectory() { return false; }
		public bool IsBlockDevice() { return false; }
		public bool IsCharacterDevice() { return false; }
		public bool IsSymbolicLink() { return false; }
		public bool IsFIFO() { return false; }
		public bool IsSocket() { return false; }
		[IntrinsicProperty] public int Dev { get; private set; }
		[IntrinsicProperty] public int Ino { get; private set; }
		[IntrinsicProperty] public int Mode { get; private set; }
		[IntrinsicProperty] public int Nlink { get; private set; }
		[IntrinsicProperty] public int Uid { get; private set; }
		[IntrinsicProperty] public int Gid { get; private set; }
		[IntrinsicProperty] public int Rdev { get; private set; }
		[IntrinsicProperty] public int Size { get; private set; }
		[IntrinsicProperty] public int Blksize { get; private set; }
		[IntrinsicProperty] public int Blocks { get; private set; }
		[IntrinsicProperty, ScriptName("atime")] public DateTime ATime { get; private set; }
		[IntrinsicProperty, ScriptName("mtime")] public DateTime MTime { get; private set; }
		[IntrinsicProperty, ScriptName("ctime")] public DateTime CTime { get; private set; }
	}
}