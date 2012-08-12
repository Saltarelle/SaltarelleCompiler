using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation.Expressions {
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
		public void ReadingFieldReturnedByAMethodWorks() {
			AssertCorrect(
@"class X { public int x; }
public X F() { return null; }
public void M() {
	// BEGIN
	int i = F().x;
	// END
}",
@"	var $i = this.$F().$x;
");
		}

		[Test]
		public void ReadingDynamicMemberWorks() {
			AssertCorrect(
@"public void M() {
	dynamic d = null;
	// BEGIN
	object o = d.someField;
	// END
}",
@"	var $o = $d.someField;
");
		}

		[Test]
		public void ReadingNotUsableFieldGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableField; public void M() { int x = UnusableField; } }" }, metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => FieldScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableField")));
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
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_{this}_"), MethodScriptSemantics.InlineCode("set_{this}_{value}_")) });
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
		public void ReadingPropertyImplementedAsFieldReturnedByAMethodWorks() {
			AssertCorrect(
@"class X { public int F { get; set; } }
public X F() { return null; }
public void M() {
	// BEGIN
	int i = F().F;
	// END
}",
@"	var $i = this.$F().$F;
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
			Compile(new[] { "class Class { int UnusableProperty { get; set; } public void M() { int i = UnusableProperty; } }" }, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableProperty")));
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
", metadataImporter: new MockMetadataImporter() { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.InlineCode("add_{this}_{value}"), MethodScriptSemantics.InlineCode("remove_{this}_{value}")) });
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
", metadataImporter: new MockMetadataImporter() { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.InlineCode("add_{this}_{value}"), MethodScriptSemantics.InlineCode("remove_{this}_{value}")) });
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
@"	var $b = {MulticastDelegate}.$op_Inequality($Upcast(this.$MyEvent, {MulticastDelegate}), null);
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
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => p.IsIndexer ? PropertyScriptSemantics.NativeIndexer() : PropertyScriptSemantics.Field(p.Name) });
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
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent += null; } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void UnsubscribingFromNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent -= null; } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void  RaisingNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent(null, null); } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void ReadingNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { bool b = UnusableEvent != null; } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessagesText.Any(m => m.StartsWith("Error:") && m.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void ReadingFieldImplementedAsConstantWorks() {
			AssertCorrect(
@"
public int F1;
public int F2;
public int F3;
public int F4;

public void M() {
	// BEGIN
	var f1 = F1;
	var f2 = F2;
	var f3 = F3;
	var f4 = F4;
	// END
}",
@"	var $f1 = null;
	var $f2 = 'abcd';
	var $f3 = 1234.5;
	var $f4 = true;
", metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => f.Name == "F1" ? FieldScriptSemantics.NullConstant() : (f.Name == "F2" ? FieldScriptSemantics.StringConstant("abcd") : (f.Name == "F3" ? FieldScriptSemantics.NumericConstant(1234.5) : (f.Name == "F4" ? FieldScriptSemantics.BooleanConstant(true) : FieldScriptSemantics.Field(f.Name)))) });
		}

		[Test]
		public void AssigningToFieldImplementedAsConstantIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public int F1;
	public void M() {
		// BEGIN
		F1 = 1;
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => f.Name == "F1" ? FieldScriptSemantics.NullConstant() : FieldScriptSemantics.Field(f.Name) }, errorReporter: er);
			
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.Contains("C.F1") && m.Contains("cannot be assigned to")));

			er = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public int F1;
	public void M() {
		// BEGIN
		F1 += 1;
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetFieldSemantics = f => f.Name == "F1" ? FieldScriptSemantics.NullConstant() : FieldScriptSemantics.Field(f.Name) }, errorReporter: er);
			
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.Contains("C.F1") && m.Contains("cannot be assigned to")));
		}
	}
}
