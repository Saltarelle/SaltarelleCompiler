using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Linq {
	[Imported(ObeysTypeSystem = true)]
	[IgnoreNamespace]
	public static class Enumerable {
		#region Generators

		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Choice<TResult>(params TResult[] arguments) { return null; }

		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Cycle<TResult>(params TResult[] arguments) { return null; }

		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Empty<TResult>() { return null; }


		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> From<TResult>(IEnumerable<TResult> source) { return null; }

		[IgnoreGenericArguments]
		public static LinqJSEnumerable<string> From(string source) { return null; }

		
		public static LinqJSEnumerable<object> From(object arrayLikeObject) { return null; }

		
		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Make<TResult>(TResult element) { return null; }


		public static LinqJSEnumerable<RegexMatch> Matches(string input, Regex pattern) { return null; }

		public static LinqJSEnumerable<RegexMatch> Matches(string input, string pattern) { return null; }

		public static LinqJSEnumerable<RegexMatch> Matches(string input, string pattern, string flags) { return null; }


		public static LinqJSEnumerable<int> Range(int start, int count) { return null; }

		public static LinqJSEnumerable<int> Range(int start, int count, int step) { return null; }


		public static LinqJSEnumerable<int> RangeDown(int start, int count) { return null; }

		public static LinqJSEnumerable<int> RangeDown(int start, int count, int step) { return null; }


		public static LinqJSEnumerable<int> RangeTo(int start, int count) { return null; }

		public static LinqJSEnumerable<int> RangeTo(int start, int count, int step) { return null; }


		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Repeat<TResult>(TResult element) { return null; }

		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Repeat<TResult>(TResult element, int count) { return null; }


		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> RepeatWithFinalize<TResult>(Func<TResult> initializer, Action<TResult> finalizer) { return null; }


		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Generate<TResult>(Func<TResult> func) { return null; }

		[IgnoreGenericArguments]
		public static LinqJSEnumerable<TResult> Generate<TResult>(Func<TResult> func, int count) { return null; }


		public static LinqJSEnumerable<int> ToInfinity() { return null; }

		public static LinqJSEnumerable<int> ToInfinity(int start) { return null; }

		public static LinqJSEnumerable<int> ToInfinity(int start, int step) { return null; }


		public static LinqJSEnumerable<int> ToNegativeInfinity() { return null; }

		public static LinqJSEnumerable<int> ToNegativeInfinity(int start) { return null; }

		public static LinqJSEnumerable<int> ToNegativeInfinity(int start, int step) { return null; }

		#endregion

		#region Projection / filtering

		[InlineCode("{$System.Linq.Enumerable}.from({source}).traverseBreadthFirst({func})")]
		public static LinqJSEnumerable<TSource> TraverseBreadthFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> func) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).traverseBreadthFirst({func}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> TraverseBreadthFirst<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> func, Func<TSource, TResult> resultSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).traverseBreadthFirst({func}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> TraverseBreadthFirst<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> func, Func<TSource, int, TResult> resultSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).traverseDepthFirst({func})")]
		public static LinqJSEnumerable<TSource> TraverseDepthFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> func) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).traverseDepthFirst({func}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> TraverseDepthFirst<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> func, Func<TSource, TResult> resultSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).traverseDepthFirst({func}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> TraverseDepthFirst<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> func, Func<TSource, int, TResult> resultSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).flatten()")]
		public static LinqJSEnumerable<object> Flatten(this IEnumerable<object> source) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).pairwise({selector})")]
		public static LinqJSEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> selector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).scan({func})")]
		public static LinqJSEnumerable<TSource> Scan<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).scan({seed}, {func})")]
		public static LinqJSEnumerable<TAccumulate> Scan<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).scan({seed}, {func}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> Scan<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).select({selector})")]
		public static LinqJSEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).select({selector})")]
		public static LinqJSEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).selectMany({selector})")]
		public static LinqJSEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).selectMany({selector})")]
		public static LinqJSEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).selectMany({collectionSelector}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).selectMany({collectionSelector}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).select(function(x) {{ return {$System.Type}.cast(x, {TResult}); }})")]
		public static LinqJSEnumerable<TResult> Cast<TResult>(this IEnumerable source) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).ofType({TResult})")]
		public static LinqJSEnumerable<TResult> OfType<TResult>(this IEnumerable source) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).zip({other}, {selector})")]
		public static LinqJSEnumerable<TResult> Zip<TSource, TOther, TResult>(this IEnumerable<TSource> source, IEnumerable<TOther> other, Func<TSource, TOther, TResult> selector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).zip({other}, {selector})")]
		public static LinqJSEnumerable<TResult> Zip<TSource, TOther, TResult>(this IEnumerable<TSource> source, IEnumerable<TOther> other, Func<TSource, TOther, int, TResult> selector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).where({predicate})")]
		public static LinqJSEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).where({predicate})")]
		public static LinqJSEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) { return null; }

		#endregion

		#region Join

		[InlineCode("{$System.Linq.Enumerable}.from({outer}).join({inner}, {outerKeySelector}, {innerKeySelector}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({outer}).join({inner}, {outerKeySelector}, {innerKeySelector}, {resultSelector}, {compareSelector})")]
		public static LinqJSEnumerable<TResult> Join<TOuter, TInner, TKey, TCompare, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({outer}).groupJoin({inner}, {outerKeySelector}, {innerKeySelector}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({outer}).groupJoin({inner}, {outerKeySelector}, {innerKeySelector}, {resultSelector}, {compareSelector})")]
		public static LinqJSEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TCompare, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }

		#endregion

		#region Set methods

		[InlineCode("{$System.Linq.Enumerable}.from({source}).all({predicate})")]
		public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return false; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).any()")]
		public static bool Any<TSource>(this IEnumerable<TSource> source) { return false; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).any({predicate})")]
		public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return false; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).concat({other})")]
		public static LinqJSEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).insert({index}, {other})")]
		public static LinqJSEnumerable<TSource> Insert<TSource>(this IEnumerable<TSource> source, int index, IEnumerable<TSource> other) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).alternate({value})")]
		public static LinqJSEnumerable<TSource> Alternate<TSource>(this IEnumerable<TSource> source, TSource value) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).contains({value})")]
		public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) { return false; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).contains({value}, {compareSelector})")]
		public static bool Contains<TSource, TValue>(this IEnumerable<TSource> source, TValue value, Func<TSource, TValue> compareSelector) { return false; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).defaultIfEmpty({TSource}.getDefaultValue())")]
		public static LinqJSEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).defaultIfEmpty({defaultValue})")]
		public static LinqJSEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).distinct()")]
		public static LinqJSEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).distinct({compareSelector})")]
		public static LinqJSEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> compareSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).except({other})")]
		public static LinqJSEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).except({other}, {compareSelector})")]
		public static LinqJSEnumerable<TSource> Except<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TSource> other, Func<TSource, TKey> compareSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sequenceEqual({other})")]
		public static bool SequenceEqual<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other) { return false; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sequenceEqual({other}, {compareSelector})")]
		public static bool SequenceEqual<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TSource> other, Func<TSource, TKey> compareSelector) { return false; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).union({other})")]
		public static LinqJSEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).union({other}, {compareSelector})")]
		public static LinqJSEnumerable<TSource> Union<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TSource> other, Func<TSource, TKey> compareSelector) { return null; }

		#endregion

		#region Ordering

		[InlineCode("{$System.Linq.Enumerable}.from({source}).orderBy()")]
		public static OrderedLinqJSEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).orderBy({keySelector})")]
		public static OrderedLinqJSEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).orderByDescending()")]
		public static OrderedLinqJSEnumerable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).orderByDescending({keySelector})")]
		public static OrderedLinqJSEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).reverse()")]
		public static LinqJSEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).shuffle()")]
		public static LinqJSEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source) { return null; }

		#endregion

		#region Grouping

		[InlineCode("{$System.Linq.Enumerable}.from({source}).groupBy({keySelector})")]
		public static LinqJSEnumerable<Grouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).groupBy({keySelector}, {elementSelector})")]
		public static LinqJSEnumerable<Grouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).groupBy({keySelector}, {elementSelector}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).groupBy({keySelector}, {elementSelector}, {resultSelector}, {compareSelector})")]
		public static LinqJSEnumerable<TResult> GroupBy<TSource, TKey, TElement, TCompare, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).partitionBy({keySelector})")]
		public static LinqJSEnumerable<Grouping<TKey, TSource>> PartitionBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).partitionBy({keySelector}, {elementSelector})")]
		public static LinqJSEnumerable<Grouping<TKey, TElement>> PartitionBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).partitionBy({keySelector}, {elementSelector}, {resultSelector})")]
		public static LinqJSEnumerable<TResult> PartitionBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).partitionBy({keySelector}, {elementSelector}, {resultSelector}, {compareSelector})")]
		public static LinqJSEnumerable<TResult> PartitionBy<TSource, TKey, TElement, TCompare, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).buffer({count})")]
		public static LinqJSEnumerable<TSource[]> Buffer<TSource>(this IEnumerable<TSource> source, int count) { return null; }

		#endregion

		#region Aggregate

		[InlineCode("{$System.Linq.Enumerable}.from({source}).aggregate({func})")]
		public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).aggregate({seed}, {func})")]
		public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) { return default(TAccumulate); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).aggregate({seed}, {func}, {resultSelector})")]
		public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) { return default(TResult); }


		[InstanceMethodOnFirstArgument]
		public static double Average(this LinqJSEnumerable<int> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static double Average(this LinqJSEnumerable<long> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static float Average(this LinqJSEnumerable<float> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static double Average(this LinqJSEnumerable<double> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static decimal Average(this LinqJSEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average()")]
		public static double Average(this IEnumerable<int> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average()")]
		public static double Average(this IEnumerable<long> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average()")]
		public static float Average(this IEnumerable<float> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average()")]
		public static double Average(this IEnumerable<double> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average()")]
		public static decimal Average(this IEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average({selector})")]
		public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average({selector})")]
		public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average({selector})")]
		public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average({selector})")]
		public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).average({selector})")]
		public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) { return 0; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).count()")]
		public static int Count<TSource>(this IEnumerable<TSource> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).count({predicate})")]
		public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return 0; }


		[InstanceMethodOnFirstArgument]
		public static int Max(this LinqJSEnumerable<int> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static long Max(this LinqJSEnumerable<long> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static float Max(this LinqJSEnumerable<float> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static double Max(this LinqJSEnumerable<double> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static decimal Max(this LinqJSEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max()")]
		public static int Max(this IEnumerable<int> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max()")]
		public static long Max(this IEnumerable<long> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max()")]
		public static float Max(this IEnumerable<float> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max()")]
		public static double Max(this IEnumerable<double> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max()")]
		public static decimal Max(this IEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max({selector})")]
		public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max({selector})")]
		public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max({selector})")]
		public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max({selector})")]
		public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).max({selector})")]
		public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) { return 0; }


		[InstanceMethodOnFirstArgument]
		public static int Min(this LinqJSEnumerable<int> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static long Min(this LinqJSEnumerable<long> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static float Min(this LinqJSEnumerable<float> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static double Min(this LinqJSEnumerable<double> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static decimal Min(this LinqJSEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min()")]
		public static int Min(this IEnumerable<int> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min()")]
		public static long Min(this IEnumerable<long> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min()")]
		public static float Min(this IEnumerable<float> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min()")]
		public static double Min(this IEnumerable<double> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min()")]
		public static decimal Min(this IEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min({selector})")]
		public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min({selector})")]
		public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min({selector})")]
		public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min({selector})")]
		public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).min({selector})")]
		public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) { return 0; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).maxBy({selector})")]
		public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).maxBy({selector})")]
		public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).maxBy({selector})")]
		public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).maxBy({selector})")]
		public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).maxBy({selector})")]
		public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).minBy({selector})")]
		public static TSource MinBy<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).minBy({selector})")]
		public static TSource MinBy<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).minBy({selector})")]
		public static TSource MinBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).minBy({selector})")]
		public static TSource MinBy<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).minBy({selector})")]
		public static TSource MinBy<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) { return default(TSource); }


		[InstanceMethodOnFirstArgument]
		public static int Sum(this LinqJSEnumerable<int> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static long Sum(this LinqJSEnumerable<long> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static float Sum(this LinqJSEnumerable<float> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static double Sum(this LinqJSEnumerable<double> source) { return 0; }

		[InstanceMethodOnFirstArgument]
		public static decimal Sum(this LinqJSEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum()")]
		public static int Sum(this IEnumerable<int> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum()")]
		public static long Sum(this IEnumerable<long> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum()")]
		public static float Sum(this IEnumerable<float> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum()")]
		public static double Sum(this IEnumerable<double> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum()")]
		public static decimal Sum(this IEnumerable<decimal> source) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum({selector})")]
		public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum({selector})")]
		public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum({selector})")]
		public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum({selector})")]
		public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) { return 0; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).sum({selector})")]
		public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) { return 0; }

		#endregion

		#region Paging

		[InlineCode("{$System.Linq.Enumerable}.from({source}).elementAt({index})")]
		public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).elementAtOrDefault({index}, {TSource}.getDefaultValue())")]
		public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).elementAtOrDefault({index}, {defaultValue})")]
		public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index, TSource defaultValue) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).first()")]
		public static TSource First<TSource>(this IEnumerable<TSource> source) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).first({predicate})")]
		public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).firstOrDefault({TSource}.getDefaultValue())")]
		public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).firstOrDefault({defaultValue})")]
		public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).firstOrDefault({TSource}.getDefaultValue(), {predicate})")]
		public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).firstOrDefault({defaultValue}, {predicate})")]
		public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).last()")]
		public static TSource Last<TSource>(this IEnumerable<TSource> source) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).last({predicate})")]
		public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).lastOrDefault({TSource}.getDefaultValue())")]
		public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).lastOrDefault({defaultValue})")]
		public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).lastOrDefault({TSource}.getDefaultValue(), {predicate})")]
		public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).lastOrDefault({defaultValue}, {predicate})")]
		public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).single()")]
		public static TSource Single<TSource>(this IEnumerable<TSource> source) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).single({predicate})")]
		public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).singleOrDefault({TSource}.getDefaultValue())")]
		public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).singleOrDefault({defaultValue})")]
		public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).singleOrDefault({TSource}.getDefaultValue(), {predicate})")]
		public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return default(TSource); }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).singleOrDefault({defaultValue}, {predicate})")]
		public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue) { return default(TSource); }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).skip({count})")]
		public static LinqJSEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).skipWhile({predicate})")]
		public static LinqJSEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).skipWhile({predicate})")]
		public static LinqJSEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).take({count})")]
		public static LinqJSEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).takeWhile({predicate})")]
		public static LinqJSEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).takeWhile({predicate})")]
		public static LinqJSEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).takeExceptLast()")]
		public static LinqJSEnumerable<TSource> TakeExceptLast<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).takeExceptLast({count})")]
		public static LinqJSEnumerable<TSource> TakeExceptLast<TSource>(this IEnumerable<TSource> source, int count) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).takeFromLast({count})")]
		public static LinqJSEnumerable<TSource> TakeFromLast<TSource>(this IEnumerable<TSource> source, int count) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).indexOf({item})")]
		public static int IndexOf<TSource>(this IEnumerable<TSource> source, TSource item) { return 0; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).lastIndexOf({item})")]
		public static int LastIndexOf<TSource>(this IEnumerable<TSource> source, TSource item) { return 0; }

		#endregion

		#region Convert

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toArray()")]
		public static T[] ToArray<T>(this IEnumerable<T> source) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).toArray()")]
		public static List<T> ToList<T>(this IEnumerable<T> source) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).toLookup({keySelector})")]
		public static Lookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toLookup({keySelector}, {elementSelector})")]
		public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toLookup({keySelector}, {elementSelector}, {compareSelector})")]
		public static Lookup<TKey, TElement> ToLookup<TSource, TKey, TElement, TCompare>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, TCompare> compareSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).toObject({keySelector}, {valueSelector})")]
		public static JsDictionary<TKey, TValue> ToObject<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).toDictionary({TSource}.getDefaultValue(), {keySelector})")]
		public static IDictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toDictionary({TValue}.getDefaultValue(), {keySelector}, {valueSelector})")]
		public static IDictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toDictionary({TValue}.getDefaultValue(), {keySelector}, {valueSelector}, {compareSelector})")]
		public static IDictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue, TCompare>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, Func<TKey, TCompare> compareSelector) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toJoinedString()")]
		public static string ToJoinedString<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toJoinedString({separator})")]
		public static string ToJoinedString<TSource>(this IEnumerable<TSource> source, string separator) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).toJoinedString({separator}, {selector})")]
		public static string ToJoinedString<TSource>(this IEnumerable<TSource> source, string separator, Func<TSource, string> selector) { return null; }

		#endregion

		#region Action

		[InlineCode("{$System.Linq.Enumerable}.from({source}).doAction({action})")]
		public static LinqJSEnumerable<TSource> DoAction<TSource>(this IEnumerable<TSource> source, Action<TSource> action) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).doAction({action})")]
		public static LinqJSEnumerable<TSource> DoAction<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action) { return null; }


		[InlineCode("{$System.Linq.Enumerable}.from({source}).forEach({action})")]
		public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action) {}

		[InlineCode("{$System.Linq.Enumerable}.from({source}).forEach({action})")]
		public static void ForEach<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> action) {}

		[InlineCode("{$System.Linq.Enumerable}.from({source}).forEach({action})")]
		public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action) {}

		[InlineCode("{$System.Linq.Enumerable}.from({source}).forEach({action})")]
		public static void ForEach<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> action) {}


		[InlineCode("{$System.Linq.Enumerable}.from({source}).force()")]
		public static void Force<TSource>(this IEnumerable<TSource> source) {}

		#endregion

		#region Functional

		[InlineCode("{$System.Linq.Enumerable}.from({source}).letBind({func})")]
		public static LinqJSEnumerable<TResult> LetBind<TSource, TResult>(this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> func) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).share()")]
		public static LinqJSEnumerable<TSource> Share<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).memoize()")]
		public static LinqJSEnumerable<TSource> Memoize<TSource>(this IEnumerable<TSource> source) { return null; }

		#endregion

		#region Error handling

		[InlineCode("{$System.Linq.Enumerable}.from({source}).catchError({action})")]
		public static LinqJSEnumerable<TSource> CatchError<TSource>(this IEnumerable<TSource> source, Action<Exception> action) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).finallyAction({action})")]
		public static LinqJSEnumerable<TSource> FinallyAction<TSource>(this IEnumerable<TSource> source, Action action) { return null; }

		#endregion

		#region For debug

		[InlineCode("{$System.Linq.Enumerable}.from({source}).trace()")]
		public static LinqJSEnumerable<TSource> Trace<TSource>(this IEnumerable<TSource> source) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).trace({message})")]
		public static LinqJSEnumerable<TSource> Trace<TSource>(this IEnumerable<TSource> source, string message) { return null; }

		[InlineCode("{$System.Linq.Enumerable}.from({source}).trace({message}, {selector})")]
		public static LinqJSEnumerable<TSource> Trace<TSource>(this IEnumerable<TSource> source, string message, Func<TSource, string> selector) { return null; }

		#endregion
	}

	[Imported]
	[IgnoreGenericArguments]
	public class LinqJSEnumerable<TSource> : IEnumerable<TSource> {
		internal LinqJSEnumerable() {}

		public IEnumerator<TSource> GetEnumerator() { return null; }

		IEnumerator IEnumerable.GetEnumerator() { return null; }

		#region Projection / filtering

		public LinqJSEnumerable<TSource> CascadeBreadthFirst(Func<TSource, IEnumerable<TSource>> func) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> CascadeBreadthFirst<TResult>(Func<TSource, IEnumerable<TSource>> func, Func<TSource, TResult> resultSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> CascadeBreadthFirst<TResult>(Func<TSource, IEnumerable<TSource>> func, Func<TSource, int, TResult> resultSelector) { return null; }


		public LinqJSEnumerable<TSource> CascadeDepthFirst(Func<TSource, IEnumerable<TSource>> func) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> CascadeDepthFirst<TResult>(Func<TSource, IEnumerable<TSource>> func, Func<TSource, TResult> resultSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> CascadeDepthFirst<TResult>(Func<TSource, IEnumerable<TSource>> func, Func<TSource, int, TResult> resultSelector) { return null; }


		public LinqJSEnumerable<object> Flatten() { return null; }


		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Pairwise<TResult>(Func<TSource, TSource, TResult> selector) { return null; }


		public static LinqJSEnumerable<T> Scan<T>(Func<T, T, T> func) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TAccumulate> Scan<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Scan<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) { return null; }


		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Select<TResult>(Func<TSource, TResult> selector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Select<TResult>(Func<TSource, int, TResult> selector) { return null; }


		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> SelectMany<TResult>(Func<TSource, IEnumerable<TResult>> selector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> SelectMany<TResult>(Func<TSource, int, IEnumerable<TResult>> selector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> SelectMany<TCollection, TResult>(Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> SelectMany<TCollection, TResult>(Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) { return null; }


		[InlineCode("{this}.select(function(x) {{ return {$System.Type}.cast(x, {TResult}); }})")]
		public LinqJSEnumerable<TResult> Cast<TResult>() { return null; }


		[InlineCode("{this}.ofType({TResult})")]
		public LinqJSEnumerable<TResult> OfType<TResult>() { return null; }


		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Zip<TOther, TResult>(IEnumerable<TOther> other, Func<TSource, TOther, TResult> selector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Zip<TOther, TResult>(IEnumerable<TOther> other, Func<TSource, TOther, int, TResult> selector) { return null; }


		public LinqJSEnumerable<TSource> Where(Func<TSource, bool> predicate) { return null; }

		public LinqJSEnumerable<TSource> Where(Func<TSource, int, bool> predicate) { return null; }

		#endregion

		#region Join

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<TSource, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TSource, TInner, TResult> resultSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> Join<TInner, TKey, TCompare, TResult>(IEnumerable<TInner> inner, Func<TSource, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TSource, TInner, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<TSource, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TSource, IEnumerable<TInner>, TResult> resultSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> GroupJoin<TInner, TKey, TCompare, TResult>(IEnumerable<TInner> inner, Func<TSource, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TSource, IEnumerable<TInner>, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }

		#endregion

		#region Set methods

		public bool All(Func<TSource, bool> predicate) { return false; }

		public bool Any() { return false; }

		public bool Any(Func<TSource, bool> predicate) { return false; }

		public LinqJSEnumerable<TSource> Concat(IEnumerable<TSource> other) { return null; }

		public LinqJSEnumerable<TSource> Insert(int index, IEnumerable<TSource> other) { return null; }

		public LinqJSEnumerable<TSource> Alternate(TSource value) { return null; }

		public bool Contains(TSource value) { return false; }

		[IgnoreGenericArguments]
		public bool Contains<TValue>(TValue value, Func<TSource, TValue> compareSelector) { return false; }

		[InlineCode("{this}.defaultIfEmpty({TSource}.getDefaultValue())")]
		public LinqJSEnumerable<TSource> DefaultIfEmpty() { return null; }

		public LinqJSEnumerable<TSource> DefaultIfEmpty(TSource defaultValue) { return null; }

		public LinqJSEnumerable<TSource> Distinct() { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TSource> Distinct<TKey>(Func<TSource, TKey> compareSelector) { return null; }

		public LinqJSEnumerable<TSource> Except(IEnumerable<TSource> other) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TSource> Except<TKey>(IEnumerable<TSource> other, Func<TSource, TKey> compareSelector) { return null; }

		public bool SequenceEqual(IEnumerable<TSource> other) { return false; }

		[IgnoreGenericArguments]
		public bool SequenceEqual<TKey>(IEnumerable<TSource> other, Func<TSource, TKey> compareSelector) { return false; }

		public LinqJSEnumerable<TSource> Union(IEnumerable<TSource> other) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TSource> Union<TKey>(IEnumerable<TSource> other, Func<TSource, TKey> compareSelector) { return null; }

		#endregion

		#region Ordering

		public OrderedLinqJSEnumerable<TSource> OrderBy() { return null; }

		[IgnoreGenericArguments]
		public OrderedLinqJSEnumerable<TSource> OrderBy<TKey>(Func<TSource, TKey> keySelector) { return null; }

		public OrderedLinqJSEnumerable<TSource> OrderByDescending() { return null; }

		[IgnoreGenericArguments]
		public OrderedLinqJSEnumerable<TSource> OrderByDescending<TKey>(Func<TSource, TKey> keySelector) { return null; }

		public LinqJSEnumerable<TSource> Reverse() { return null; }

		public LinqJSEnumerable<TSource> Shuffle() { return null; }

		#endregion

		#region Grouping

		[IgnoreGenericArguments]
		public LinqJSEnumerable<Grouping<TKey, TSource>> GroupBy<TKey>(Func<TSource, TKey> keySelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<Grouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> GroupBy<TKey, TElement, TCompare, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }


		[IgnoreGenericArguments]
		public LinqJSEnumerable<Grouping<TKey, TSource>> PartitionBy<TKey>(Func<TSource, TKey> keySelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<Grouping<TKey, TElement>> PartitionBy<TKey, TElement>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> PartitionBy<TKey, TElement, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector) { return null; }

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> PartitionBy<TKey, TElement, TCompare, TResult>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, IEnumerable<TElement>, TResult> resultSelector, Func<TKey, TCompare> compareSelector) { return null; }


		[IgnoreGenericArguments]
		public LinqJSEnumerable<TSource[]> Buffer(int count) { return null; }

		#endregion

		#region Aggregate

		public TSource Aggregate(Func<TSource, TSource, TSource> func) { return default(TSource); }

		[IgnoreGenericArguments]
		public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) { return default(TAccumulate); }

		[IgnoreGenericArguments]
		public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) { return default(TResult); }


		public double Average(Func<TSource, int> selector) { return 0; }

		public double Average(Func<TSource, long> selector) { return 0; }

		public float Average(Func<TSource, float> selector) { return 0; }

		public double Average(Func<TSource, double> selector) { return 0; }

		public decimal Average(Func<TSource, decimal> selector) { return 0; }


		public int Count() { return 0; }

		public int Count(Func<TSource, bool> predicate) { return 0; }


		public int Max(Func<TSource, int> selector) { return 0; }

		public long Max(Func<TSource, long> selector) { return 0; }

		public float Max(Func<TSource, float> selector) { return 0; }

		public double Max(Func<TSource, double> selector) { return 0; }

		public decimal Max(Func<TSource, decimal> selector) { return 0; }


		public int Min(Func<TSource, int> selector) { return 0; }

		public long Min(Func<TSource, long> selector) { return 0; }

		public float Min(Func<TSource, float> selector) { return 0; }

		public double Min(Func<TSource, double> selector) { return 0; }

		public decimal Min(Func<TSource, decimal> selector) { return 0; }


		public TSource MaxBy(Func<TSource, int> selector) { return default(TSource); }

		public TSource MaxBy(Func<TSource, long> selector) { return default(TSource); }

		public TSource MaxBy(Func<TSource, float> selector) { return default(TSource); }

		public TSource MaxBy(Func<TSource, double> selector) { return default(TSource); }

		public TSource MaxBy(Func<TSource, decimal> selector) { return default(TSource); }


		public TSource MinBy(Func<TSource, int> selector) { return default(TSource); }

		public TSource MinBy(Func<TSource, long> selector) { return default(TSource); }

		public TSource MinBy(Func<TSource, float> selector) { return default(TSource); }

		public TSource MinBy(Func<TSource, double> selector) { return default(TSource); }

		public TSource MinBy(Func<TSource, decimal> selector) { return default(TSource); }


		public int Sum(Func<TSource, int> selector) { return 0; }

		public long Sum(Func<TSource, long> selector) { return 0; }

		public float Sum(Func<TSource, float> selector) { return 0; }

		public double Sum(Func<TSource, double> selector) { return 0; }

		public decimal Sum(Func<TSource, decimal> selector) { return 0; }

		#endregion

		#region Paging

		public TSource ElementAt(int index) { return default(TSource); }


		[InlineCode("{this}.elementAtOrDefault({index}, {TSource}.getDefaultValue())")]
		public TSource ElementAtOrDefault(int index) { return default(TSource); }


		public TSource ElementAtOrDefault(int index, TSource defaultValue) { return default(TSource); }


		public TSource First() { return default(TSource); }

		public TSource First(Func<TSource, bool> predicate) { return default(TSource); }


		[InlineCode("{this}.firstOrDefault({TSource}.getDefaultValue())")]
		public TSource FirstOrDefault() { return default(TSource); }

		public TSource FirstOrDefault(TSource defaultValue) { return default(TSource); }

		[InlineCode("{this}.firstOrDefault({TSource}.getDefaultValue(), {predicate})")]
		public TSource FirstOrDefault(Func<TSource, bool> predicate) { return default(TSource); }

		[InlineCode("{this}.firstOrDefault({defaultValue}, {predicate})")]
		public TSource FirstOrDefault(Func<TSource, bool> predicate, TSource defaultValue) { return default(TSource); }


		public TSource Last() { return default(TSource); }

		public TSource Last(Func<TSource, bool> predicate) { return default(TSource); }


		[InlineCode("{this}.lastOrDefault({TSource}.getDefaultValue())")]
		public TSource LastOrDefault() { return default(TSource); }

		public TSource LastOrDefault(TSource defaultValue) { return default(TSource); }

		[InlineCode("{this}.lastOrDefault({TSource}.getDefaultValue(), {predicate})")]
		public TSource LastOrDefault(Func<TSource, bool> predicate) { return default(TSource); }

		[InlineCode("{this}.lastOrDefault({defaultValue}, {predicate})")]
		public TSource LastOrDefault(Func<TSource, bool> predicate, TSource defaultValue) { return default(TSource); }


		public TSource Single() { return default(TSource); }

		public TSource Single(Func<TSource, bool> predicate) { return default(TSource); }


		[InlineCode("{this}.singleOrDefault({TSource}.getDefaultValue())")]
		public TSource SingleOrDefault() { return default(TSource); }

		public TSource SingleOrDefault(TSource defaultValue) { return default(TSource); }

		[InlineCode("{this}.singleOrDefault({TSource}.getDefaultValue(), {predicate})")]
		public TSource SingleOrDefault(Func<TSource, bool> predicate) { return default(TSource); }

		[InlineCode("{this}.singleOrDefault({defaultValue}, {predicate})")]
		public TSource SingleOrDefault(Func<TSource, bool> predicate, TSource defaultValue) { return default(TSource); }


		public LinqJSEnumerable<TSource> Skip(int count) { return null; }


		public LinqJSEnumerable<TSource> SkipWhile(Func<TSource, bool> predicate) { return null; }

		public LinqJSEnumerable<TSource> SkipWhile(Func<TSource, int, bool> predicate) { return null; }


		public LinqJSEnumerable<TSource> Take(int count) { return null; }


		public LinqJSEnumerable<TSource> TakeWhile(Func<TSource, bool> predicate) { return null; }

		public LinqJSEnumerable<TSource> TakeWhile(Func<TSource, int, bool> predicate) { return null; }


		public LinqJSEnumerable<TSource> TakeExceptLast() { return null; }

		public LinqJSEnumerable<TSource> TakeExceptLast(int count) { return null; }


		public LinqJSEnumerable<TSource> TakeFromLast(int count) { return null; }


		public int IndexOf(TSource item) { return 0; }


		public int LastIndexOf(TSource item) { return 0; }

		#endregion

		#region Convert
		
		public TSource[] ToArray() { return null; }


		[ScriptName("toArray")]
		public List<TSource> ToList() { return null; }


		[IgnoreGenericArguments]
		public Lookup<TKey, TSource> ToLookup<TKey>(Func<TSource, TKey> keySelector) { return null; }

		[IgnoreGenericArguments]
		public Lookup<TKey, TElement> ToLookup<TKey, TElement, TCompare>(Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TSource, TCompare> compareSelector) { return null; }


		[IgnoreGenericArguments]
		public JsDictionary<TKey, TValue> ToObject<TKey, TValue>(Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) { return null; }


		[InlineCode("{this}.toDictionary({TSource}.getDefaultValue(), {keySelector})")]
		public IDictionary<TKey, TSource> ToDictionary<TKey>(Func<TSource, TKey> keySelector) { return null; }

		[InlineCode("{this}.toDictionary({TValue}.getDefaultValue(), {keySelector}, {valueSelector})")]
		public IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) { return null; }

		[InlineCode("{this}.toDictionary({TValue}.getDefaultValue(), {keySelector}, {valueSelector})")]
		public IDictionary<TKey, TValue> ToDictionary<TKey, TValue, TCompare>(Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, Func<TKey, TCompare> compareSelector) { return null; }


		public string ToJoinedString() { return null; }

		public string ToJoinedString(string separator) { return null; }

		public string ToJoinedString(string separator, Func<TSource, string> selector) { return null; }

		#endregion

		#region Action

		public LinqJSEnumerable<TSource> DoAction(Action<TSource> action) { return null; }

		public LinqJSEnumerable<TSource> DoAction(Action<TSource, int> action) { return null; }


		public void ForEach(Action<TSource> action) {}

		public void ForEach(Func<TSource, bool> action) {}

		public void ForEach(Action<TSource, int> action) {}

		public void ForEach(Func<TSource, int, bool> action) {}


		public void Force() {}

		#endregion

		#region Functional

		[IgnoreGenericArguments]
		public LinqJSEnumerable<TResult> LetBind<TResult>(Func<IEnumerable<TSource>, IEnumerable<TResult>> func) { return null; }

		public LinqJSEnumerable<TSource> Share() { return null; }

		public LinqJSEnumerable<TSource> Memoize() { return null; }

		#endregion

		#region Error handling

		public LinqJSEnumerable<TSource> CatchError(Action<Exception> action) { return null; }

		public LinqJSEnumerable<TSource> FinallyAction(Action action) { return null; }

		#endregion

		#region For debug

		public LinqJSEnumerable<TSource> Trace() { return null; }

		public LinqJSEnumerable<TSource> Trace(string message) { return null; }

		public LinqJSEnumerable<TSource> Trace(string message, Func<TSource, string> selector) { return null; }

		#endregion
	}

	[Imported]
	[IgnoreGenericArguments]
	public class OrderedLinqJSEnumerable<TSource> : LinqJSEnumerable<TSource> {
		internal OrderedLinqJSEnumerable() {}

		[IgnoreGenericArguments]
		public OrderedLinqJSEnumerable<TSource> ThenBy<TKey>(Func<TSource, TKey> keySelector) { return null; }

		[IgnoreGenericArguments]
		public OrderedLinqJSEnumerable<TSource> ThenByDescending<TKey>(Func<TSource, TKey> keySelector) { return null; }
	}

	[Imported]
	[IgnoreGenericArguments]
	public class Grouping<TKey, TElement> : LinqJSEnumerable<TElement> {
		internal Grouping() {}

		public TKey Key { [ScriptName("key")] get { return default(TKey); } }
	}

	[Imported]
	[IgnoreGenericArguments]
	public class Lookup<TKey, TElement> : IEnumerable<Grouping<TKey, TElement>> {
		internal Lookup() {}

		public int Count { [ScriptName("count")] get { return 0; } }

		public LinqJSEnumerable<TElement> this[TKey key] { [ScriptName("get")] get { return null; } }

		public bool Contains(TKey key) { return false; }

		public IEnumerator<Grouping<TKey, TElement>> GetEnumerator() {
			return null;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return null;
		}
	}
}
