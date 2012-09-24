using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Threading.Tasks {
	public enum TaskStatus {
		Created,
		WaitingForActivation,
		WaitingToRun,
		Running,
		WaitingForChildrenToComplete,
		RanToCompletion,
		Canceled,
		Faulted,
	}

	[Imported(IsRealType = true)]
	[ScriptNamespace("ss")]
	public class Task : IDisposable {
		internal Task() {
		}

		[IntrinsicProperty]
		public Exception Exception { get { return null; } }
		public bool IsCanceled { [ScriptName("isCanceled")] get { return false; } }
		public bool IsCompleted { [ScriptName("isCompleted")] get { return false; } }
		public bool IsFaulted { [ScriptName("isFaulted")] get { return false; } }
		[IntrinsicProperty]
		public TaskStatus Status { get { return TaskStatus.Created; } }

		public Task ContinueWith(Action<Task> continuationAction) {
			return null;
		}

		[IgnoreGenericArguments]
		public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction) {
			return null;
		}

		[NonScriptable]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Tasks are always started.")]
		public void Start() {
		}

		public void Dispose() {
		}

		public static Task Delay(int millisecondDelay) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<TResult> FromResult<TResult>(TResult result) {
			return null;
		}

		public static Task Run(Action action) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<TResult> Run<TResult>(Func<TResult> function) {
			return null;
		}

		public static Task WhenAll(params Task[] tasks) {
			return null;
		}

		public static Task WhenAll(IEnumerable<Task> tasks) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) {
			return null;
		}

		public static Task<Task> WhenAny(params Task[] tasks) {
			return null;
		}

		public static Task<Task> WhenAny(IEnumerable<Task> tasks) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks) {
			return null;
		}

		public static Task FromPromise(IPromise promise) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<TResult>(IPromise<TResult> promise) {
			return null;
		}
	}

	[Imported(IsRealType = true)]
	[ScriptNamespace("ss")]
	[IgnoreGenericArguments]
	public class Task<TResult> : Task {
		public TResult Result { [ScriptName("getResult")] get { return default(TResult); } }

		public Task ContinueWith(Action<Task<TResult>> continuationAction) {
			return null;
		}

		[IgnoreGenericArguments]
		public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction) {
			return null;
		}
	}
}
