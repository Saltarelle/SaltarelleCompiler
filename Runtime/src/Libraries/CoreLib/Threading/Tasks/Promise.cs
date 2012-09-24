using System.Runtime.CompilerServices;

namespace System.Threading.Tasks {
	[Imported]
	public interface IPromise {
		[PreserveName]
		IPromise Then(Delegate doneHandler);
		[PreserveName]
		IPromise Then(Delegate doneHandler, Delegate failHandler);
		[PreserveName]
		IPromise Then(Delegate doneHandler, Delegate failHandler, Delegate progressHandler);
	}

	[Imported]
	[IgnoreGenericArguments]
	public interface IPromise<out TResult> {
		[PreserveName]
		void Then(Action<TResult> doneHandler);
		[PreserveName]
		void Then(Action<TResult> doneHandler, Delegate failHandler);
		[PreserveName]
		void Then(Action<TResult> doneHandler, Delegate failHandler, Delegate progressHandler);
	}
}
