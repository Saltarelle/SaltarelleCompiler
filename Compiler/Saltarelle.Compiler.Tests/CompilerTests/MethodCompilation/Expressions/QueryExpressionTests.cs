using System;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class QueryExpressionTests : MethodCompilerTestBase {
		private static readonly Lazy<IAssemblyReference[]> _referencesLazy = new Lazy<IAssemblyReference[]>(() => new[] { Common.LoadAssemblyFile(typeof(object).Assembly.Location), Common.LoadAssemblyFile(typeof(Enumerable).Assembly.Location) });

		private void AssertCorrect(string csharp, string expected) {
			AssertCorrect(@"
using System;
using System.Collections.Generic;
using System.Linq;
class C {
	" + csharp + @"
}", expected, references: _referencesLazy.Value, addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.DeclaringTypeDefinition.FullName == "System.Linq.Enumerable" ? MethodScriptSemantics.InlineCode("{" + m.Parameters[0].Name + "}.$" + m.Name + "(" + string.Join(", ", m.Parameters.Skip(1).Select(p => "{" + p.Name + "}")) + ")") : MethodScriptSemantics.NormalMethod("$" + m.Name, ignoreGenericArguments: true), GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name, ignoreGenericArguments: true) }, runtimeLibrary: new MockRuntimeLibrary { Upcast = (e, _1, _2, _) => e });

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
		public void QueryExpressionWithSingleFromAndExplicitTypeWorks() {
			AssertCorrect(@"
void M() {
	object[] args = null;
	// BEGIN
	var result = from string a in args select int.Parse((string)a);
	// END
}",
@"	var $result = $args.$Cast().$Select(function($a) {
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
	}).$Select(function($x0) {
		return $x0.$a + $x0.$b.$ToString();
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
	}).$Select(function($x0) {
		return { $x0: $x0, $c: $x0.$b + 1 };
	}).$Select(function($x1) {
		return $x1.$x0.$a + $x1.$x0.$b.$ToString() + $x1.$c.$ToString();
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
	}, function($i2, $j) {
		return $i2 + $j;
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
	}, function($i2, $j) {
		return { $i: $i2, $j: $j };
	}).$Select(function($x0) {
		return { $x0: $x0, $k: $x0.$i + $x0.$j };
	}).$Select(function($x1) {
		return $x1.$x0.$i + $x1.$x0.$j + $x1.$k;
	});
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
	var result = from i in outer let j = F(i) from k in j.Result select i + j.X + k;
	// END
}",
@"	var $result = $outer.$Select($Bind(function($i) {
		return { $i: $i, $j: this.$F($i) };
	}, this)).$SelectMany(function($x0) {
		return $x0.$j.$Result;
	}, function($x1, $k) {
		return $x1.$i + $x1.$j.$X + $k;
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
	}, this)).$SelectMany(function($x0) {
		return $x0.$j.$Result;
	}, function($x1, $k) {
		return { $x1: $x1, $k: $k };
	}).$Select(function($x2) {
		return { $x2: $x2, $l: $x2.$x1.$i + $x2.$x1.$j.$X + $x2.$k };
	}).$Select(function($x3) {
		return $x3.$x2.$x1.$i + $x3.$x2.$x1.$j.$X + $x3.$x2.$k + $x3.$l;
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
	}, function($i2, $j) {
		return { $i: $i2, $j: $j };
	}).$SelectMany(function($x0) {
		return $arr3;
	}, function($x1, $k) {
		return $x1.$i + $x1.$j + $k;
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
	}, function($i2) {
		return $i2.$something;
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
	}, this)).$GroupBy(function($x0) {
		return $x0.$i.$field;
	}, function($x1) {
		return $x1.$i;
	});
");
		}

		[Test]
		public void JoinFollowedBySelect() {
			AssertCorrect(@"
class CI { public int keyi, valuei; }
class CJ { public int keyj, valuej; }

void M() {
	CI[] arr1;
	CJ[] arr2;
	// BEGIN
	var result = from i in arr1 join j in arr2 on i.keyi equals j.keyj select i.valuei + j.valuej;
	// END
}",
@"	var $result = $arr1.$Join($arr2, function($i) {
		return $i.$keyi;
	}, function($j) {
		return $j.$keyj;
	}, function($i2, $j2) {
		return $i2.$valuei + $j2.$valuej;
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
	}, function($i2, $j2) {
		return { $i: $i2, $j: $j2 };
	}).$Select(function($x0) {
		return { $x0: $x0, $k: $x0.$i.$valuei + $x0.$j.$valuej };
	}).$Select(function($x1) {
		return $x1.$x0.$i.$valuei + $x1.$x0.$j.$valuej + $x1.$k;
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
	}, this)).$Join($arr2, function($x0) {
		return $x0.$j.$keyj;
	}, function($k) {
		return $k.$keyk;
	}, function($x1, $k2) {
		return $x1.$i + $x1.$j.$valuej + $k2.$valuek;
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
	}, function($i2, $g) {
		return {sm_C}.$F($i2, $g);
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
	}, function($i2, $g) {
		return { $i: $i2, $g: $g };
	}).$Select(function($x0) {
		return { $x0: $x0, $k: {sm_C}.$F($x0.$i, $x0.$g) };
	}).$Select(function($x1) {
		return {sm_C}.$F($x1.$x0.$i, $x1.$x0.$g) + $x1.$k;
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
	}).$GroupJoin($arr2, function($x0) {
		return $x0.$j.$keyj;
	}, function($k) {
		return $k.$keyk;
	}, function($x1, $g) {
		return {sm_C}.$F2($x1.$i, $x1.$j, $g);
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
	}).$Select(function($i2) {
		return $i2 + 1;
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
	}).$Where(function($x0) {
		return $x0.$i > $x0.$j;
	}).$Select(function($x1) {
		return $x1.$i + $x1.$j;
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
		public void TrivialSelectIsNotEliminatingWhenTheOnlyOperation() {
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
	}).$OrderBy(function($x0) {
		return $x0.$i + $x0.$j;
	}).$Select(function($x1) {
		return $x1.$i;
	});
");
		}

		[Test]
		public void ThenByWorks() {
			AssertCorrect(@"
class C { public int field1, field2; }
void M() {
	C[] arr = null;
	// BEGIN
	var result = from i in arr orderby i.field1, i.field2 select i;
	// END
}",
@"	var $result = $arr.$OrderBy(function($i) {
		return $i.$field1;
	}).$ThenBy(function($i2) {
		return $i2.$field2;
	});
");
		}

		[Test]
		public void OrderingDescendingWorks() {
			AssertCorrect(@"
class C { public int field1, field2; }
void M() {
	C[] arr = null;
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
	}, function($i2, $j) {
		return $i2 + $j;
	}).$Where(function($a) {
		return $a > 5;
	}).$Select(function($a2) {
		return $a2 + 1;
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
	}, function($i2, $j) {
		return { $i: $i2, $j: $j };
	}).$Select(function($x0) {
		return { $x0: $x0, $l: { $i: $x0.$i, $j: $x0.$j } };
	}).$GroupBy(function($x1) {
		return $x1.$l.$i;
	}, function($x2) {
		return $x2.$l;
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
	int[] arr1 = null, arr2;
	// BEGIN
	var result = from i in arr1 from j in arr2 let k = new[] { i, j } select (from l in k let m = l + 1 select l + m + i);
	// END
}",
@"	var $result = $arr1.$SelectMany(function($i) {
		return $arr2;
	}, function($i2, $j) {
		return { $i: $i2, $j: $j };
	}).$Select(function($x0) {
		return { $x0: $x0, $k: [$x0.$i, $x0.$j] };
	}).$Select(function($x1) {
		return $x1.$k.$Select(function($l) {
			return { $l: $l, $m: $l + 1 };
		}).$Select(function($x2) {
			return $x2.$l + $x2.$m + $x1.$x0.$i;
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
	}).$Select($Bind(function($x0) {
		return $arr.$Select(function($b) {
			return { $b: $b, $b2: $b };
		}).$GroupJoin($arr, function($x1) {
			return $x1.$b;
		}, $Bind(function($c) {
			return this.$b + $x0.$a;
		}, this), function($x2, $g) {
			return $g;
		});
	}, this));
");
		}
	}
}
