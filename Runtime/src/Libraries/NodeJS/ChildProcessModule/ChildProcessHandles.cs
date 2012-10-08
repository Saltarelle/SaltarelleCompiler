using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;

namespace NodeJS.ChildProcessModule {
	[Imported]
	[Serializable]
	public class ChildProcessHandles {
		[IntrinsicProperty] public Buffer Stdout { get; set; }
		[IntrinsicProperty] public Buffer Stderr { get; set; }
	}
}
