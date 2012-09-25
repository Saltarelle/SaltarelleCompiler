using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class AwaitTests : MethodCompilerTestBase {
		[Test]
		public void SimpleAwaitWorks() {
			AssertCorrect(@"
using System;
public class MyAwaiter {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
}
public class Awaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async void M() {
		Awaitable x = null;
		// BEGIN
		int i = await x;
		// END
	}
}
",
@"	var $tmp1 = $x.$GetAwaiter();
	var $tmp2;
	$tmp2 = await $tmp1[$GetResult, $OnCompleted];
	var $i = $tmp2;
", addSkeleton: false);
		}

		[Test]
		public void AwaitWithIgnoredReturnValueWorks() {
			AssertCorrect(@"
using System;
public class MyAwaiter {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
}
public class Awaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async void M() {
		Awaitable x = null;
		// BEGIN
		await x;
		// END
	}
}
",
@"	var $tmp1 = $x.$GetAwaiter();
	await $tmp1[$GetResult, $OnCompleted];
", addSkeleton: false);
		}

		[Test]
		public void UsingNonMethodImplementationForGetResultIsAnError() {
		}
	}
}
