using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
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
@"	f = $Bind(function() { return this.x; }, this);
");
		}

		[Test]
		public void AnonymousMethodThatUsesByRefVariablesWorks() {
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
@"	f = $Bind(function() { return this.x; }, { $i: $i });
");
		}

		[Test]
		public void AnonymousMethodThatUsesThisAndByRefVariablesWorks() {
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
@"	f = $Bind(function() { return this.x; }, { $this: this, $i: $i });
");
		}

		[Test]
		public void AnonymousMethodDoesNotGetBoundToParametersDeclaredInsideItselfOrNestedFunctions() {
			AssertCorrect(
@"public void F(ref int a) {}
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
@"	f = $Bind(function() { return this.x; }, { $this: this, $i: $i });	// Not really but I'm too lazy to work it out now.
");
		}

		[Test]
		public void AnonymousMethodThatUsesThisIsNotBoundInAStaticMethodWithThisAsFirstArgument() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	Func<int> f = () => x;
	// END
}",
@"	var $f = function() { return $this.x; };
");
		}

		[Test]
		public void WorksWhenUsingThisInStaticMethodWithThisAsFirstArgument() {
			Assert.Fail("TODO");
		}
	}
}
