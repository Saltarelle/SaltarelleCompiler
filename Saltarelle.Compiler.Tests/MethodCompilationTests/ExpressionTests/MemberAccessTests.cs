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
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableField; public void M() { int x = UnusableField; } }" }, namingConvention: new MockNamingConventionResolver { GetFieldImplementation = f => FieldImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableField")));
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
		public void ReadingNotUsablePropertyGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableProperty { get; set; } public void M() { int i = UnusableProperty; } }" }, namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableProperty")));
		}

		[Test]
		public void UsingEventAddAccessorWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h = null;
	// BEGIN
	MyEvent += h;
	// END
}",
@"	this.add_$MyEvent($h);
");
		}

		[Test]
		public void UsingEventRemoveAccessorWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h = null;
	// BEGIN
	MyEvent -= h;
	// END
}",
@"	this.remove_$MyEvent($h);
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
		public void ReadingIndexerImplementedAsMethodsWorks() {
			AssertCorrect(
@"int this[int x, int y] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	var c = this[a, b];
	// END
}",
@"	var $c = this.get_$Item($a, $b);
");
		}

		[Test]
		public void ReadingIndexerImplementedAsNativeIndexerWorks() {
			AssertCorrect(
@"int this[int x] { get { return 0; } set {} }
public void M() {
	int a = 0, b = 0;
	// BEGIN
	var c = this[a];
	// END
}",
@"	var $c = this[$a];
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => p.IsIndexer ? PropertyImplOptions.NativeIndexer() : PropertyImplOptions.Field(p.Name) });
		}

		[Test]
		public void SubscribingToNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent += null; } }" }, namingConvention: new MockNamingConventionResolver { GetEventImplementation = e => EventImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void UnsubscribingFromNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent -= null; } }" }, namingConvention: new MockNamingConventionResolver { GetEventImplementation = e => EventImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void  RaisingNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent(null, null); } }" }, namingConvention: new MockNamingConventionResolver { GetEventImplementation = e => EventImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void ReadingNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { bool b = UnusableEvent != null; } }" }, namingConvention: new MockNamingConventionResolver { GetEventImplementation = e => EventImplOptions.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void InvokingMemberWorks() {
			// Probably not here. both generic and non-generic
			Assert.Inconclusive("TODO");
		}

		[Test]
		public void AccessingDynamicMemberWorks() {
			Assert.Inconclusive("Not supported in NRefactory");
		}
	}
}
