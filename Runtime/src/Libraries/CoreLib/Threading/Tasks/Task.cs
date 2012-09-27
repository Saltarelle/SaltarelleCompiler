using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Threading.Tasks {
	public enum TaskStatus {
		Created,
		[NonScriptable, Obsolete("This enum member is not used in script", true), EditorBrowsable(EditorBrowsableState.Never)]
		WaitingForActivation,
		[NonScriptable, Obsolete("This enum member is not used in script", true), EditorBrowsable(EditorBrowsableState.Never)]
		WaitingToRun,
		Running,
		[NonScriptable, Obsolete("This enum member is not used in script", true), EditorBrowsable(EditorBrowsableState.Never)]
		WaitingForChildrenToComplete,
		RanToCompletion,
		Canceled,
		Faulted,
	}

	[Imported(IsRealType = true)]
	[ScriptNamespace("ss")]
	public class Task : IDisposable {
		[AlternateSignature]
		public Task(Action action) {
		}

		public Task(Action<object> action, object state) {
		}

		[IntrinsicProperty]
		public AggregateException Exception { get { return null; } }

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

		public void Start() {
		}

		[ScriptSkip]
		public TaskAwaiter GetAwaiter() {
			return null;
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

		[InlineCode("{$System.Threading.Tasks.Task}.whenAll({$System.Array}.fromEnumerable({tasks}))")]
		public static Task WhenAll(IEnumerable<Task> tasks) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks) {
			return null;
		}

		[InlineCode("{$System.Threading.Tasks.Task}.whenAll({$System.Array}.fromEnumerable({tasks}))")]
		public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) {
			return null;
		}

		public static Task<Task> WhenAny(params Task[] tasks) {
			return null;
		}

		[InlineCode("{$System.Threading.Tasks.Task}.whenAny({$System.Array}.fromEnumerable({tasks}))")]
		public static Task<Task> WhenAny(IEnumerable<Task> tasks) {
			return null;
		}

		[IgnoreGenericArguments]
		public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks) {
			return null;
		}

		[IgnoreGenericArguments]
		[InlineCode("{$System.Threading.Tasks.Task}.whenAny({$System.Array}.fromEnumerable({tasks}))")]
		public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks) {
			return null;
		}

		[ExpandParams]
		public static Task FromDoneCallback(object target, string method, int callbackIndex, params object[] otherArguments) {
			return null;
		}

		[ExpandParams, IgnoreGenericArguments]
		public static Task<T> FromDoneCallback<T>(object target, string method, int callbackIndex, params object[] otherArguments) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. The result of the promise is not accessible through the task. THe result of the task is the parameters passed to the onCompleted callback of the promise.
		/// </summary>
		public static Task<object[]> FromPromise(IPromise promise) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. The result of the task will be set to one of the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultArgIndex">Index of the argument in the onCompleted callback of the promise that will become the result of the task. negative values count from the back, positive values count from the front.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<TResult>(IPromise promise, int resultArgIndex) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<TResult>(IPromise promise, Func<TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, TResult>(IPromise promise, Func<T1, TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, T2, TResult>(IPromise promise, Func<T1, T2, TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, T2, T3, TResult>(IPromise promise, Func<T1, T2, T3, TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, T2, T3, T4, TResult>(IPromise promise, Func<T1, T2, T3, T4, TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, T2, T3, T4, T5, TResult>(IPromise promise, Func<T1, T2, T3, T4, T5, TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, T2, T3, T4, T5, T6, TResult>(IPromise promise, Func<T1, T2, T3, T4, T5, T6, TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, T2, T3, T4, T5, T6, T7, TResult>(IPromise promise, Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultFactory) {
			return null;
		}

		/// <summary>
		/// Creates a task from a promise. A factory method is used to create the result of the task based on the arguments to the onCompleted callback of the promise.
		/// </summary>
		/// <param name="promise">The promise to create a task from.</param>
		/// <param name="resultFactory">Factory method to create the result of the task from the arguments to the onCompleted callback.</param>
		[IgnoreGenericArguments]
		public static Task<TResult> FromPromise<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IPromise promise, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultFactory) {
			return null;
		}
	}

	[Imported(IsRealType = true)]
	[ScriptNamespace("ss")]
	[IgnoreGenericArguments]
	public class Task<TResult> : Task {
		[AlternateSignature]
		public Task(Func<TResult> function) : base(() => {}) {
		}

		public Task(Func<object, TResult> function, object state) : base(() => {}) {
		}

		public TResult Result { [ScriptName("getResult")] get { return default(TResult); } }

		public Task ContinueWith(Action<Task<TResult>> continuationAction) {
			return null;
		}

		[ScriptSkip]
		public new TaskAwaiter<TResult> GetAwaiter() {
			return null;
		}

		[IgnoreGenericArguments]
		public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction) {
			return null;
		}
	}
}
