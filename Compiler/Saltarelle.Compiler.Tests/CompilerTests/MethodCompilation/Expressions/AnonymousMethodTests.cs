using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class AnonymousMethodTests : MethodCompilerTestBase {
		[Test]
		public void AssigningImplicitlyTypedStatementLambdaToADelegateTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Func<int, int> f = i => { return i + 1; };
	// END
}
",
@"	// @(3, 2) - (3, 44)
	var $f = function($i) {
		// @(3, 28) - (3, 41)
		return $i + 1;
		// @(3, 42) - (3, 43)
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningExplicitlyTypedStatementLambdaToADelegateTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Func<int, int> f = (int i) => { return i + 1; };
	// END
}
",
@"	// @(3, 2) - (3, 50)
	var $f = function($i) {
		// @(3, 34) - (3, 47)
		return $i + 1;
		// @(3, 48) - (3, 49)
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningImplicitlyTypedExpressionLambdaToADelegateTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Func<int, int> f = i => i + 1;
	// END
}
",
@"	// @(3, 2) - (3, 32)
	var $f = function($i) {
		// @(3, 26) - (3, 31)
		return $i + 1;
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningImplicitlyTypedExpressionLambdaWithMultipleArgumentsToADelegateTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Func<int, int, int> f = (i, j) => i + j;
	// END
}
",
@"	// @(3, 2) - (3, 42)
	var $f = function($i, $j) {
		// @(3, 36) - (3, 41)
		return $i + $j;
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningImplicitlyTypedExpressionLambdaToADelegateTypeWithoutReturnValueWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Action<int> f = i => i++;
	// END
}
",
@"	// @(3, 2) - (3, 27)
	var $f = function($i) {
		// @(3, 23) - (3, 26)
		$i++;
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningExplicitlyTypedExpressionLambdaToADelegateTypeWithoutReturnValueWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Action<int> f = (int i) => i++;
	// END
}
",
@"	// @(3, 2) - (3, 33)
	var $f = function($i) {
		// @(3, 29) - (3, 32)
		$i++;
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningExplicitlyTypedExpressionLambdaToADelegateTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Func<int, int> f = (int i) => i + 1;
	// END
}
",
@"	// @(3, 2) - (3, 38)
	var $f = function($i) {
		// @(3, 32) - (3, 37)
		return $i + 1;
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningOldStyleAnonymousMethodToADelegateTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Func<int, int> f = delegate(int i) { return i + 1; };
	// END
}
",
@"	// @(3, 2) - (3, 55)
	var $f = function($i) {
		// @(3, 39) - (3, 52)
		return $i + 1;
		// @(3, 53) - (3, 54)
	};
", addSourceLocations: true);
		}

		[Test]
		public void AssigningOldStyleAnonymousMethodWithoutArgumentsToADelegateTypeWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	Func<int, int> f = delegate { return 1; };
	// END
}
",
@"	// @(3, 2) - (3, 44)
	var $f = function() {
		// @(3, 32) - (3, 41)
		return 1;
		// @(3, 42) - (3, 43)
	};
", addSourceLocations: true);
		}

		[Test]
		public void InvokingLambdaDirectlyWorks() {
			AssertCorrect(
@"public void M() {
	// BEGIN
	((Func<int, int>)(i => i + 1))(0);
	// END
}
",
@"	// @(3, 2) - (3, 36)
	(function($i) {
		// @(3, 25) - (3, 30)
		return $i + 1;
	})(0);
", addSourceLocations: true);
		}

		[Test]
		public void AnonymousMethodThatUsesThisWorks() {
			AssertCorrect(
@"public int x;
public void M() {
	Func<int> f;
	// BEGIN
	f = () => x;
	// END
}",
@"	// @(5, 2) - (5, 14)
	$f = $Bind(function() {
		// @(5, 12) - (5, 13)
		return this.$x;
	}, this);
", addSourceLocations: true);
		}

		[Test]
		public void StatementLambdaThatUsesByRefVariablesWorks() {
			AssertCorrect(
@"public void F(ref int a) {}
public void M() {
	int i = 0;
	Func<int> f;
	// BEGIN
	f = () => i;
	// END
	F(ref i);
}",
@"	// @(6, 2) - (6, 14)
	$f = $Bind(function() {
		// @(6, 12) - (6, 13)
		return this.$i.$;
	}, { $i: $i });
", addSourceLocations: true);
		}

		[Test]
		public void StatementLambdaThatIndirectlyUsesThisWorks() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	Action a = () => {
		Func<int> f = () => x;
	};
	// END
}",
@"	// @(4, 2) - (6, 4)
	var $a = $Bind(function() {
		// @(5, 3) - (5, 25)
		var $f = $Bind(function() {
			// @(5, 23) - (5, 24)
			return this.$x;
		}, this);
		// @(6, 2) - (6, 3)
	}, this);
", addSourceLocations: true);
		}

		[Test]
		public void StatementLambdaThatIndirectlyUsesByRefVariableWorks() {
			AssertCorrect(
@"public void F(ref int a) {}
public void M() {
	int i = 0;
	// BEGIN
	Action a = () => {
		Func<int> f = () => i;
	};
	// END
	F(ref i);
}",
@"	// @(5, 2) - (7, 4)
	var $a = $Bind(function() {
		// @(6, 3) - (6, 25)
		var $f = $Bind(function() {
			// @(6, 23) - (6, 24)
			return this.$i.$;
		}, { $i: this.$i });
		// @(7, 2) - (7, 3)
	}, { $i: $i });
", addSourceLocations: true);
		}

		[Test]
		public void StatementLambdaThatUsesThisAndByRefVariablesWorks() {
			AssertCorrect(
@"public void F(ref int a) {}
int x;
public void M() {
	int i = 0;
	Func<int> f;
	// BEGIN
	f = () => { return x + i; };
	// END
	F(ref i);
}",
@"	// @(7, 2) - (7, 30)
	$f = $Bind(function() {
		// @(7, 14) - (7, 27)
		return this.$this.$x + this.$i.$;
		// @(7, 28) - (7, 29)
	}, { $i: $i, $this: this });
", addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaThatUsesThisAndByRefVariablesWorks() {
			AssertCorrect(
@"public void F(ref int a) {}
int x;
public void M() {
	int i = 0;
	Func<int> f;
	// BEGIN
	f = () => i + x;
	// END
	F(ref i);
}",
@"	// @(7, 2) - (7, 18)
	$f = $Bind(function() {
		// @(7, 12) - (7, 17)
		return this.$i.$ + this.$this.$x;
	}, { $i: $i, $this: this });
", addSourceLocations: true);
		}

		[Test]
		public void UsingByRefVariableAsRefArgumentInsideNestedFunctionWorks() {
			AssertCorrect(
@"public void F(ref int a) {}
public void M() {
	Action f;
	int i = 0;
	// BEGIN
	f = () => { F(ref i); };
	// END
}",
@"	// @(6, 2) - (6, 26)
	$f = $Bind(function() {
		// @(6, 14) - (6, 23)
		this.$this.$F(this.$i);
		// @(6, 24) - (6, 25)
	}, { $i: $i, $this: this });
", addSourceLocations: true);
		}

		[Test]
		public void StatementLambdaDoesNotGetBoundToParametersDeclaredInsideItselfOrNestedFunctions() {
			AssertCorrect(
@"static void F(ref int a) {}
public void M() {
	int i = 0;
	Action f;
	// BEGIN
	f = () => {
		int j = 0;
		Action g = () => {
			int k = 0;
			F(ref k);
		};
		F(ref j);
	};
	// END
	F(ref i);
}",
@"	// @(6, 2) - (13, 4)
	$f = function() {
		// @(7, 3) - (7, 13)
		var $j = { $: 0 };
		// @(8, 3) - (11, 5)
		var $g = function() {
			// @(9, 4) - (9, 14)
			var $k = { $: 0 };
			// @(10, 4) - (10, 13)
			{sm_C}.$F($k);
			// @(11, 3) - (11, 4)
		};
		// @(12, 3) - (12, 12)
		{sm_C}.$F($j);
		// @(13, 2) - (13, 3)
	};
", addSourceLocations: true);
		}

		[Test]
		public void StatementLambdaThatUsesThisIsNotBoundInAStaticMethodWithThisAsFirstArgument() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	Func<int> f = () => { return x; };
	// END
}",
@"	// @(4, 2) - (4, 36)
	var $f = function() {
		// @(4, 24) - (4, 33)
		return $this.$x;
		// @(4, 34) - (4, 35)
	};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaThatUsesThisIsNotBoundInAStaticMethodWithThisAsFirstArgument() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	Func<int> f = () => x;
	// END
}",
@"	// @(4, 2) - (4, 24)
	var $f = function() {
		// @(4, 22) - (4, 23)
		return $this.$x;
	};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) }, addSourceLocations: true);
		}

		[Test]
		public void BindFirstParameterToThisWorks() {
			AssertCorrect(
@"private int i;
public void M() {
	// BEGIN
	Func<int, int, int> f = (_this, b) => _this + b + i;
	// END
}
",
@"	// @(4, 2) - (4, 54)
	var $f = $BindFirstParameterToThis($Bind(function($_this, $b) {
		// @(4, 40) - (4, 53)
		return $_this + $b + this.$i;
	}, this));
", metadataImporter: new MockMetadataImporter { GetDelegateSemantics = d => new DelegateScriptSemantics(bindThisToFirstParameter: true) }, addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaThatSetsAPropertyValueIsCorrect() {
			AssertCorrect(@"
public string P { get; set; }
public void M() {
	string s = null;
	// BEGIN
	System.Action test = () => P = ""x"";
	// END
}
",
@"	// @(6, 2) - (6, 37)
	var $test = $Bind(function() {
		// @(6, 29) - (6, 36)
		this.set_$P('x');
	}, this);
", addSourceLocations: true);
		}

		[Test]
		public void StatementLambdaClonesMutableValueTypeWhenReturning() {
			AssertCorrect(@"
struct S {}
public void M() {
	S s = default(S);
	// BEGIN
	System.Func<S> test = () => { return s; };
	// END
}
",
@"	// @(6, 2) - (6, 44)
	var $test = function() {
		// @(6, 32) - (6, 41)
		return $Clone($s, {to_S});
		// @(6, 42) - (6, 43)
	};
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaClonesMutableValueTypeWhenReturning() {
			AssertCorrect(@"
struct S {}
public void M() {
	S s = default(S);
	// BEGIN
	System.Func<S> test = () => s;
	// END
}
",
@"	// @(6, 2) - (6, 32)
	var $test = function() {
		// @(6, 30) - (6, 31)
		return $Clone($s, {to_S});
	};
", mutableValueTypes: true, addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaWithIndexerExpressionAsBody() {
			AssertCorrect(@"
int this[int x] { get { return 0; } }
public void M() {
	// BEGIN
	System.Func<int> test = () => this[0];
	// END
}
",
@"	// @(5, 2) - (5, 40)
	var $test = $Bind(function() {
		// @(5, 32) - (5, 39)
		return this.get_$Item(0);
	}, this);
", addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaWithNoReturnValue() {
			AssertCorrect(@"
private int F() { return 0; }
public void M() {
	// BEGIN
	Action test = () => F();
	// END
}
",
@"	// @(5, 2) - (5, 26)
	var $test = $Bind(function() {
		// @(5, 22) - (5, 25)
		this.$F();
	}, this);
", addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaWithImplicitConversionInReturnValue() {
			AssertCorrect(@"
private int F() { return 0; }
public void M() {
	// BEGIN
	Func<object> test = () => F();
	// END
}
",
@"	// @(5, 2) - (5, 32)
	var $test = $Bind(function() {
		// @(5, 28) - (5, 31)
		return $Upcast(this.$F(), {ct_Object});
	}, this);
", addSourceLocations: true);
		}

		[Test]
		public void ExpressionLambdaInvokingRemovedConditionalMethod() {
			AssertCorrect(@"
[System.Diagnostics.Conditional(""NOT_DEFINED"")]
private static void F() {}
public void M() {
	// BEGIN
	Action test = () => F();
	// END
}
",
@"	// @(6, 2) - (6, 26)
	var $test = function() {
		// @(6, 22) - (6, 25)
	};
", addSourceLocations: true);
		}
	}
}
