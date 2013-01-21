// Delegate.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System {

	[Imported(ObeysTypeSystem = true)]
	[IgnoreNamespace]
	[ScriptName("Function")]
	public abstract class Delegate {

		public static readonly Delegate Empty = null;

		protected Delegate(object target, string method) {
		}

		protected Delegate(Type target, string method) {
		}

		public static Delegate Combine(Delegate a, Delegate b) {
			return null;
		}

		[ScriptName("mkdel")]
		public static Delegate Create(object instance, Function f) {
			return null;
		}

		public static Delegate Remove(Delegate source, Delegate value) {
			return null;
		}

		public static Delegate Clone(Delegate source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action ThisFix<TThis>(Action<TThis> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action<T1> ThisFix<TThis, T1>(Action<TThis, T1> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action<T1, T2> ThisFix<TThis, T1, T2>(Action<TThis, T1, T2> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action<T1, T2, T3> ThisFix<TThis, T1, T2, T3>(Action<TThis, T1, T2, T3> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action<T1, T2, T3, T4> ThisFix<TThis, T1, T2, T3, T4>(Action<TThis, T1, T2, T3, T4> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action<T1, T2, T3, T4, T5> ThisFix<TThis, T1, T2, T3, T4, T5>(Action<TThis, T1, T2, T3, T4, T5> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action<T1, T2, T3, T4, T5, T6> ThisFix<TThis, T1, T2, T3, T4, T5, T6>(Action<TThis, T1, T2, T3, T4, T5, T6> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Action<T1, T2, T3, T4, T5, T6, T7> ThisFix<TThis, T1, T2, T3, T4, T5, T6, T7>(Action<TThis, T1, T2, T3, T4, T5, T6, T7> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1> ThisFix<TThis, T1>(Func<TThis, T1> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1, T2> ThisFix<TThis, T1, T2>(Func<TThis, T1, T2> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1, T2, T3> ThisFix<TThis, T1, T2, T3>(Func<TThis, T1, T2, T3> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1, T2, T3, T4> ThisFix<TThis, T1, T2, T3, T4>(Func<TThis, T1, T2, T3, T4> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1, T2, T3, T4, T5> ThisFix<TThis, T1, T2, T3, T4, T5>(Func<TThis, T1, T2, T3, T4, T5> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1, T2, T3, T4, T5, T6> ThisFix<TThis, T1, T2, T3, T4, T5, T6>(Func<TThis, T1, T2, T3, T4, T5, T6> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1, T2, T3, T4, T5, T6, T7> ThisFix<TThis, T1, T2, T3, T4, T5, T6, T7>(Func<TThis, T1, T2, T3, T4, T5, T6, T7> source) {
			return null;
		}

		/// <summary>
		/// This method will return a delegate that (when called) will call another delegate, with the JavaScript 'this' passed as the first parameter, and 'this' being as expected by the C# code. This is useful when dealing with jQuery.
		/// </summary>
		[IgnoreGenericArguments]
		public static Func<T1, T2, T3, T4, T5, T6, T7, T8> ThisFix<TThis, T1, T2, T3, T4, T5, T6, T7, T8>(Func<TThis, T1, T2, T3, T4, T5, T6, T7, T8> source) {
			return null;
		}

		[IntrinsicOperator]
		public static bool operator==(Delegate a, Delegate b) {
			return false;
		}

		[IntrinsicOperator]
		public static bool operator!=(Delegate a, Delegate b) {
			return false;
		}
	}
}
