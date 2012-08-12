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
@"	var $f = function($i) {
		return $i + 1;
	};
");
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
@"	var $f = function($i) {
		return $i + 1;
	};
");
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
@"	var $f = function($i) {
		return $i + 1;
	};
");
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
@"	var $f = function($i) {
		$i++;
	};
");
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
@"	var $f = function($i) {
		$i++;
	};
");
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
@"	var $f = function($i) {
		return $i + 1;
	};
");
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
@"	var $f = function($i) {
		return $i + 1;
	};
");
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
@"	var $f = function() {
		return 1;
	};
");
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
@"	(function($i) {
		return $i + 1;
	})(0);
");
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
@"	$f = $Bind(function() {
		return this.$x;
	}, this);
");
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
@"	$f = $Bind(function() {
		return this.$i.$;
	}, { $i: $i });
");
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
@"	var $a = $Bind(function() {
		var $f = $Bind(function() {
			return this.$x;
		}, this);
	}, this);
");
		}

		[Test]
		public void StatementLambdaThatIndirectlyUsesByRefVariableWorks() {
			AssertCorrect(
@"public void F(ref int a) {}
public void M() {
	int i;
	// BEGIN
	Action a = () => {
		Func<int> f = () => i;
	};
	// END
	F(ref i);
}",
@"	var $a = $Bind(function() {
		var $f = $Bind(function() {
			return this.$i.$;
		}, { $i: this.$i });
	}, { $i: $i });
");
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
@"	$f = $Bind(function() {
		return this.$this.$x + this.$i.$;
	}, { $i: $i, $this: this });
");
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
@"	$f = $Bind(function() {
		return this.$i.$ + this.$this.$x;
	}, { $i: $i, $this: this });
");
		}

		[Test]
		public void UsingByRefVariableAsRefArgumentInsideNestedFunctionWorks() {
			AssertCorrect(
@"public void F(ref int a) {}
public void M() {
	Action f;
	int i;
	// BEGIN
	f = () => { F(ref i); }
	// END
}",
@"	$f = $Bind(function() {
		this.$this.$F(this.$i);
	}, { $i: $i, $this: this });
");
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
@"	$f = function() {
		var $j = { $: 0 };
		var $g = function() {
			var $k = { $: 0 };
			{C}.$F($k);
		};
		{C}.$F($j);
	};
");
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
@"	var $f = function() {
		return $this.$x;
	};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) });
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
@"	var $f = function() {
		return $this.$x;
	};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("$" + m.Name) });
		}
	}
}
