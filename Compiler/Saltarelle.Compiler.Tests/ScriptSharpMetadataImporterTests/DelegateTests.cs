using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {
	[TestFixture]
	public class DelegateTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void BindThisToFirstParameterWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
delegate void D1(int x);

[BindThisToFirstParameter]
delegate void D2(int x);
}");

			var d1 = FindDelegate("D1");
			Assert.That(d1.BindThisToFirstParameter, Is.False);

			var d2 = FindDelegate("D2");
			Assert.That(d2.BindThisToFirstParameter, Is.True);
		}

		[Test]
		public void BindThisToFirstParameterCannotBeUsedOnDelegateWithoutParameters() {
			Prepare(
@"using System.Runtime.CompilerServices;
[BindThisToFirstParameter]
delegate void D1();
}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D1") && m.Contains("BindThisToFirstParameterAttribute") && m.Contains("does not have any parameters")));
		}

		[Test]
		public void ExpandParamsWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
delegate void D1(params int[] arr);

[ExpandParams]
delegate void D2(params int[] arr);
}");

			var d1 = FindDelegate("D1");
			Assert.That(d1.ExpandParams, Is.False);

			var d2 = FindDelegate("D2");
			Assert.That(d2.ExpandParams, Is.True);
		}

		[Test]
		public void ExpandParamsAttributeCanOnlyBeAppliedToDelegateWithParamArray() {
			Prepare(
@"using System.Runtime.CompilerServices;
[ExpandParams]
delegate void D1(int[] args);
}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D1") && m.Contains("ExpandParamsAttribute") && m.Contains("params")));
		}
	}
}
