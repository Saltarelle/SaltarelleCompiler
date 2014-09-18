using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Roslyn;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class QueryExpressionTests : MethodCompilerTestBase {
		private MockMetadataImporter CreateDefaultMetadataImporter() {
			return new MockMetadataImporter {
				GetMethodSemantics = m => {
					if (m.ContainingType.FullyQualifiedName() == "System.Linq.Enumerable") {
						if (m.Name == "Cast") {
							return MethodScriptSemantics.InlineCode("{" + m.Parameters[0].Name + "}.$Cast({" + m.TypeParameters[0].Name + "})");
						}
						else {
							return MethodScriptSemantics.InlineCode("{" + m.Parameters[0].Name + "}.$" + m.Name + "(" + string.Join(", ", m.Parameters.Skip(1).Select(p => "{" + p.Name + "}")) + ")");
						}
					}
					else {
						return MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true);
					}
				},
				GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name, ignoreGenericArguments: true)
			};
		}

		private static readonly Lazy<MetadataReference[]> _referencesLazy = new Lazy<MetadataReference[]>(() => new[] { Common.LoadAssemblyFile(typeof(object).Assembly.Location), Common.LoadAssemblyFile(typeof(Enumerable).Assembly.Location) });

		private void AssertCorrect(string csharp, string expected, IMetadataImporter metadataImporter = null) {
			AssertCorrect(@"
using System;
using System.Collections.Generic;
using System.Linq;
class C {
	" + csharp + @"
}", expected, references: _referencesLazy.Value, addSkeleton: false, metadataImporter: metadataImporter ?? CreateDefaultMetadataImporter(), runtimeLibrary: new MockRuntimeLibrary { Upcast = (e, _1, _2, _) => e });

		}

		[Test]
		public void QueryExpressionWithFromAndSelectWorks() {
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	var result = from a in args select int.Parse(a);
	// END
}",
@"	var $result = $args.$Select(function($a) {
		return {sm_Int32}.$Parse($a);
	});
");
		}

		[Test]
		public void SelectAsNormalExtensionMethod() {
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	var result = from a in args select int.Parse(a);
	// END
}",
@"	var $result = $InstantiateGenericMethod({sm_Enumerable}.Select, {ga_String}, {ga_Int32}).call(null, $args, function($a) {
		return {sm_Int32}.Parse($a);
	});
", metadataImporter: new MockMetadataImporter());
		}

		[Test]
		public void SelectAsDelegate() {
			AssertCorrect(@"
class X { public Func<Func<int, int>, int> Select { get; set; } }

void M() {
	X x = null;
	// BEGIN
	var e = from a in x select a;
	// END
}",
@"	var $result = $args.get_$Select()(function($a) {
		return {sm_Int32}.$Parse($a);
	});
");
		}

		[Test]
		public void SelectAsInstanceMethod() {
			AssertCorrect(@"
class X { public int Select(Func<int, int> f) { return 0; } }

void M() {
	X x = null;
	// BEGIN
	var e = from a in x select a;
	// END
}",
@"	var $e = $x.$Select(function($a) {
		return $a;
	});
");
		}

		[Test]
		public void SelectAsStaticMethod() {
			AssertCorrect(@"
class X { public static int Select(Func<int, int> f) { return 0; } }

void M() {
	// BEGIN
	var e = from a in X select a;
	// END
}",
@"	var $e = {sm_X}.$Select(function($a) {
		return $a;
	});
");
		}

		[Test]
		public void SelectWithBindThisToFirstArgument() {
			var metadataImporter = CreateDefaultMetadataImporter();
			metadataImporter.GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: true);

			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	var result = from a in args select int.Parse(a);
	// END
}",
@"	var $result = $args.$Select($BindFirstParameterToThis(function($a) {
		return {sm_Int32}.$Parse($a);
	}));
", metadataImporter: metadataImporter);
		}

		[Test]
		public void SelectWhichUsesThis() {
			AssertCorrect(@"
int f;
void M() {
	string[] args = null;
	// BEGIN
	var result = from a in args select int.Parse(a) + f;
	// END
}",
@"	var $result = $args.$Select($Bind(function($a) {
		return {sm_Int32}.$Parse($a) + this.$f;
	}, this));
");
		}

		[Test]
		public void SelectWhichUsesByRefArgument() {
			AssertCorrect(@"
void F(ref int p) {}
void M() {
	string[] args = null;
	int p = 0;
	F(ref p);
	// BEGIN
	var result = from a in args select int.Parse(a) + p;
	// END
}",
@"	var $result = $args.$Select($Bind(function($a) {
		return {sm_Int32}.$Parse($a) + this.$p.$;
	}, { $p: $p }));
");
		}

		[Test]
		public void SelectWhichUsesByRefArgumentAndThis() {
			AssertCorrect(@"
void F(ref int p) {}
int f;
void M() {
	string[] args = null;
	int p = 0;
	F(ref p);
	// BEGIN
	var result = from a in args select int.Parse(a) + p + f;
	// END
}",
@"	var $result = $args.$Select($Bind(function($a) {
		return {sm_Int32}.$Parse($a) + this.$p.$ + this.$this.$f;
	}, { $p: $p, $this: this }));
");
		}

		[Test]
		public void StatementLambdaInsideQueryExpression() {
			Assert.Fail("TODO, problem is that nested statement compiler (proabably) doesn't currently inherit range variable map");
		}

		[Test]
		public void QueryExpressionWithSingleFromAndExplicitTypeWorks() {
			AssertCorrect(@"
void M() {
	object[] args = null;
	// BEGIN
	var result = from string a in args select int.Parse(a);
	// END
}",
@"	var $result = $args.$Cast({ga_String}).$Select(function($a) {
		return {sm_Int32}.$Parse($a);
	});
");
		}

		[Test]
		public void QueryExpressionWithLetWorks() {
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	var result = from a in args let b = int.Parse(a) select a + b.ToString();
	// END
}",
@"	var $result = $args.$Select(function($a) {
		return { $a: $a, $b: {sm_Int32}.$Parse($a) };
	}).$Select(function($tmp1) {
		return $tmp1.$a + $tmp1.$b.$ToString();
	});
");
		}

		[Test]
		public void QueryExpressionWithLetWithBindThisToFirstArgumentWorks() {
			Assert.Fail("TODO");
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	var result = from a in args let b = int.Parse(a) select a + b.ToString();
	// END
}",
@"	var $result = $args.$Select(function($a) {
		return { $a: $a, $b: {sm_Int32}.$Parse($a) };
	}).$Select(function($tmp1) {
		return $tmp1.$a + $tmp1.$b.$ToString();
	});
");
		}

		[Test]
		public void QueryExpressionWithTwoLetsWorks() {
			AssertCorrect(@"
void M() {
	string[] args = null;
	// BEGIN
	var result = from a in args let b = int.Parse(a) let c = b + 1 select a + b.ToString() + c.ToString();
	// END
}",
@"	var $result = $args.$Select(function($a) {
		return { $a: $a, $b: {sm_Int32}.$Parse($a) };
	}).$Select(function($tmp1) {
		return { $tmp1: $tmp1, $c: $tmp1.$b + 1 };
	}).$Select(function($tmp2) {
		return $tmp2.$tmp1.$a + $tmp2.$tmp1.$b.$ToString() + $tmp2.$c.$ToString();
	});
");
		}

		[Test]
		public void TwoFromClausesFollowedBySelectWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	var result = from i in arr1 from j in arr2 select i + j;
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, function($i, $j) {
		return $i + $j;
	});
");
		}

		[Test]
		public void CastInSecondFromClauseWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	var result = from i in arr1 from int j in arr2 select i + j;
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2.$Cast({ga_Int32});
	}, function($i, $j) {
		return $i + $j;
	});
");
		}

		[Test]
		public void TwoFromClausesFollowedByLetWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	var result = from i in arr1 from j in arr2 let k = i + j select i + j + k;
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, function($i, $j) {
		return { $i: $i, $j: $j };
	}).$Select(function($tmp1) {
		return { $tmp1: $tmp1, $k: $tmp1.$i + $tmp1.$j };
	}).$Select(function($tmp2) {
		return $tmp2.$tmp1.$i + $tmp2.$tmp1.$j + $tmp2.$k;
	});
");
		}

		[Test]
		public void TwoFromClausesFollowedByLetWorksWithBindThisToFirstParameter() {
			var metadataImporter = CreateDefaultMetadataImporter();
			metadataImporter.GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: d.DelegateInvokeMethod.Parameters.Length == 2);

			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	var result = from i in arr1 from j in arr2 let k = i + j select i + j + k;
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, $BindFirstParameterToThis(function($i, $j) {
		return { $i: $i, $j: $j };
	})).$Select(function($tmp1) {
		return { $tmp1: $tmp1, $k: $tmp1.$i + $tmp1.$j };
	}).$Select(function($tmp2) {
		return $tmp2.$tmp1.$i + $tmp2.$tmp1.$j + $tmp2.$k;
	});
", metadataImporter);
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
	var result = from i in outer let j = F(i) from k in j.Result select i + j.X + k;
	// END
}",
@"	var $result = $outer.$Select($Bind(function($i) {
		return { $i: $i, $j: this.$F($i) };
	}, this)).$SelectMany(function($tmp1) {
		return $tmp1.$j.$Result;
	}, function($tmp1, $k) {
		return $tmp1.$i + $tmp1.$j.$X + $k;
	});
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
	var result = from i in outer let j = F(i) from k in j.Result let l = i + j.X + k select i + j.X + k + l;
	// END
}",
@"	var $result = $outer.$Select($Bind(function($i) {
		return { $i: $i, $j: this.$F($i) };
	}, this)).$SelectMany(function($tmp1) {
		return $tmp1.$j.$Result;
	}, function($tmp1, $k) {
		return { $tmp1: $tmp1, $k: $k };
	}).$Select(function($tmp2) {
		return { $tmp2: $tmp2, $l: $tmp2.$tmp1.$i + $tmp2.$tmp1.$j.$X + $tmp2.$k };
	}).$Select(function($tmp3) {
		return $tmp3.$tmp2.$tmp1.$i + $tmp3.$tmp2.$tmp1.$j.$X + $tmp3.$tmp2.$k + $tmp3.$l;
	});
");
		}

		[Test]
		public void ThreeFromClausesFollowedBySelectWorks() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null, arr3 = null;
	// BEGIN
	var result = from i in arr1 from j in arr2 from k in arr3 select i + j + k;
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, function($i, $j) {
		return { $i: $i, $j: $j };
	}).$SelectMany(function($tmp1) {
		return $arr3;
	}, function($tmp1, $k) {
		return $tmp1.$i + $tmp1.$j + $k;
	});
");
		}

		[Test]
		public void GroupByWithSimpleValue() {
			AssertCorrect(@"
class C1 { public int field; }

void M() {
	C1[] arr = null;
	// BEGIN
	var result = from i in arr group i by i.field;
	// END
}",
@"	var $result = $arr.$GroupBy(function($i) {
		return $i.$field;
	});
");
		}

		[Test]
		public void GroupByWithProjectedValue() {
			AssertCorrect(@"
class C1 { public int field, something; }

void M() {
	C1[] arr = null;
	// BEGIN
	var result = from i in arr group i.something by i.field;
	// END
}",
@"	var $result = $arr.$GroupBy(function($i) {
		return $i.$field;
	}, function($i) {
		return $i.$something;
	});
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
	var result = from i in arr let j = F(i) group i by i.field;
	// END
}",
@"	var $result = $arr.$Select($Bind(function($i) {
		return { $i: $i, $j: this.$F($i) };
	}, this)).$GroupBy(function($tmp1) {
		return $tmp1.$i.$field;
	}, function($tmp1) {
		return $tmp1.$i;
	});
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
	var result = from i in arr1 join CJ j in arr2 on i.keyi equals j.keyj select i.valuei + j.valuej;
	// END
}",
@"	var $result = $arr1.$Join($arr2.$Cast({ga_CJ}), function($i) {
		return $i.$keyi;
	}, function($j) {
		return $j.$keyj;
	}, function($i, $j) {
		return $i.$valuei + $j.$valuej;
	});
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
	var result = from i in arr1 join j in arr2 on i.keyi equals j.keyj select i.valuei + j.valuej;
	// END
}",
@"	var $result = $arr1.$Join($arr2, function($i) {
		return $i.$keyi;
	}, function($j) {
		return $j.$keyj;
	}, function($i, $j) {
		return $i.$valuei + $j.$valuej;
	});
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
	var result = from i in arr1 join j in arr2 on i.keyi equals j.keyj let k = i.valuei + j.valuej select i.valuei + j.valuej + k;
	// END
}",
@"	var $result = $arr1.$Join($arr2, function($i) {
		return $i.$keyi;
	}, function($j) {
		return $j.$keyj;
	}, function($i, $j) {
		return { $i: $i, $j: $j };
	}).$Select(function($tmp1) {
		return { $tmp1: $tmp1, $k: $tmp1.$i.$valuei + $tmp1.$j.$valuej };
	}).$Select(function($tmp2) {
		return $tmp2.$tmp1.$i.$valuei + $tmp2.$tmp1.$j.$valuej + $tmp2.$k;
	});
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
	var result = from i in arr1 let j = F(i) join k in arr2 on j.keyj equals k.keyk select i + j.valuej + k.valuek;
	// END
}",
@"	var $result = $arr1.$Select($Bind(function($i) {
		return { $i: $i, $j: this.$F($i) };
	}, this)).$Join($arr2, function($tmp1) {
		return $tmp1.$j.$keyj;
	}, function($k) {
		return $k.$keyk;
	}, function($tmp1, $k) {
		return $tmp1.$i + $tmp1.$j.$valuej + $k.$valuek;
	});
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
	var result = from i in arr1 join j in arr2 on i.keyi equals j.keyj into g select F(i, g);
	// END
}",
@"	var $result = $arr1.$GroupJoin($arr2, function($i) {
		return $i.$keyi;
	}, function($j) {
		return $j.$keyj;
	}, function($i, $g) {
		return {sm_C}.$F($i, $g);
	});
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
	var result = from i in arr1 join j in arr2 on i.keyi equals j.keyj into g let k = F(i, g) select F(i, g) + k;
	// END
}",
@"	var $result = $arr1.$GroupJoin($arr2, function($i) {
		return $i.$keyi;
	}, function($j) {
		return $j.$keyj;
	}, function($i, $g) {
		return { $i: $i, $g: $g };
	}).$Select(function($tmp1) {
		return { $tmp1: $tmp1, $k: {sm_C}.$F($tmp1.$i, $tmp1.$g) };
	}).$Select(function($tmp2) {
		return {sm_C}.$F($tmp2.$tmp1.$i, $tmp2.$tmp1.$g) + $tmp2.$k;
	});
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
	var result = from i in arr1 let j = F1(i) join k in arr2 on j.keyj equals k.keyk into g select F2(i, j, g);
	// END
}",
@"	var $result = $arr1.$Select(function($i) {
		return { $i: $i, $j: {sm_C}.$F1($i) };
	}).$GroupJoin($arr2, function($tmp1) {
		return $tmp1.$j.$keyj;
	}, function($k) {
		return $k.$keyk;
	}, function($tmp1, $g) {
		return {sm_C}.$F2($tmp1.$i, $tmp1.$j, $g);
	});
");
		}

		[Test]
		public void WhereWorks() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	var result = from i in arr where i > 5 select i + 1;
	// END
}",
@"	var $result = $arr.$Where(function($i) {
		return $i > 5;
	}).$Select(function($i) {
		return $i + 1;
	});
");
		}

		[Test]
		public void WhereWorksWhenThereIsATransparentIdentifier() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	var result = from i in arr let j = i + 1 where i > j select i + j;
	// END
}",
@"	var $result = $arr.$Select(function($i) {
		return { $i: $i, $j: $i + 1 };
	}).$Where(function($tmp1) {
		return $tmp1.$i > $tmp1.$j;
	}).$Select(function($tmp1) {
		return $tmp1.$i + $tmp1.$j;
	});
");
		}

		[Test]
		public void TrivialSelectIsEliminatedAfterWhere() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	var result = from i in arr where i > 5 select i;
	// END
}",
@"	var $result = $arr.$Where(function($i) {
		return $i > 5;
	});
");
		}

		[Test]
		public void TrivialSelectIsNotEliminatedWhenTheOnlyOperation() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	var result = from i in arr select i;
	// END
}",
@"	var $result = $arr.$Select(function($i) {
		return $i;
	});
");
		}

		[Test]
		public void OrderingWorks() {
			AssertCorrect(@"
class C1 { public int field1; }

void M() {
	C1[] arr = null;
	// BEGIN
	var result = from i in arr orderby i.field1 select i;
	// END
}",
@"	var $result = $arr.$OrderBy(function($i) {
		return $i.$field1;
	});
");
		}

		[Test]
		public void OrderingWorksWhenThereIsATransparentIdentifier() {
			AssertCorrect(@"
void M() {
	int[] arr = null;
	// BEGIN
	var result = from i in arr let j = i + 1 orderby i + j select i;
	// END
}",
@"	var $result = $arr.$Select(function($i) {
		return { $i: $i, $j: $i + 1 };
	}).$OrderBy(function($tmp1) {
		return $tmp1.$i + $tmp1.$j;
	}).$Select(function($tmp1) {
		return $tmp1.$i;
	});
");
		}

		[Test]
		public void ThenByWorks() {
			AssertCorrect(@"
class C2 { public int field1, field2; }
void M() {
	C2[] arr = null;
	// BEGIN
	var result = from i in arr orderby i.field1, i.field2 select i;
	// END
}",
@"	var $result = $arr.$OrderBy(function($i) {
		return $i.$field1;
	}).$ThenBy(function($i) {
		return $i.$field2;
	});
");
		}

		[Test]
		public void OrderingDescendingWorks() {
			AssertCorrect(@"
class C2 { public int field1, field2; }
void M() {
	C2[] arr = null;
	// BEGIN
	var result = from i in arr orderby i.field1 descending, i.field2 descending select i;
	// END
}",
@"	var $result = $arr.$OrderByDescending(function($i) {
		return $i.$field1;
	}).$ThenByDescending(function($i2) {
		return $i2.$field2;
	});
");
		}

		[Test]
		public void QueryContinuation() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	var result = from i in arr1 from j in arr2 select i + j into a where a > 5 select a + 1;
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, function($i, $j) {
		return $i + $j;
	}).$Where(function($a) {
		return $a > 5;
	}).$Select(function($a) {
		return $a + 1;
	});
");
		}

		[Test]
		public void NestedQueries() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	var result = from i in arr1 from j in arr2 let l = new { i, j } group l by l.i into g select new { g.Key, a = from q in g select new { q.i, q.j } };
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, function($i, $j) {
		return { $i: $i, $j: $j };
	}).$Select(function($tmp1) {
		return { $tmp1: $tmp1, $l: { $i: $tmp1.$i, $j: $tmp1.$j } };
	}).$GroupBy(function($tmp2) {
		return $tmp2.$l.$i;
	}, function($tmp2) {
		return $tmp2.$l;
	}).$Select(function($g) {
		return { $Key: $g.get_Key(), $a: $g.$Select(function($q) {
			return { $i: $q.$i, $j: $q.$j };
		}) };
	});
");
		}

		[Test]
		public void NestedQueryUsingRangeVariableFromOuter() {
			AssertCorrect(@"
void M() {
	int[] arr1 = null, arr2 = null;
	// BEGIN
	var result = from i in arr1 from j in arr2 let k = new[] { i, j } select (from l in k let m = l + 1 select l + m + i);
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, function($i2, $j) {
		return { $i: $i2, $j: $j };
	}).$Select(function($tmp1) {
		return { $tmp1: $tmp1, $k: [$tmp1.$i, $tmp1.$j] };
	}).$Select(function($tmp2) {
		return $tmp2.$k.$Select(function($l) {
			return { $l: $l, $m: $l + 1 };
		}).$Select(function($tmp3) {
			return $tmp3.$l + $tmp3.$m + $tmp2.$tmp1.$i;
		});
	});
");
		}

		[Test]
		public void RangeVariablesAreNotInScopeInJoinEquals() {
			AssertCorrect(@"
int b;
void M() {
	int[] arr = null;
	// BEGIN
	var result = from a in arr let a2 = a select (from b in arr let b2 = b join c in arr on b equals b + a into g select g);
	// END
}",
@"	var $result = $arr.$Select(function($a) {
		return { $a: $a, $a2: $a };
	}).$Select($Bind(function($tmp1) {
		return $arr.$Select(function($b) {
			return { $b: $b, $b2: $b };
		}).$GroupJoin($arr, function($tmp2) {
			return $tmp2.$b;
		}, $Bind(function($c) {
			return this.$b + $tmp1.$a;
		}, this), function($tmp3, $g) {
			return $g;
		});
	}, this));
");
		}

		[Test]
		public void ExpressionAreEvaluatedInTheCorrectOrderWhenAJoinClauseRequiresAdditionalStatements() {
			Assert.Fail("TODO. Might require bind (which the current implementation does not do");
		}
	}
}
