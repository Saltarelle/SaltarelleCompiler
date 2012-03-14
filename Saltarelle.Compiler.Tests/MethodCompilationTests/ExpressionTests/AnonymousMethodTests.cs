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
	}
}
