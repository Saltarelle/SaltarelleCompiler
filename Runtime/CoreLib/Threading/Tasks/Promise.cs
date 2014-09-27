using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
	/// <summary>
	/// Represents a CommonJS Promise/A object. All the handlers can receive 0 or more arguments.
	/// </summary>
	[Imported]
	public interface IPromise {
		[PreserveName, OmitUnspecifiedArgumentsFrom(1)]
		void Then(Delegate fulfilledHandler, Delegate errorHandler = null, Delegate progressHandler = null);
	}

	public static class PromiseExtensions {
		[InlineCode("{$System.Threading.Tasks.Task}.fromPromise({promise})")]
		public static TaskAwaiter<object[]> GetAwaiter(this IPromise promise) {
			return null;
		}
	}
}
