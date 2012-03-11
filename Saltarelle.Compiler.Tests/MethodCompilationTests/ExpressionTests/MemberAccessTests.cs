using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture, Ignore("TODO")]
	public class MemberAccessTests : MethodCompilerTestBase {
		[Test]
		public void ReadingFieldWorks() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	int i = x;
	// END
}",
@"	var $i = this.$x;
");
		}

		[Test]
		public void ReadingNotUsableFieldGivesAnError() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void ReadingPropertyImplementedWithMethodsWorks() {
			AssertCorrect(
@"int P { get; set; }
public void M() {
	// BEGIN
	int i = P;
	// END
}",
@"	var $i = this.get_$P();
");
		}

		[Test]
		public void ReadingPropertyImplementedAsFieldWorks() {
			AssertCorrect(
@"int F { get; set; }
public void M() {
	// BEGIN
	int i = F;
	// END
}",
@"	var $i = this.$F;
");
		}

		[Test]
		public void ReadingIndexerImplementedAsIndexingMethodWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	int i = this[a, b];
	// END
}",
@"	var $i = this.get_$Item(a, b);
");
		}


		[Test]
		public void ReadingPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	int i = this[a, b];
	// END
}",
@"	var $i = this.get_$Item(a, b);
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) });
		}

		[Test]
		public void ReadingNotUsablePropertyGivesAnError() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void ReadingMethodGroupWorks() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void InvokingMemberWorks() {
			// Probably not here
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void UsingEventAddAccessorWorks() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void UsingEventRemoveAccessorWorks() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void UsingEventRaiserWorks() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void SubscribingToNotUsableEventGivesAnError() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void RaisingNotUsableEventGivesAnError() {
			Assert.Inconclusive("TODO");
		}
	}
}
