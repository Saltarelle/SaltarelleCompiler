using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace System {
	public static class ArrayExtensions {
		[InlineCode("{$System.Script}.contains({array}, {item})")]
		public static bool Contains<T>(this T[] array, T item) { return false; }

		[InlineCode("{$System.Script}.arrayClone({array})")]
		public static T[] Clone<T>(this T[] array) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static T[] Concat<T>(this T[] array, params T[] items) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static bool Every<T>(this T[] array, Func<T, int, T[], bool> callback) { return false; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static bool Every<T>(this T[] array, Func<T, bool> callback) { return false; }

		[InlineCode("{$System.Script}.arrayExtract({array}, {start})")]
		public static T[] Extract<T>(this T[] array, int start) { return null; }

		[InlineCode("{$System.Script}.arrayExtract({array}, {start}, {count})")]
		public static T[] Extract<T>(this T[] array, int start, int count) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static T[] Slice<T>(this T[] array, int start) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static T[] Slice<T>(this T[] array, int start, int end) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static T[] Filter<T>(this T[] array, Func<T, int, T[], bool> callback) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static T[] Filter<T>(this T[] array, Func<T, bool> callback) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static void ForEach<T>(this T[] array, Action<T, int, T[]> callback) {}

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static void ForEach<T>(this T[] array, Action<T> callback) {}

		[InlineCode("{$System.Script}.indexOf({array}, {item})")]
		public static int IndexOf<T>(this T[] array, T item) { return 0; }

		[InlineCode("{$System.Script}.indexOfArray({array}, {item}, {startIndex})")]
		public static int IndexOf<T>(this T[] array, T item, int startIndex) { return 0; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static TResult[] Map<TSource, TResult>(this TSource[] array, Func<TSource, int, TSource[], TResult> callback) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static TResult[] Map<TSource, TResult>(this TSource[] array, Func<TSource, TResult> callback) { return null; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static bool Some<T>(this T[] array, Func<T, int, T[], bool> callback) { return false; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static bool Some<T>(this T[] array, Func<T, bool> callback) { return false; }

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static void Sort<T>(this T[] array) {}

		[InstanceMethodOnFirstArgument, IncludeGenericArguments(false)]
		public static void Sort<T>(this T[] array, Func<T, T, int> compareCallback) {}
	}
}
