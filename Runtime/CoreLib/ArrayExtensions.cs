using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	public static class ArrayExtensions {
		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static bool Contains<T>(this T[] array, T item) { return false; }

		[InlineCode("{$System.Script}.arrayClone({array})")]
		public static T[] Clone<T>(this T[] array) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static T[] Concat<T>(this T[] array, params T[] items) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static bool Every<T>(this T[] array, Func<T, int, T[], bool> callback) { return false; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static bool Every<T>(this T[] array, Func<T, bool> callback) { return false; }

		[InlineCode("{$System.Script}.arrayExtract({array}, {start})")]
		public static T[] Extract<T>(this T[] array, int start) { return null; }

		[InlineCode("{$System.Script}.arrayExtract({array}, {start}, {count})")]
		public static T[] Extract<T>(this T[] array, int start, int count) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static T[] Slice<T>(this T[] array, int start) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static T[] Slice<T>(this T[] array, int start, int end) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static T[] Filter<T>(this T[] array, Func<T, int, T[], bool> callback) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static T[] Filter<T>(this T[] array, Func<T, bool> callback) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static void ForEach<T>(this T[] array, Action<T, int, T[]> callback) {}

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static void ForEach<T>(this T[] array, Action<T> callback) {}

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static int IndexOf<T>(this T[] array, T item) { return 0; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static int IndexOf<T>(this T[] array, T item, int startIndex) { return 0; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static TResult[] Map<TSource, TResult>(this TSource[] array, Func<TSource, int, TSource[], TResult> callback) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static TResult[] Map<TSource, TResult>(this TSource[] array, Func<TSource, TResult> callback) { return null; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static bool Some<T>(this T[] array, Func<T, int, T[], bool> callback) { return false; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static bool Some<T>(this T[] array, Func<T, bool> callback) { return false; }

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static void Sort<T>(this T[] array) {}

		[InstanceMethodOnFirstArgument, IgnoreGenericArguments]
		public static void Sort<T>(this T[] array, Func<T, T, int> compareCallback) {}
	}
}
