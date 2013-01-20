using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
	/// <summary>
	/// Represents a CommonJS Promise/A object. All the handlers can receive 0 or more arguments.
	/// </summary>
	[Imported]
	public interface IPromise {
		[PreserveName]
		void Then(Delegate fulfilledHandler);
		[PreserveName]
		void Then(Delegate fulfilledHandler, Delegate errorHandler);
		[PreserveName]
		void Then(Delegate fulfilledHandler, Delegate errorHandler, Delegate progressHandler);
	}

	public static class PromiseExtensions {
		[InlineCode("{$System.Threading.Tasks.Task}.fromPromise({promise}))")]
		public static TaskAwaiter<object[]> GetAwaiter(this IPromise promise) {
			return null;
		}
	}
}
