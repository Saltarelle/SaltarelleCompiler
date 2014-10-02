using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Threading.Tasks {
	[Imported]
	[ScriptNamespace("ss")]
	[ScriptName("Task")]
	public class TaskAwaiter : INotifyCompletion {
		internal TaskAwaiter() {}

		[NonScriptable]
		public bool IsCompleted { get { return false; } }

		[ScriptName("onCompleted")]
		public void OnCompleted(Action continuation) {}

		[ScriptName("getAwaitedResult")]
		public void GetResult() {}
	}

	[Imported]
	[IncludeGenericArguments(false)]
	[ScriptNamespace("ss")]
	[ScriptName("Task")]
	public class TaskAwaiter<TResult> : INotifyCompletion {
		internal TaskAwaiter() {}

		[NonScriptable]
		public bool IsCompleted { get { return false; } }

		[ScriptName("onCompleted")]
		public void OnCompleted(Action continuation) {}

		[ScriptName("getAwaitedResult")]
		public TResult GetResult() { return default(TResult); }
	}
}
