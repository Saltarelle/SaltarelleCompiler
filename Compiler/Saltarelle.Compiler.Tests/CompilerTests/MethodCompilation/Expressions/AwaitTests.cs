using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
	[TestFixture]
	public class AwaitTests : MethodCompilerTestBase {
		[Test]
		public void AwaitWithReturnValueWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

			AssertCorrect(@"
using System;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
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
	await $tmp1:$OnCompleted;
	var $i = $tmp1.$GetResult();
", addSkeleton: false);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void AwaitWithGetAwaiterExtensionMethodWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(@"
using System;
namespace N {
	public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
		public bool IsCompleted { get { return false; } }
		public void OnCompleted(Action continuation) {}
		public int GetResult() {}
	}
	public class Awaitable {
	}
	public static class AwaitableExtensions {
		public static MyAwaiter GetAwaiter(this Awaitable a) { return null; }
	}
	public class C {
		public async void M() {
			Awaitable x = null;
			// BEGIN
			int i = await x;
			// END
		}
	}
}
",
@"	var $tmp1 = {sm_AwaitableExtensions}.$GetAwaiter($x);
	await $tmp1:$OnCompleted;
	var $i = $tmp1.$GetResult();
", addSkeleton: false);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void AwaitWithIgnoredReturnValueWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(@"
using System;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
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
	await $tmp1:$OnCompleted;
	$tmp1.$GetResult();
", addSkeleton: false);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void InlineCodeImplementationOfGetAwaiterWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(@"
using System;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
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
@"	var $tmp1 = _GetAwaiter_($x)._;
	await $tmp1:$OnCompleted;
	$tmp1.$GetResult();
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetAwaiter" ? MethodScriptSemantics.InlineCode("_GetAwaiter_({this})._") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void InlineCodeImplementationOfGetResultWorks() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(@"
using System;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
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
	await $tmp1:$OnCompleted;
	_GetResult($tmp1)._;
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetResult" ? MethodScriptSemantics.InlineCode("_GetResult({this})._") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void InlineCodeImplementationOfOnCompletedIsAnError() {
			var er = new MockErrorReporter();
			Compile(new[] { @"
using System;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
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
		await x;
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "OnCompleted" ? MethodScriptSemantics.InlineCode("_OnCompleted({this})._") : MethodScriptSemantics.NormalMethod("$" + m.Name) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(e => e.Contains("OnCompleted") && e.Contains("normal method") && e.Contains("await")));
		}

		[Test]
		public void TwoAwaitsInAnExpression() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(@"
using System;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() {}
}
public class Awaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async void M() {
		Awaitable x = null, y = null;
		// BEGIN
		int i = await x + await y;
		// END
	}
}
",
@"	var $tmp1 = $x.$GetAwaiter();
	await $tmp1:$OnCompleted;
	var $tmp3 = $tmp1.$GetResult();
	var $tmp2 = $y.$GetAwaiter();
	await $tmp2:$OnCompleted;
	var $i = $tmp3 + $tmp2.$GetResult();
", addSkeleton: false);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void AwaitingDynamicWorksAndMethodsAreCamelCased() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

			AssertCorrect(@"
using System;
public class C {
	public async void M() {
		dynamic x = null;
		// BEGIN
		dynamic i = await x;
		// END
	}
}
",
@"	var $tmp1 = $x.getAwaiter();
	await $tmp1:onCompleted;
	var $i = $tmp1.getResult();
", addSkeleton: false);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}
	}
}
