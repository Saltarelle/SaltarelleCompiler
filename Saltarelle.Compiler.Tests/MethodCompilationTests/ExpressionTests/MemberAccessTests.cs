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
		public void ReadingPropertyWithGetMethodImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"int P { get; set; }
public void M() {
	// BEGIN
	int i = P;
	// END
}",
@"	var $i = get_this_;
", namingConvention: new MockNamingConventionResolver { GetPropertyImplementation = p => PropertyImplOptions.GetAndSetMethods(MethodImplOptions.InlineCode("get_{this}_"), MethodImplOptions.InlineCode("set_{this}_{value}_")) });
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
		public void NonVirtualCallToPropertyAccessorsWorks() {
			AssertCorrect(
@"class B {
	public virtual int P { get; set; }
}

class D : B {
	public override int P { get; set; }

	public void M() {
		// BEGIN
		P = 1;
		int i1 = P;
		base.P = 2;
		int i2 = base.P;
		// END
	}
}",
@"	this.set_$P(1);
	var $i1 = this.get_$P();
	$CallBase({B}, 'set_$P', [], [this, 2]);
	var $i2 = $CallBase({B}, 'get_$P', [], [this]);
", addSkeleton: false);
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
		public void UsingEventAddAccessorImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h = null;
	// BEGIN
	MyEvent += h;
	// END
}",
@"	add_this_$h;
", namingConvention: new MockNamingConventionResolver() { GetEventImplementation = e => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.InlineCode("add_{this}_{value}"), MethodImplOptions.InlineCode("remove_{this}_{value}")) });
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
		public void UsingEventRemoveAccessorImplementedAsInlineCodeWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h = null;
	// BEGIN
	MyEvent -= h;
	// END
}",
@"	remove_this_$h;
", namingConvention: new MockNamingConventionResolver() { GetEventImplementation = e => EventImplOptions.AddAndRemoveMethods(MethodImplOptions.InlineCode("add_{this}_{value}"), MethodImplOptions.InlineCode("remove_{this}_{value}")) });
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
		public void NonVirtualCallToEventAccessorsWork() {
			AssertCorrect(
@"class B {
	public virtual event System.EventHandler MyEvent;
}

class D : B {
	public override event System.EventHandler MyEvent;

	public void M() {
		System.EventHandler h = null;
		// BEGIN
		MyEvent += h;
		MyEvent -= h;
		base.MyEvent += h;
		base.MyEvent -= h;
		// END
	}
}",
@"	this.add_$MyEvent($h);
	this.remove_$MyEvent($h);
	$CallBase({B}, 'add_$MyEvent', [], [this, $h]);
	$CallBase({B}, 'remove_$MyEvent', [], [this, $h]);
", addSkeleton: false);
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
		public void NonVirtualCallsToIndexerWorks() {
			AssertCorrect(
@"class B {
	public virtual int this[int a] { get { return 0; } set {} }
}

class D : B {
	public virtual int this[int a] { get { return 0; } set {} }

	public void M() {
		// BEGIN
		this[0] = 1;
		int i1 = this[2];
		base[3] = 4;
		int i2 = base[5];
		// END
	}
}",
@"	this.set_$Item(0, 1);
	var $i1 = this.get_$Item(2);
	$CallBase({B}, 'set_$Item', [], [this, 3, 4]);
	var $i2 = $CallBase({B}, 'get_$Item', [], [this, 5]);
", addSkeleton: false);
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
		public void AccessingDynamicMemberWorks() {
			Assert.Inconclusive("Not supported in NRefactory");
		}
	}
}
