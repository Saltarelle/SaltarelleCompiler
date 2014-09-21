using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class QueryExpressionToExpressionTreeNestedTests : MethodCompilerTestBase {
		private static readonly Lazy<MetadataReference> _mscorlibLazy = new Lazy<MetadataReference>(() => new MetadataFileReference(typeof(object).Assembly.Location));
		private static readonly Lazy<MetadataReference[]> _referencesLazy = new Lazy<MetadataReference[]>(() => new[] { _mscorlibLazy.Value, Common.ExpressionAssembly });

		private void AssertCorrect(string csharp, string expected, IMetadataImporter metadataImporter = null) {
			AssertCorrect(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
interface IOrderedEnumerable<T> : IEnumerable<T> {}
interface IGrouping<K,T> : IEnumerable<T> {
	K Key { get; }
}
static class Enumerable {
	public static IEnumerable<T> Cast<T>(this IEnumerable obj) { return null; }
	public static IEnumerable<T> Where<T>(this IEnumerable<T> obj, Func<T,bool> predicate) { return null; }
	public static IEnumerable<U> Select<T,U>(this IEnumerable<T> obj, Func<T,U> selector) { return null; }
	public static IEnumerable<V> SelectMany<T,U,V>(this IEnumerable<T> obj, Func<T,IEnumerable<U>> selector, Func<T,U,V> resultSelector) { return null; }
	public static IEnumerable<V> Join<T,U,K,V>(this IEnumerable<T> obj, IEnumerable<U> inner, Func<T,K> outerKeySelector, Func<U,K> innerKeySelector, Func<T,U,V> resultSelector) { return null; }
	public static IEnumerable<V> GroupJoin<T,U,K,V>(this IEnumerable<T> obj, IEnumerable<U> inner, Func<T,K> outerKeySelector, Func<U,K> innerKeySelector, Func<T,IEnumerable<U>,V> resultSelector) { return null; }
	public static IOrderedEnumerable<T> OrderBy<T,K>(this IEnumerable<T> obj, Func<T,K> keySelector) { return null; }
	public static IOrderedEnumerable<T> OrderByDescending<T,K>(this IEnumerable<T> obj, Func<T,K> keySelector) { return null; }
	public static IOrderedEnumerable<T> ThenBy<T,K>(this IOrderedEnumerable<T> obj, Func<T,K> keySelector) { return null; }
	public static IOrderedEnumerable<T> ThenByDescending<T,K>(this IOrderedEnumerable<T> obj, Func<T,K> keySelector) { return null; }
	public static IEnumerable<IGrouping<K,T>> GroupBy<T,K>(this IEnumerable<T> obj, Func<T,K> keySelector) { return null; }
	public static IEnumerable<IGrouping<K,E>> GroupBy<T,K,E>(this IEnumerable<T> obj, Func<T,K> keySelector, Func<T,E> elementSelector) { return null; }
}
class C {
	" + csharp + @"
}", expected, references: _referencesLazy.Value, addSkeleton: false, collapseWhitespace: true);

		}

		[Test]
		public void QueryExpressionWithFromAndSelectWorks() {
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from a in args select int.Parse(a);
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_String}, '$a');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [{ga_String}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('args', to_$Array({ga_String}), $args),
					sm_$InstantiateGenericType({IEnumerable}, {ga_String})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Call(
						null,
						$GetMember({to_Int32}, 'Parse'),
						[$tmp1]
					),
					[$tmp1]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void QueryExpressionWithSingleFromAndExplicitTypeWorks() {
			AssertCorrect(@"
void M() {
	object[] args = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from string a in args select int.Parse(a);
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_String}, '$a');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [{ga_String}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Cast', [{ga_String}]),
					[
						{sm_Expression}.$Convert(
							$Local('args', to_$Array({ga_Object}), $args),
							{sm_IEnumerable}
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Call(
						null,
						$GetMember({to_Int32}, 'Parse'),
						[$tmp1]
					),
					[$tmp1]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void QueryExpressionWithLetWorks() {
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	Expression<Func<IEnumerable<string>>> result = () => from a in args let b = int.Parse(a) select a + b.ToString();
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_String}, '$a', {sm_Int32}, '$b');
	var $tmp1 = {sm_Expression}.$Parameter({sm_String}, '$a');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_String}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [{ga_String}, ga_$Anonymous]),
					[
						{sm_Expression}.$Convert(
							$Local('args', to_$Array({ga_String}), $args),
							sm_$InstantiateGenericType({IEnumerable}, {ga_String})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp2.$GetConstructors()[0],
								[
									$tmp1,
									{sm_Expression}.$Call(
										null,
										$GetMember({to_Int32}, 'Parse'),
										[$tmp1]
									)
								],
								[$tmp2.$GetProperty('$a'), $tmp2.$GetProperty('$b')]
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Property(
							$tmp3,
							$tmp2.$GetProperty('$a')
						),
						{sm_Expression}.$Call(
							{sm_Expression}.$Property(
								$tmp3,
								$tmp2.$GetProperty('$b')
							),
							$GetMember({to_Int32}, 'ToString'),
							[]
						),
						{sm_String}
					),
					[$tmp3]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void QueryExpressionWithTwoLetsWorks() {
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	Expression<Func<IEnumerable<string>>> result = () => from a in args let b = int.Parse(a) let c = b + 1 select a + b.ToString() + c.ToString();
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_String}, '$a', {sm_Int32}, '$b');
	var $tmp4 = $GetTransparentType($tmp2, '$tmp1', {sm_Int32}, '$c');
	var $tmp1 = {sm_Expression}.$Parameter({sm_String}, '$a');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp5 = {sm_Expression}.$Parameter($tmp4, '$tmp3');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_String}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
					[
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'Select', [{ga_String}, ga_$Anonymous]),
							[
								{sm_Expression}.$Convert(
									$Local('args', to_$Array({ga_String}), $args),
									sm_$InstantiateGenericType({IEnumerable}, {ga_String})
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$New($tmp2.$GetConstructors()[0],
									[
										$tmp1,
										{sm_Expression}.$Call(
											null,
											$GetMember({to_Int32}, 'Parse'),
											[$tmp1]
										)
									],
									[$tmp2.$GetProperty('$a'), $tmp2.$GetProperty('$b')]
								),
								[$tmp1]
							)
						]
					),
					{sm_Expression}.$Lambda(
						{sm_Expression}.$New(
							$tmp4.$GetConstructors()[0],
							[
								$tmp3,
								{sm_Expression}.$Add(
									{sm_Expression}.$Property(
										$tmp3,
										$tmp2.$GetProperty('$b')
									),
									{sm_Expression}.$Constant(1, {sm_Int32}),
									{sm_Int32}
								)
							],
							[$tmp4.$GetProperty('$tmp1'), $tmp4.$GetProperty('$c')]
						),
						[$tmp3])
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Add(
							{sm_Expression}.$Property(
								{sm_Expression}.$Property(
									$tmp5,
									$tmp4.$GetProperty('$tmp1')
								),
								$tmp2.$GetProperty('$a')
							),
							{sm_Expression}.$Call(
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										$tmp5,
										$tmp4.$GetProperty('$tmp1')
									),
									$tmp2.$GetProperty('$b')
								),
								$GetMember({to_Int32}, 'ToString'),
								[]
							),
							{sm_String}
						),
						{sm_Expression}.$Call(
							{sm_Expression}.$Property(
								$tmp5,
								$tmp4.$GetProperty('$c')
							),
							$GetMember({to_Int32}, 'ToString'),
							[]
						),
						{sm_String}
					),
					[$tmp5]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void TwoFromClausesFollowedBySelectWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 from j in i.ToString() select i + (int)j;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Char}, '$j');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'SelectMany', [{ga_Int32}, {ga_Char}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr1', to_$Array({ga_Int32}), $arr1),
					sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Convert(
						{sm_Expression}.$Call(
							$tmp1,
							$GetMember({to_Int32}, 'ToString'),
							[]
						),
						sm_$InstantiateGenericType({IEnumerable}, {ga_Char})
					),
					[$tmp1]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						$tmp2,
						{sm_Expression}.$Convert($tmp3, {sm_Int32}),
						{sm_Int32}
					),
					[$tmp2, $tmp3]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void CastInSecondFromClauseWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 from int j in i.ToString() select i + (int)j;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, '$j');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'SelectMany', [{ga_Int32}, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr1', to_$Array({ga_Int32}), $arr1),
					sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Call(
						null,
						$GetMember({to_Enumerable}, 'Cast', [{ga_Int32}]),
						[
							{sm_Expression}.$Convert(
								{sm_Expression}.$Call(
									$tmp1,
									$GetMember({to_Int32}, 'ToString'),
									[]
								),
								{sm_IEnumerable}
							)
						]
					),
					[$tmp1]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						$tmp2,
						$tmp3,
						{sm_Int32}
					),
					[$tmp2, $tmp3]
				)
			]
		),
		[]
	);

");
		}

		[Test]
		public void TwoFromClausesFollowedByLetWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null;
	string[] arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<string>>> result = () => from i in arr1 from j in arr2 let k = i + int.Parse(j) select i + j + k;
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_String}, '$j');
	var $tmp6 = $GetTransparentType($tmp2, '$tmp2', {sm_Int32}, '$k');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp4 = {sm_Expression}.$Parameter({sm_String}, '$j');
	var $tmp5 = {sm_Expression}.$Parameter($tmp2, '$tmp2');
	var $tmp7 = {sm_Expression}.$Parameter($tmp6, '$tmp5');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_String}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
					[
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'SelectMany', [{ga_Int32}, {ga_String}, ga_$Anonymous]),
							[
								{sm_Expression}.$Convert(
									$Local('arr1', to_$Array({ga_Int32}), $arr1),
									sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Convert(
										$Local('arr2', to_$Array({ga_String}), $arr2),
										sm_$InstantiateGenericType({IEnumerable}, {ga_String})
									),
									[$tmp1]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$New(
										$tmp2.$GetConstructors()[0],
										[$tmp3, $tmp4],
										[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
									),
									[$tmp3, $tmp4]
								)
							]
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp6.$GetConstructors()[0],
								[
									$tmp5,
									{sm_Expression}.$Add(
										{sm_Expression}.$Property(
											$tmp5,
											$tmp2.$GetProperty('$i')
										),
										{sm_Expression}.$Call(
											null,
											$GetMember({to_Int32}, 'Parse'),
											[
												{sm_Expression}.$Property(
													$tmp5,
													$tmp2.$GetProperty('$j')
												)
											]
										),
										{sm_Int32}
									)
								],
								[$tmp6.$GetProperty('$tmp2'), $tmp6.$GetProperty('$k')]
							),
							[$tmp5]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Add(
							{sm_Expression}.$Convert(
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										$tmp7,
										$tmp6.$GetProperty('$tmp2')
									),
									$tmp2.$GetProperty('$i')
								),
								{sm_Object}
							),
							{sm_Expression}.$Property(
								{sm_Expression}.$Property(
									$tmp7,
									$tmp6.$GetProperty('$tmp2')
								),
								$tmp2.$GetProperty('$j')
							),
							{sm_String}
						),
						{sm_Expression}.$Convert(
							{sm_Expression}.$Property(
								$tmp7,
								$tmp6.$GetProperty('$k')
							),
							{sm_Object}
						),
						{sm_String}
					),
					[$tmp7]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void SelectManyFollowedBySelectWorksWhenTheTargetIsTransparentAndTheCollectionsAreCorrelated() {
			AssertCorrect(@"
class C1 {
	public int[] Result;
	public int X;
}
C1 F(int i) {
	return null;
}

void M() {
	int[] outer = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in outer let j = F(i) from k in j.Result select i + j.X + k;
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_C1}, '$j');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp4 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp5 = {sm_Expression}.$Parameter({sm_Int32}, '$k');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'SelectMany', [ga_$Anonymous, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
					[
						{sm_Expression}.$Convert(
							$Local('outer', to_$Array({ga_Int32}), $outer),
							sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp2.$GetConstructors()[0],
								[
									$tmp1,
									{sm_Expression}.$Call(
										{sm_Expression}.$Constant(this, {sm_C}),
										$GetMember({to_C}, 'F'),
										[$tmp1]
									)
								],
								[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Convert(
						{sm_Expression}.$Field(
							{sm_Expression}.$Property(
								$tmp3,
								$tmp2.$GetProperty('$j')
							),
							$GetMember({to_C1}, 'Result')
						),
						sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
					),
					[$tmp3]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Add(
							{sm_Expression}.$Property(
								$tmp4,
								$tmp2.$GetProperty('$i')
							),
							{sm_Expression}.$Field(
								{sm_Expression}.$Property(
									$tmp4,
									$tmp2.$GetProperty('$j')
								),
								$GetMember({to_C1}, 'X')
							),
							{sm_Int32}
						),
						$tmp5,
						{sm_Int32}
					),
					[$tmp4, $tmp5]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void SelectManyFollowedByLetWorksWhenTheTargetIsTransparentAndTheCollectionsAreCorrelated() {
			AssertCorrect(@"
class C1 {
	public int[] Result;
	public int X;
}
C1 F(int i) {
	return null;
}

void M() {
	int[] outer = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in outer let j = F(i) from k in j.Result let l = i + j.X + k select i + j.X + k + l;
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_C1}, '$j');
	var $tmp4 = $GetTransparentType($tmp2, '$tmp1', {sm_Int32}, '$k');
	var $tmp8 = $GetTransparentType($tmp4, '$tmp4', {sm_Int32}, '$l');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp5 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp6 = {sm_Expression}.$Parameter({sm_Int32}, '$k');
	var $tmp7 = {sm_Expression}.$Parameter($tmp4, '$tmp4');
	var $tmp9 = {sm_Expression}.$Parameter($tmp8, '$tmp7');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
					[
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'SelectMany', [ga_$Anonymous, {ga_Int32}, ga_$Anonymous]),
							[
								{sm_Expression}.$Call(
									null,
									$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
									[
										{sm_Expression}.$Convert(
											$Local('outer', to_$Array({ga_Int32}), $outer),
											sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
										),
										{sm_Expression}.$Lambda(
											{sm_Expression}.$New(
												$tmp2.$GetConstructors()[0],
												[
													$tmp1,
													{sm_Expression}.$Call(
														{sm_Expression}.$Constant(this, {sm_C}),
														$GetMember({to_C}, 'F'),
														[$tmp1]
													)
												],
												[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
											),
											[$tmp1]
										)
									]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Convert(
										{sm_Expression}.$Field(
											{sm_Expression}.$Property(
												$tmp3,
												$tmp2.$GetProperty('$j')
											),
											$GetMember({to_C1}, 'Result')
										),
										sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
									),
									[$tmp3]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$New(
										$tmp4.$GetConstructors()[0],
										[$tmp5, $tmp6],
										[$tmp4.$GetProperty('$tmp1'), $tmp4.$GetProperty('$k')]
									),
									[$tmp5, $tmp6]
								)
							]
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp8.$GetConstructors()[0],
								[
									$tmp7,
									{sm_Expression}.$Add(
										{sm_Expression}.$Add(
											{sm_Expression}.$Property(
												{sm_Expression}.$Property(
													$tmp7,
													$tmp4.$GetProperty('$tmp1')
												),
												$tmp2.$GetProperty('$i')
											),
											{sm_Expression}.$Field(
												{sm_Expression}.$Property(
													{sm_Expression}.$Property(
														$tmp7,
														$tmp4.$GetProperty('$tmp1')
													),
													$tmp2.$GetProperty('$j')
												),
												$GetMember({to_C1}, 'X')
											),
											{sm_Int32}
										),
										{sm_Expression}.$Property(
											$tmp7,
											$tmp4.$GetProperty('$k')
										),
										{sm_Int32}
									)
								],
								[$tmp8.$GetProperty('$tmp4'), $tmp8.$GetProperty('$l')]
							),
							[$tmp7]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Add(
							{sm_Expression}.$Add(
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										{sm_Expression}.$Property(
											$tmp9,
											$tmp8.$GetProperty('$tmp4')
										),
										$tmp4.$GetProperty('$tmp1')
									),
									$tmp2.$GetProperty('$i')
								),
								{sm_Expression}.$Field(
									{sm_Expression}.$Property(
										{sm_Expression}.$Property(
											{sm_Expression}.$Property(
												$tmp9,
												$tmp8.$GetProperty('$tmp4')
											),
											$tmp4.$GetProperty('$tmp1')
										),
										$tmp2.$GetProperty('$j')
									),
									$GetMember({to_C1}, 'X')
								),
								{sm_Int32}
							),
							{sm_Expression}.$Property(
								{sm_Expression}.$Property(
									$tmp9,
									$tmp8.$GetProperty('$tmp4')
								),
								$tmp4.$GetProperty('$k')
							),
							{sm_Int32}
						),
						{sm_Expression}.$Property(
							$tmp9,
							$tmp8.$GetProperty('$l')
						),
						{sm_Int32}
					),
					[$tmp9]
				)
			]
		),
		[]
	);

");
		}

		[Test]
		public void ThreeFromClausesFollowedBySelectWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null, arr3 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 from j in arr2 from k in arr3 select i + j + k;
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_Int32}, '$j');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, '$j');
	var $tmp5 = {sm_Expression}.$Parameter($tmp2, '$tmp2');
	var $tmp6 = {sm_Expression}.$Parameter($tmp2, '$tmp2');
	var $tmp7 = {sm_Expression}.$Parameter({sm_Int32}, '$k');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'SelectMany', [ga_$Anonymous, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'SelectMany', [{ga_Int32}, {ga_Int32}, ga_$Anonymous]),
					[
						{sm_Expression}.$Convert(
							$Local('arr1', to_$Array({ga_Int32}), $arr1),
							sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$Convert(
								$Local('arr2', to_$Array({ga_Int32}), $arr2),
								sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
							),
							[$tmp1]
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp2.$GetConstructors()[0],
								[$tmp3, $tmp4],
								[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
							),
							[$tmp3, $tmp4]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Convert(
						$Local('arr3', to_$Array({ga_Int32}), $arr3),
						sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
					),
					[$tmp5]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Add(
							{sm_Expression}.$Property(
								$tmp6,
								$tmp2.$GetProperty('$i')
							),
							{sm_Expression}.$Property(
								$tmp6,
								$tmp2.$GetProperty('$j')
							),
							{sm_Int32}
						),
						$tmp7,
						{sm_Int32}
					),
					[$tmp6, $tmp7]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void GroupByWithSimpleValue() {
			AssertCorrect(@"
class C1 { public int field; }

void M() {
	C1[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<IGrouping<int, C1>>>> result = () => from i in arr group i by i.field;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C1}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'GroupBy', [{ga_C1}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr', to_$Array({ga_C1}), $arr),
					sm_$InstantiateGenericType({IEnumerable}, {ga_C1})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp1,
						$GetMember({to_C1}, 'field')
					),
					[$tmp1]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void GroupByWithProjectedValue() {
			AssertCorrect(@"
class C1 { public int field, something; }

void M() {
	C1[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<IGrouping<int, int>>>> result = () => from i in arr group i.something by i.field;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C1}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_C1}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'GroupBy', [{ga_C1}, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr', to_$Array({ga_C1}), $arr),
					sm_$InstantiateGenericType({IEnumerable}, {ga_C1})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp1,
						$GetMember({to_C1}, 'field')
					),
					[$tmp1]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp2,
						$GetMember({to_C1}, 'something')
					),
					[$tmp2]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void GroupByWhenThereIsATransparentIdentifer() {
			AssertCorrect(@"
class C1 { public int field; }

int F(C1 x) { return 0; }

void M() {
	C1[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<IGrouping<int, C1>>>> result = () => from i in arr let j = F(i) group i by i.field;
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_C1}, '$i', {sm_Int32}, '$j');
	var $tmp1 = {sm_Expression}.$Parameter({sm_C1}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp4 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'GroupBy', [ga_$Anonymous, {ga_Int32}, {ga_C1}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [{ga_C1}, ga_$Anonymous]),
					[
						{sm_Expression}.$Convert(
							$Local('arr', to_$Array({ga_C1}), $arr),
							sm_$InstantiateGenericType({IEnumerable}, {ga_C1})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp2.$GetConstructors()[0],
								[
									$tmp1,
									{sm_Expression}.$Call(
										{sm_Expression}.$Constant(this, {sm_C}),
										$GetMember({to_C}, 'F'),
										[$tmp1]
									)
								],
								[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						{sm_Expression}.$Property(
							$tmp3,
							$tmp2.$GetProperty('$i')
						),
						$GetMember({to_C1}, 'field')
					),
					[$tmp3]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Property(
						$tmp4,
						$tmp2.$GetProperty('$i')
					),
					[$tmp4]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void JoinWithTypeCast() {
			AssertCorrect(@"
class CI { public int keyi, valuei; }
class CJ { public int keyj, valuej; }

void M() {
	CI[] arr1 = null;
	object[] arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 join CJ j in arr2 on i.keyi equals j.keyj select i.valuei + j.valuej;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $tmp3 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp4 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Join', [{ga_CI}, {ga_CJ}, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr1', to_$Array({ga_CI}), $arr1),
					sm_$InstantiateGenericType({IEnumerable}, {ga_CI})
				),
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Cast', [{ga_CJ}]),
					[
						{sm_Expression}.$Convert(
							$Local('arr2', to_$Array({ga_Object}), $arr2),
							{sm_IEnumerable}
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp1,
						$GetMember({to_CI}, 'keyi')
					),
					[$tmp1]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp2,
						$GetMember({to_CJ}, 'keyj')
					),
					[$tmp2]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Field(
							$tmp3,
							$GetMember({to_CI}, 'valuei')
						),
						{sm_Expression}.$Field(
							$tmp4,
							$GetMember({to_CJ}, 'valuej')
						),
						{sm_Int32}
					),
					[$tmp3, $tmp4]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void JoinFollowedBySelect() {
			AssertCorrect(@"
class CI { public int keyi, valuei; }
class CJ { public int keyj, valuej; }

void M() {
	CI[] arr1 = null;
	CJ[] arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 join j in arr2 on i.keyi equals j.keyj select i.valuei + j.valuej;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $tmp3 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp4 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Join', [{ga_CI}, {ga_CJ}, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr1', to_$Array({ga_CI}), $arr1),
					sm_$InstantiateGenericType({IEnumerable}, {ga_CI})
				),
				{sm_Expression}.$Convert(
					$Local('arr2', to_$Array({ga_CJ}), $arr2),
					sm_$InstantiateGenericType({IEnumerable}, {ga_CJ})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp1,
						$GetMember({to_CI}, 'keyi')
					),
					[$tmp1]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp2,
						$GetMember({to_CJ}, 'keyj')
					),
					[$tmp2]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Field(
							$tmp3,
							$GetMember({to_CI}, 'valuei')
						),
						{sm_Expression}.$Field(
							$tmp4,
							$GetMember({to_CJ}, 'valuej')
						),
						{sm_Int32}
					),
					[$tmp3, $tmp4]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void JoinFollowedByLet() {
			AssertCorrect(@"
class CI { public int keyi, valuei; }
class CJ { public int keyj, valuej; }

void M() {
	CI[] arr1 = null;
	CJ[] arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 join j in arr2 on i.keyi equals j.keyj let k = i.valuei + j.valuej select i.valuei + j.valuej + k;
	// END
}",
@"	var $tmp3 = $GetTransparentType({sm_CI}, '$i', {sm_CJ}, '$j');
	var $tmp7 = $GetTransparentType($tmp3, '$tmp3', {sm_Int32}, '$k');
	var $tmp1 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $tmp4 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp5 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $tmp6 = {sm_Expression}.$Parameter($tmp3, '$tmp3');
	var $tmp8 = {sm_Expression}.$Parameter($tmp7, '$tmp6');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
					[
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'Join', [{ga_CI}, {ga_CJ}, {ga_Int32}, ga_$Anonymous]),
							[
								{sm_Expression}.$Convert(
									$Local('arr1', to_$Array({ga_CI}), $arr1),
									sm_$InstantiateGenericType({IEnumerable}, {ga_CI})
								),
								{sm_Expression}.$Convert(
									$Local('arr2', to_$Array({ga_CJ}), $arr2),
									sm_$InstantiateGenericType({IEnumerable}, {ga_CJ})
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Field(
										$tmp1,
										$GetMember({to_CI}, 'keyi')
									),
									[$tmp1]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Field(
										$tmp2,
										$GetMember({to_CJ}, 'keyj')
									),
									[$tmp2]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$New(
										$tmp3.$GetConstructors()[0],
										[$tmp4, $tmp5],
										[$tmp3.$GetProperty('$i'), $tmp3.$GetProperty('$j')]
									),
									[$tmp4, $tmp5]
								)
							]
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp7.$GetConstructors()[0],
								[
									$tmp6,
									{sm_Expression}.$Add(
										{sm_Expression}.$Field(
											{sm_Expression}.$Property(
												$tmp6,
												$tmp3.$GetProperty('$i')
											),
											$GetMember({to_CI}, 'valuei')
										),
										{sm_Expression}.$Field(
											{sm_Expression}.$Property(
												$tmp6,
												$tmp3.$GetProperty('$j')
											),
											$GetMember({to_CJ}, 'valuej')
										),
										{sm_Int32}
									)
								],
								[$tmp7.$GetProperty('$tmp3'), $tmp7.$GetProperty('$k')]
							),
							[$tmp6]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Add(
							{sm_Expression}.$Field(
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										$tmp8,
										$tmp7.$GetProperty('$tmp3')
									),
									$tmp3.$GetProperty('$i')
								),
								$GetMember({to_CI}, 'valuei')
							),
							{sm_Expression}.$Field(
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										$tmp8,
										$tmp7.$GetProperty('$tmp3')
									),
									$tmp3.$GetProperty('$j')
								),
								$GetMember({to_CJ}, 'valuej')
							),
							{sm_Int32}
						),
						{sm_Expression}.$Property(
							$tmp8,
							$tmp7.$GetProperty('$k')
						),
						{sm_Int32}
					),
					[$tmp8]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void JoinFollowedBySelectWhenThereIsATransparentIdentifier() {
			AssertCorrect(@"
class CJ { public int keyj, valuej; }
class CK { public int keyk, valuek; }
CJ F(int i) { return null; }

void M() {
	int[] arr1 = null;
	CK[] arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 let j = F(i) join k in arr2 on j.keyj equals k.keyk select i + j.valuej + k.valuek;
	// END
}",
@"		var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_CJ}, '$j');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp4 = {sm_Expression}.$Parameter({sm_CK}, '$k');
	var $tmp5 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp6 = {sm_Expression}.$Parameter({sm_CK}, '$k');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Join', [ga_$Anonymous, {ga_CK}, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
					[
						{sm_Expression}.$Convert(
							$Local('arr1', to_$Array({ga_Int32}), $arr1),
							sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp2.$GetConstructors()[0],
								[
									$tmp1,
									{sm_Expression}.$Call(
										{sm_Expression}.$Constant(this, {sm_C}),
										$GetMember({to_C}, 'F'),
										[$tmp1]
									)
								],
								[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Convert(
					$Local('arr2', to_$Array({ga_CK}), $arr2),
					sm_$InstantiateGenericType({IEnumerable}, {ga_CK})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						{sm_Expression}.$Property(
							$tmp3,
							$tmp2.$GetProperty('$j')
						),
						$GetMember({to_CJ}, 'keyj')
					),
					[$tmp3]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp4,
						$GetMember({to_CK}, 'keyk')
					),
					[$tmp4]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Add(
							{sm_Expression}.$Property(
								$tmp5,
								$tmp2.$GetProperty('$i')
							),
							{sm_Expression}.$Field(
								{sm_Expression}.$Property(
									$tmp5,
									$tmp2.$GetProperty('$j')
								),
								$GetMember({to_CJ}, 'valuej')
							),
							{sm_Int32}
						),
						{sm_Expression}.$Field(
							$tmp6,
							$GetMember({to_CK}, 'valuek')
						),
						{sm_Int32}
					),
					[$tmp5, $tmp6]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void GroupJoinFollowedBySelect() {
			AssertCorrect(@"
class CI { public int keyi, valuei; }
class CJ { public int keyj, valuej; }
static int F(CI i, IEnumerable<CJ> g) { return 0; }

void M() {
	CI[] arr1 = null;
	CJ[] arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 join j in arr2 on i.keyi equals j.keyj into g select F(i, g);
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $tmp3 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp4 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({IEnumerable}, {ga_CJ}), '$g');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'GroupJoin', [{ga_CI}, {ga_CJ}, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr1', to_$Array({ga_CI}), $arr1),
					sm_$InstantiateGenericType({IEnumerable}, {ga_CI})
				),
				{sm_Expression}.$Convert(
					$Local('arr2', to_$Array({ga_CJ}), $arr2),
					sm_$InstantiateGenericType({IEnumerable}, {ga_CJ})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp1,
						$GetMember({to_CI}, 'keyi')
					),
					[$tmp1]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp2,
						$GetMember({to_CJ}, 'keyj')
					),
					[$tmp2]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Call(
						null,
						$GetMember({to_C}, 'F'),
						[$tmp3, $tmp4]
					),
					[$tmp3, $tmp4]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void GroupJoinFollowedByLet() {
			AssertCorrect(@"
class CI { public int keyi, valuei; }
class CJ { public int keyj, valuej; }
static int F(CI i, IEnumerable<CJ> j) { return 0; }

void M() {
	CI[] arr1 = null;
	CJ[] arr2 = null;
	int[] outer = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () =>  from i in arr1 join j in arr2 on i.keyi equals j.keyj into g let k = F(i, g) select F(i, g) + k;
	// END
}",
@"	var $tmp3 = $GetTransparentType({sm_CI}, '$i', sm_$InstantiateGenericType({IEnumerable}, {ga_CJ}), '$g');
	var $tmp7 = $GetTransparentType($tmp3, '$tmp3', {sm_Int32}, '$k');
	var $tmp1 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_CJ}, '$j');
	var $tmp4 = {sm_Expression}.$Parameter({sm_CI}, '$i');
	var $tmp5 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({IEnumerable}, {ga_CJ}), '$g');
	var $tmp6 = {sm_Expression}.$Parameter($tmp3, '$tmp3');
	var $tmp8 = {sm_Expression}.$Parameter($tmp7, '$tmp6');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
					[
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'GroupJoin', [{ga_CI}, {ga_CJ}, {ga_Int32}, ga_$Anonymous]),
							[
								{sm_Expression}.$Convert(
									$Local('arr1', to_$Array({ga_CI}), $arr1),
									sm_$InstantiateGenericType({IEnumerable}, {ga_CI})
								),
								{sm_Expression}.$Convert(
									$Local('arr2', to_$Array({ga_CJ}), $arr2),
									sm_$InstantiateGenericType({IEnumerable}, {ga_CJ})
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Field(
										$tmp1,
										$GetMember({to_CI}, 'keyi')
									),
									[$tmp1]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Field(
										$tmp2,
										$GetMember({to_CJ}, 'keyj')
									),
									[$tmp2]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$New(
										$tmp3.$GetConstructors()[0],
										[$tmp4, $tmp5],
										[$tmp3.$GetProperty('$i'), $tmp3.$GetProperty('$g')]
									),
									[$tmp4, $tmp5]
								)
							]
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp7.$GetConstructors()[0],
								[
									$tmp6,
									{sm_Expression}.$Call(
										null,
										$GetMember({to_C}, 'F'),
										[
											{sm_Expression}.$Property(
												$tmp6,
												$tmp3.$GetProperty('$i')
											),
											{sm_Expression}.$Property(
												$tmp6,
												$tmp3.$GetProperty('$g')
											)
										]
									)
								],
								[$tmp7.$GetProperty('$tmp3'), $tmp7.$GetProperty('$k')]
							),
							[$tmp6]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Call(
							null,
							$GetMember({to_C}, 'F'),
							[
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										$tmp8,
										$tmp7.$GetProperty('$tmp3')
									),
									$tmp3.$GetProperty('$i')
								),
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										$tmp8,
										$tmp7.$GetProperty('$tmp3')
									),
									$tmp3.$GetProperty('$g')
								)
							]
						),
						{sm_Expression}.$Property(
							$tmp8,
							$tmp7.$GetProperty('$k')
						),
						{sm_Int32}
					),
					[$tmp8]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void GroupJoinFollowedBySelectWhenThereIsATransparentIdentifier() {
			AssertCorrect(@"
class CJ { public int keyj; }
class CK { public int keyk; }

static CJ F1(int i) { return null; }
static int F2(int i, CJ j, IEnumerable<CK> k) { return 0; }

void M() {
	int[] arr1 = null;
	CK[] arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 let j = F1(i) join k in arr2 on j.keyj equals k.keyk into g select F2(i, j, g);
	// END
}",
@"		var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_CJ}, '$j');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp4 = {sm_Expression}.$Parameter({sm_CK}, '$k');
	var $tmp5 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp6 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({IEnumerable}, {ga_CK}), '$g');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'GroupJoin', [ga_$Anonymous, {ga_CK}, {ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
					[
						{sm_Expression}.$Convert(
							$Local('arr1', to_$Array({ga_Int32}), $arr1),
							sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp2.$GetConstructors()[0],
								[
									$tmp1,
									{sm_Expression}.$Call(null, $GetMember({to_C}, 'F1'), [$tmp1])
								],
								[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Convert(
					$Local('arr2', to_$Array({ga_CK}), $arr2),
					sm_$InstantiateGenericType({IEnumerable}, {ga_CK})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						{sm_Expression}.$Property(
							$tmp3,
							$tmp2.$GetProperty('$j')
						),
						$GetMember({to_CJ}, 'keyj')
					),
					[$tmp3]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp4,
						$GetMember({to_CK}, 'keyk')
					),
					[$tmp4]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Call(
						null,
						$GetMember({to_C}, 'F2'),
						[
							{sm_Expression}.$Property(
								$tmp5,
								$tmp2.$GetProperty('$i')
							),
							{sm_Expression}.$Property(
								$tmp5,
								$tmp2.$GetProperty('$j')
							),
							$tmp6
						]
					),
					[$tmp5, $tmp6]
				)
			]
		),
		[]
	);

");
		}

		[Test]
		public void WhereWorks() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr where i > 5 select i + 1;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Where', [{ga_Int32}]),
					[
						{sm_Expression}.$Convert(
							$Local('arr', to_$Array({ga_Int32}), $arr),
							sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$GreaterThan(
								$tmp1,
								{sm_Expression}.$Constant(5, {sm_Int32}),
								{sm_Boolean}
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						$tmp2,
						{sm_Expression}.$Constant(1, {sm_Int32}),
						{sm_Int32}
					),
					[$tmp2]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void WhereWorksWhenThereIsATransparentIdentifier() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr let j = i + 1 where i > j select i + j;
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_Int32}, '$j');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp4 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null, $GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Where', [ga_$Anonymous]),
					[
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
							[
								{sm_Expression}.$Convert(
									$Local('arr', to_$Array({ga_Int32}), $arr),
									sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$New(
										$tmp2.$GetConstructors()[0],
										[
											$tmp1,
											{sm_Expression}.$Add(
												$tmp1,
												{sm_Expression}.$Constant(1, {sm_Int32}),
												{sm_Int32}
											)
										],
										[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
									),
									[$tmp1]
								)
							]
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$GreaterThan(
								{sm_Expression}.$Property(
									$tmp3,
									$tmp2.$GetProperty('$i')
								),
								{sm_Expression}.$Property(
									$tmp3,
									$tmp2.$GetProperty('$j')
								),
								{sm_Boolean}
							),
							[$tmp3]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						{sm_Expression}.$Property(
							$tmp4,
							$tmp2.$GetProperty('$i')
						),
						{sm_Expression}.$Property(
							$tmp4,
							$tmp2.$GetProperty('$j')
						),
						{sm_Int32}
					),
					[$tmp4]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void TrivialSelectIsEliminatedAfterWhere() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr where i > 5 select i;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Where', [{ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr', to_$Array({ga_Int32}), $arr),
					sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$GreaterThan(
						$tmp1,
						{sm_Expression}.$Constant(5, {sm_Int32}),
						{sm_Boolean}
					),
					[$tmp1]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void TrivialSelectIsNotEliminatedWhenTheOnlyOperation() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr select i;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr', to_$Array({ga_Int32}), $arr),
					sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
				),
				{sm_Expression}.$Lambda(
					$tmp1,
					[$tmp1]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void OrderingWorks() {
			AssertCorrect(@"
class C1 { public int field1; }

void M() {
	C1[] arr = null;
	// BEGIN
	Expression<Func<IOrderedEnumerable<C1>>> result = () => from i in arr orderby i.field1 select i;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C1}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'OrderBy', [{ga_C1}, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					$Local('arr', to_$Array({ga_C1}), $arr), sm_$InstantiateGenericType({IEnumerable}, {ga_C1})
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp1,
						$GetMember({to_C1}, 'field1')
					),
					[$tmp1]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void OrderingWorksWhenThereIsATransparentIdentifier() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr let j = i + 1 orderby i + j select i;
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_Int32}, '$j');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp4 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_Int32}]),
			[
				{sm_Expression}.$Convert(
					{sm_Expression}.$Call(
						null,
						$GetMember({to_Enumerable}, 'OrderBy', [ga_$Anonymous, {ga_Int32}]),
						[
							{sm_Expression}.$Call(
								null,
								$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
								[
									{sm_Expression}.$Convert(
										$Local('arr', to_$Array({ga_Int32}), $arr),
										sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
									),
									{sm_Expression}.$Lambda(
										{sm_Expression}.$New(
											$tmp2.$GetConstructors()[0],
											[
												$tmp1,
												{sm_Expression}.$Add(
													$tmp1,
													{sm_Expression}.$Constant(1, {sm_Int32}),
													{sm_Int32}
												)
											],
											[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
										),
										[$tmp1]
									)
								]
							),
							{sm_Expression}.$Lambda(
								{sm_Expression}.$Add(
									{sm_Expression}.$Property(
										$tmp3,
										$tmp2.$GetProperty('$i')
									),
									{sm_Expression}.$Property(
										$tmp3,
										$tmp2.$GetProperty('$j')
									),
									{sm_Int32}
								),
								[$tmp3]
							)
						]
					),
					sm_$InstantiateGenericType({IEnumerable},ga_$Anonymous)
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Property(
						$tmp4,
						$tmp2.$GetProperty('$i')
					),
					[$tmp4]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void ThenByWorks() {
			AssertCorrect(@"
class C2 { public int field1, field2; }
void M() {
	C2[] arr = null;
	// BEGIN
	Expression<Func<IOrderedEnumerable<C2>>> result = () => from i in arr orderby i.field1, i.field2 select i;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C2}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_C2}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'ThenBy', [{ga_C2}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'OrderBy', [{ga_C2}, {ga_Int32}]),
					[
						{sm_Expression}.$Convert(
							$Local('arr', to_$Array({ga_C2}), $arr),
							sm_$InstantiateGenericType({IEnumerable}, {ga_C2})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$Field(
								$tmp1,
								$GetMember({to_C2}, 'field1')
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp2,
						$GetMember({to_C2}, 'field2')
					),
					[$tmp2]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void OrderingDescendingWorks() {
			AssertCorrect(@"
class C2 { public int field1, field2; }
void M() {
	C2[] arr = null;
	// BEGIN
	Expression<Func<IOrderedEnumerable<C2>>> result = () => from i in arr orderby i.field1 descending, i.field2 descending select i;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_C2}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_C2}, '$i');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'ThenByDescending', [{ga_C2}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'OrderByDescending', [{ga_C2}, {ga_Int32}]),
					[
						{sm_Expression}.$Convert(
							$Local('arr', to_$Array({ga_C2}), $arr),
							sm_$InstantiateGenericType({IEnumerable}, {ga_C2})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$Field(
								$tmp1,
								$GetMember({to_C2}, 'field1')
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Field(
						$tmp2, $GetMember({to_C2}, 'field2')
					),
					[$tmp2]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void QueryContinuation() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<int>>> result = () => from i in arr1 from j in arr2 select i + j into a where a > 5 select a + 1;
	// END
}",
@"	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp2 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, '$j');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, '$a');
	var $tmp5 = {sm_Expression}.$Parameter({sm_Int32}, '$a');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, {ga_Int32}]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Where', [{ga_Int32}]),
					[
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'SelectMany', [{ga_Int32}, {ga_Int32}, {ga_Int32}]),
							[
								{sm_Expression}.$Convert(
									$Local('arr1', to_$Array({ga_Int32}), $arr1),
									sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Convert(
										$Local('arr2', to_$Array({ga_Int32}), $arr2),
										sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
									),
									[$tmp1]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Add(
										$tmp2,
										$tmp3,
										{sm_Int32}
									),
									[$tmp2, $tmp3]
								)
							]
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$GreaterThan(
								$tmp4,
								{sm_Expression}.$Constant(5, {sm_Int32}),
								{sm_Boolean}
							),
							[$tmp4]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Add(
						$tmp5,
						{sm_Expression}.$Constant(1, {sm_Int32}),
						{sm_Int32}
					),
					[$tmp5]
				)
			]
		),
		[]
	);
");
		}

		[Test]
		public void NestedQueries() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<object>>> result = () => from i in arr1 from j in arr2 let l = new { i, j } group l by l.i into g select new { g.Key, a = from q in g select new { q.i, q.j } };
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_Int32}, '$j');
	var $tmp6 = sm_$Anonymous;
	var $tmp7 = $GetTransparentType($tmp2, '$tmp2', $tmp6, '$l');
	var $tmp11 = sm_$Anonymous;
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, '$j');
	var $tmp5 = {sm_Expression}.$Parameter($tmp2, '$tmp2');
	var $tmp8 = {sm_Expression}.$Parameter($tmp7, '$tmp5');
	var $tmp9 = {sm_Expression}.$Parameter($tmp7, '$tmp5');
	var $tmp10 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({IGrouping}, {ga_Int32}, ga_$Anonymous), '$g');
	var $tmp12 = {sm_Expression}.$Parameter($tmp6, '$q');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Convert(
			{sm_Expression}.$Call(
				null,
				$GetMember({to_Enumerable}, 'Select', [ga_$InstantiateGenericType({IGrouping}, {ga_Int32}, ga_$Anonymous), ga_$Anonymous]),
				[
					{sm_Expression}.$Call(
						null,
						$GetMember({to_Enumerable}, 'GroupBy', [ga_$Anonymous, {ga_Int32}, ga_$Anonymous]),
						[
							{sm_Expression}.$Call(
								null,
								$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
								[
									{sm_Expression}.$Call(
										null,
										$GetMember({to_Enumerable}, 'SelectMany', [{ga_Int32}, {ga_Int32}, ga_$Anonymous]),
										[
											{sm_Expression}.$Convert(
												$Local('arr1', to_$Array({ga_Int32}), $arr1),
												sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
											),
											{sm_Expression}.$Lambda(
												{sm_Expression}.$Convert(
													$Local('arr2', to_$Array({ga_Int32}), $arr2),
													sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
												),
												[$tmp1]
											),
											{sm_Expression}.$Lambda(
												{sm_Expression}.$New(
													$tmp2.$GetConstructors()[0],
													[$tmp3, $tmp4],
													[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
												),
												[$tmp3, $tmp4]
											)
										]
									),
									{sm_Expression}.$Lambda(
										{sm_Expression}.$New(
											$tmp7.$GetConstructors()[0],
											[
												$tmp5,
												{sm_Expression}.$New(
													$tmp6.$GetConstructors()[0],
													[
														{sm_Expression}.$Property(
															$tmp5,
															$tmp2.$GetProperty('$i')
														),
														{sm_Expression}.$Property(
															$tmp5,
															$tmp2.$GetProperty('$j')
														)
													],
													[$tmp6.$GetProperty('i'), $tmp6.$GetProperty('j')]
												)
											],
											[$tmp7.$GetProperty('$tmp2'), $tmp7.$GetProperty('$l')]
										),
										[$tmp5]
									)
								]
							),
							{sm_Expression}.$Lambda(
								{sm_Expression}.$Property(
									{sm_Expression}.$Property(
										$tmp8,
										$tmp7.$GetProperty('$l')
									),
									$tmp6.$GetProperty('i')
								),
								[$tmp8]
							),
							{sm_Expression}.$Lambda(
								{sm_Expression}.$Property(
									$tmp9,
									$tmp7.$GetProperty('$l')
								),
								[$tmp9]
							)
						]
					),
					{sm_Expression}.$Lambda(
						{sm_Expression}.$New(
							$tmp11.$GetConstructors()[0],
							[
								{sm_Expression}.$Property(
									$tmp10,
									$GetMember(to_$InstantiateGenericType({IGrouping}, {ga_Int32}, ga_$Anonymous), 'Key')
								),
								{sm_Expression}.$Call(
									null,
									$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
									[
										{sm_Expression}.$Convert(
											$tmp10,
											sm_$InstantiateGenericType({IEnumerable}, ga_$Anonymous)
										),
										{sm_Expression}.$Lambda(
											{sm_Expression}.$New(
												$tmp6.$GetConstructors()[0],
												[
													{sm_Expression}.$Property(
														$tmp12,
														$tmp6.$GetProperty('i')
													),
													{sm_Expression}.$Property(
														$tmp12,
														$tmp6.$GetProperty('j')
													)
												],
												[$tmp6.$GetProperty('i'), $tmp6.$GetProperty('j')]
											),
											[$tmp12]
										)
									]
								)
							],
							[$tmp11.$GetProperty('Key'), $tmp11.$GetProperty('a')]
						),
						[$tmp10]
					)
				]
			),
			sm_$InstantiateGenericType({IEnumerable}, {ga_Object})
		),
		[]
	);
");
		}

		[Test]
		public void NestedQueryUsingRangeVariableFromOuter() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	Expression<Func<IEnumerable<object>>> result = () => from i in arr1 from j in arr2 let k = new[] { i, j } select (from l in k let m = l + 1 select l + m + i);
	// END
}",
@"	var $tmp2 = $GetTransparentType({sm_Int32}, '$i', {sm_Int32}, '$j');
	var $tmp6 = $GetTransparentType($tmp2, '$tmp2', sm_$Array({ga_Int32}), '$k');
	var $tmp9 = $GetTransparentType({sm_Int32}, '$l', {sm_Int32}, '$m');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp3 = {sm_Expression}.$Parameter({sm_Int32}, '$i');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, '$j');
	var $tmp5 = {sm_Expression}.$Parameter($tmp2, '$tmp2');
	var $tmp7 = {sm_Expression}.$Parameter($tmp6, '$tmp5');
	var $tmp8 = {sm_Expression}.$Parameter({sm_Int32}, '$l');
	var $tmp10 = {sm_Expression}.$Parameter($tmp9, '$tmp8');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Convert(
			{sm_Expression}.$Call(
				null,
				$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$InstantiateGenericType({IEnumerable}, {ga_Int32})]),
				[
					{sm_Expression}.$Call(
						null,
						$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$Anonymous]),
						[
							{sm_Expression}.$Call(
								null,
								$GetMember({to_Enumerable}, 'SelectMany', [{ga_Int32}, {ga_Int32}, ga_$Anonymous]),
								[
									{sm_Expression}.$Convert(
										$Local('arr1', to_$Array({ga_Int32}), $arr1),
										sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
									),
									{sm_Expression}.$Lambda(
										{sm_Expression}.$Convert(
											$Local('arr2', to_$Array({ga_Int32}), $arr2),
											sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
										),
										[$tmp1]
									),
									{sm_Expression}.$Lambda(
										{sm_Expression}.$New(
											$tmp2.$GetConstructors()[0],
											[$tmp3, $tmp4],
											[$tmp2.$GetProperty('$i'), $tmp2.$GetProperty('$j')]
										),
										[$tmp3, $tmp4]
									)
								]
							),
							{sm_Expression}.$Lambda(
								{sm_Expression}.$New(
									$tmp6.$GetConstructors()[0],
									[
										$tmp5,
										{sm_Expression}.$NewArrayInit(
											{sm_Int32},
											[
												{sm_Expression}.$Property(
													$tmp5,
													$tmp2.$GetProperty('$i')
												),
												{sm_Expression}.$Property(
													$tmp5,
													$tmp2.$GetProperty('$j')
												)
											]
										)
									],
									[$tmp6.$GetProperty('$tmp2'), $tmp6.$GetProperty('$k')]
								),
								[$tmp5]
							)
						]
					),
					{sm_Expression}.$Lambda(
						{sm_Expression}.$Call(
							null,
							$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, {ga_Int32}]),
							[
								{sm_Expression}.$Call(
									null,
									$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
									[
										{sm_Expression}.$Convert(
											{sm_Expression}.$Property(
												$tmp7,
												$tmp6.$GetProperty('$k')
											),
											sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
										),
										{sm_Expression}.$Lambda(
											{sm_Expression}.$New(
												$tmp9.$GetConstructors()[0],
												[
													$tmp8,
													{sm_Expression}.$Add(
														$tmp8,
														{sm_Expression}.$Constant(1, {sm_Int32}),
														{sm_Int32}
													)
												],
												[$tmp9.$GetProperty('$l'), $tmp9.$GetProperty('$m')]
											),
											[$tmp8]
										)
									]
								),
								{sm_Expression}.$Lambda(
									{sm_Expression}.$Add(
										{sm_Expression}.$Add(
											{sm_Expression}.$Property(
												$tmp10,
												$tmp9.$GetProperty('$l')
											),
											{sm_Expression}.$Property(
												$tmp10,
												$tmp9.$GetProperty('$m')
											),
											{sm_Int32}
										),
										{sm_Expression}.$Property(
											{sm_Expression}.$Property(
												$tmp7,
												$tmp6.$GetProperty('$tmp2')
											),
											$tmp2.$GetProperty('$i')
										),
										{sm_Int32}
									),
									[$tmp10]
								)
							]
						),
						[$tmp7]
					)
				]
			),
			sm_$InstantiateGenericType({IEnumerable}, {ga_Object})
		),
		[]
	);
");
		}

		[Test]
		public void RangeVariablesAreNotInScopeInJoinEquals() {
			AssertCorrect(@"
int b;
void M() {
	int[] arr = null;
	// BEGIN
	Expression<Func<IEnumerable<IEnumerable<IEnumerable<int>>>>> result = () => from a in arr let a2 = a select (from b in arr let b2 = b join c in arr on b equals b + a into g select g);
	// END
}",
@"		var $tmp2 = $GetTransparentType({sm_Int32}, '$a', {sm_Int32}, '$a2');
	var $tmp5 = $GetTransparentType({sm_Int32}, '$b', {sm_Int32}, '$b2');
	var $tmp1 = {sm_Expression}.$Parameter({sm_Int32}, '$a');
	var $tmp3 = {sm_Expression}.$Parameter($tmp2, '$tmp1');
	var $tmp4 = {sm_Expression}.$Parameter({sm_Int32}, '$b');
	var $tmp6 = {sm_Expression}.$Parameter($tmp5, '$tmp4');
	var $tmp7 = {sm_Expression}.$Parameter({sm_Int32}, '$c');
	var $tmp8 = {sm_Expression}.$Parameter($tmp5, '$tmp4');
	var $tmp9 = {sm_Expression}.$Parameter(sm_$InstantiateGenericType({IEnumerable}, {ga_Int32}), '$g');
	var $result = {sm_Expression}.$Lambda(
		{sm_Expression}.$Call(
			null,
			$GetMember({to_Enumerable}, 'Select', [ga_$Anonymous, ga_$InstantiateGenericType({IEnumerable}, ga_$InstantiateGenericType({IEnumerable}, {ga_Int32}))]),
			[
				{sm_Expression}.$Call(
					null,
					$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
					[
						{sm_Expression}.$Convert(
							$Local('arr', to_$Array({ga_Int32}), $arr),
							sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
						),
						{sm_Expression}.$Lambda(
							{sm_Expression}.$New(
								$tmp2.$GetConstructors()[0],
								[$tmp1, $tmp1],
								[$tmp2.$GetProperty('$a'), $tmp2.$GetProperty('$a2')]
							),
							[$tmp1]
						)
					]
				),
				{sm_Expression}.$Lambda(
					{sm_Expression}.$Call(
						null,
						$GetMember({to_Enumerable}, 'GroupJoin', [ga_$Anonymous, {ga_Int32}, {ga_Int32}, ga_$InstantiateGenericType({IEnumerable}, {ga_Int32})]),
						[
							{sm_Expression}.$Call(
								null,
								$GetMember({to_Enumerable}, 'Select', [{ga_Int32}, ga_$Anonymous]),
								[
									{sm_Expression}.$Convert(
										$Local('arr', to_$Array({ga_Int32}), $arr),
										sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
									),
									{sm_Expression}.$Lambda(
										{sm_Expression}.$New(
											$tmp5.$GetConstructors()[0],
											[$tmp4, $tmp4],
											[$tmp5.$GetProperty('$b'), $tmp5.$GetProperty('$b2')]
										),
										[$tmp4]
									)
								]
							),
							{sm_Expression}.$Convert(
								$Local('arr', to_$Array({ga_Int32}), $arr),
								sm_$InstantiateGenericType({IEnumerable}, {ga_Int32})
							),
							{sm_Expression}.$Lambda(
								{sm_Expression}.$Property(
									$tmp6,
									$tmp5.$GetProperty('$b')
								),
								[$tmp6]
							),
							{sm_Expression}.$Lambda(
								{sm_Expression}.$Add(
									{sm_Expression}.$Field(
										{sm_Expression}.$Constant(this, {sm_C}),
										$GetMember({to_C}, 'b')
									),
									{sm_Expression}.$Property(
										$tmp3,
										$tmp2.$GetProperty('$a')
									),
									{sm_Int32}
								),
								[$tmp7]
							),
							{sm_Expression}.$Lambda(
								$tmp9,
								[$tmp8, $tmp9]
							)
						]
					),
					[$tmp3]
				)
			]
		),
		[]
	);
");
		}
	}
}
