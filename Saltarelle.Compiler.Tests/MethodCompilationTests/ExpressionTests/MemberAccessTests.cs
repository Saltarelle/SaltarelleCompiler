using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
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
@"	var $i = this.get_$Item($a, $b);
");
		}

		[Test]
		public void ReadingIndexerImplementedAsIndexingMethodEvaluatesArgumentsInOrder() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
int P { get; set; }
public void M() {
	int a = 0;
	// BEGIN
	int i = this[P, P = a];
	// END
}",
@"	var $tmp1 = this.get_$P();
	this.set_$P($a);
	var $i = this.get_$Item($tmp1, $a);
");
		}

		[Test]
		public void ReadingPropertyImplementedAsNativeIndexerWorks() {
			AssertCorrect(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	int i = this[a];
	// END
}",
@"	var $i = this[$a];
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) });
		}

		[Test]
		public void ReadingNotUsablePropertyGivesAnError() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void InvokingMemberWorks() {
			// Probably not here. both generic and non-generic
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void UsingEventAddAccessorWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h;
	// BEGIN
	MyEvent += h;
	// END
}",
@"	add_$MyEvent(h);
");
		}

		[Test]
		public void UsingEventRemoveAccessorWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h;
	// BEGIN
	MyEvent -= h;
	// END
}",
@"	remove_$MyEvent(h);
");
		}

		[Test]
		public void InvokingEventBackingFieldWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	// BEGIN
	MyEvent(null, null);
	// END
}",
@"	this.$MyEvent(null, null);
");
		}

		[Test]
		public void AccessingEventBackingFieldWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h;
	// BEGIN
	var b = MyEvent != null;
	// END
}",
@"	var $b = $Upcast(this.$MyEvent, {MulticastDelegate}) !== null;
");
		}

		[Test]
		public void SubscribingToNotUsableEventGivesAnError() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void RaisingNotUsableEventGivesAnError() {
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void AccessingDynamicMemberWorks() {
			Assert.Inconclusive("TODO");
		}
	}
}
