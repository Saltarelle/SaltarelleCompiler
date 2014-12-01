using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
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
	public int GetResult() { return 0; }
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
@"	// @(15, 3) - (15, 19)
	var $tmp1 = $x.$GetAwaiter();
	await $tmp1:$OnCompleted;
	// @(15, 3) - (15, 19)
	var $i = $tmp1.$GetResult();
", addSkeleton: false, addSourceLocations: true);
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
		public int GetResult() { return 0; }
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
@"	// @(18, 4) - (18, 20)
	var $tmp1 = {sm_AwaitableExtensions}.$GetAwaiter($x);
	await $tmp1:$OnCompleted;
	// @(18, 4) - (18, 20)
	var $i = $tmp1.$GetResult();
", addSkeleton: false, addSourceLocations: true);
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
	public int GetResult() { return 0; }
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
@"	// @(15, 3) - (15, 11)
	var $tmp1 = $x.$GetAwaiter();
	await $tmp1:$OnCompleted;
	// @(15, 3) - (15, 11)
	$tmp1.$GetResult();
", addSkeleton: false, addSourceLocations: true);
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
	public int GetResult() { return 0; }
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
@"	// @(15, 3) - (15, 11)
	var $tmp1 = _GetAwaiter_($x)._;
	await $tmp1:$OnCompleted;
	// @(15, 3) - (15, 11)
	$tmp1.$GetResult();
", addSkeleton: false, addSourceLocations: true, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetAwaiter" ? MethodScriptSemantics.InlineCode("_GetAwaiter_({this})._") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
	public int GetResult() { return 0; }
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
@"	// @(15, 3) - (15, 11)
	var $tmp1 = $x.$GetAwaiter();
	await $tmp1:$OnCompleted;
	// @(15, 3) - (15, 11)
	_GetResult($tmp1)._;
", addSkeleton: false, addSourceLocations: true, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "GetResult" ? MethodScriptSemantics.InlineCode("_GetResult({this})._") : MethodScriptSemantics.NormalMethod("$" + m.Name) });
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
	public int GetResult() { return 0; }
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

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(e => e.FormattedMessage.Contains("OnCompleted") && e.FormattedMessage.Contains("normal method") && e.FormattedMessage.Contains("await")));
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
	public int GetResult() { return 0; }
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
@"	// @(15, 3) - (15, 29)
	var $tmp1 = $x.$GetAwaiter();
	await $tmp1:$OnCompleted;
	// @(15, 3) - (15, 29)
	var $tmp3 = $tmp1.$GetResult();
	var $tmp2 = $y.$GetAwaiter();
	await $tmp2:$OnCompleted;
	// @(15, 3) - (15, 29)
	var $i = $tmp3 + $tmp2.$GetResult();
", addSkeleton: false, addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void AwaitInExpressionLambda1() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class Awaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async void M() {
		Awaitable x = null;
		// BEGIN
		Func<Task<int>> f = async() => await x + 1, g = null;
		// END
	}
}
",
@"	// @(16, 3) - (16, 56)
	var $f = function() {
		// @(16, 34) - (16, 45)
		var $tmp1 = $x.$GetAwaiter();
		await $tmp1:$OnCompleted;
		// @(16, 34) - (16, 45)
		return $tmp1.$GetResult() + 1;
	}, $g = null;
", addSkeleton: false, addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void AwaitInExpressionLambda2() {
			try {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = true;

				AssertCorrect(@"
using System;
using System.Threading.Tasks;
public class MyAwaiter : System.Runtime.CompilerServices.INotifyCompletion {
	public bool IsCompleted { get { return false; } }
	public void OnCompleted(Action continuation) {}
	public int GetResult() { return 0; }
}
public class Awaitable {
	public MyAwaiter GetAwaiter() { return null; }
}
public class C {
	public async void M() {
		Awaitable x = null;
		// BEGIN
		Func<int, Task<int>> f = async a => await x + a, g = null;
		// END
	}
}
",
@"	// @(16, 3) - (16, 61)
	var $f = function($a) {
		// @(16, 39) - (16, 50)
		var $tmp1 = $x.$GetAwaiter();
		await $tmp1:$OnCompleted;
		// @(16, 39) - (16, 50)
		return $tmp1.$GetResult() + $a;
	}, $g = null;
", addSkeleton: false, addSourceLocations: true);
			}
			finally {
				StatementCompiler.DisableStateMachineRewriteTestingUseOnly = false;
			}
		}

		[Test]
		public void AwaitingDynamicIsAnError() {
			var er = new MockErrorReporter();
			Compile(new[] { @"
using System;
public class C {
	public async void M() {
		dynamic x = null;
		// BEGIN
		dynamic i = await x;
		// END
	}
}
" }, errorReporter: er);

			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7541 && m.FormattedMessage.Contains("dynamic")));
		}
	}
}
