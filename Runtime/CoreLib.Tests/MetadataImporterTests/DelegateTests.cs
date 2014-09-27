using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class DelegateTests : MetadataImporterTestBase {
		[Test]
		public void BindThisToFirstParameterWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
delegate void D1(int x);

[BindThisToFirstParameter]
delegate void D2(int x);
");

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
", expectErrors: true);
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
");

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
", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("D1") && m.Contains("ExpandParamsAttribute") && m.Contains("params")));
		}

		[Test]
		public void OmitUnspecifiedArgumentsWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;
[OmitUnspecifiedArgumentsFrom(2)]
delegate void D1(int i = 0, int j = 0, int k = 0, int l = 0);
delegate void D2(int i = 0, int j = 0, int k = 0, int l = 0);");

			var d1 = FindDelegate("D1");
			Assert.That(d1.OmitUnspecifiedArgumentsFrom, Is.EqualTo(2));

			var d2 = FindDelegate("D2");
			Assert.That(d2.OmitUnspecifiedArgumentsFrom, Is.Null);
		}

		[Test]
		public void BadIndexOnArgumentToOmitUnspecifiedArgumentsFromAttributeIsAnError() {
			Prepare(
@"using System.Runtime.CompilerServices;
[OmitUnspecifiedArgumentsFrom(-1)]
delegate void D1(int i = 0, int j = 0, int k = 0, int l = 0);", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7183 && m.FormattedMessage.Contains("D1")));

			Prepare(
@"using System.Runtime.CompilerServices;
[OmitUnspecifiedArgumentsFrom(4)]
delegate void D1(int i = 0, int j = 0, int k = 0, int l = 0);", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7183 && m.FormattedMessage.Contains("D1")));

			Prepare(
@"using System.Runtime.CompilerServices;
[OmitUnspecifiedArgumentsFrom(1)]
delegate void D1(int i, int j, int k = 0, int l = 0);", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7183 && m.FormattedMessage.Contains("D1")));
		}
	}
}
