using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Threading.Tasks {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class TaskCanceledException : OperationCanceledException {
		public TaskCanceledException() {
		}

		public TaskCanceledException(string message) {
		}

		[InlineCode("new {$System.Threading.Tasks.TaskCanceledException}(null, {task})")]
		public TaskCanceledException(Task task) {
		}

		[InlineCode("new {$System.Threading.Tasks.TaskCanceledException}({message}, null, {innerException})")]
		public TaskCanceledException(string message, Exception innerException) {
		}

		[IntrinsicProperty]
		public Task Task { get; private set; }
	}
}
