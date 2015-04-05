using System.Linq;
using Microsoft.CodeAnalysis;
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
@"public class X { public int x; }
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
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableField")));
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
@"	var $i = get_(this);
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.GetAndSetMethods(MethodScriptSemantics.InlineCode("get_({this})"), MethodScriptSemantics.InlineCode("set_({this})._({value})")) });
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
@"public class X { public int F { get; set; } }
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
	$CallBase({bind_B}, '$set_P', [], [this, 2]);
	var $i2 = $CallBase({bind_B}, '$get_P', [], [this]);
", addSkeleton: false);
		}

		[Test]
		public void NonVirtualGetOfFieldLikePropertyWorks() {
			AssertCorrect(
@"class B {
	public virtual int P { get; set; }
}

class D : B {
	public override int P { get; set; }

	public void M() {
		// BEGIN
		int i1 = P;
		int i2 = base.P;
		// END
	}
}",
@"	var $i1 = this.$P;
	var $i2 = $GetBaseProperty(this, 'P');
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) });
		}

		[Test]
		public void NonVirtualGetOfFieldLikePropertyWorksStruct() {
			AssertCorrect(
@"struct S {}
class B {
	public virtual S P { get; set; }
}

class D : B {
	public override S P { get; set; }

	public void M() {
		// BEGIN
		S s1 = P;
		S s2 = base.P;
		// END
	}
}",
@"	var $s1 = $Clone(this.$P, {to_S});
	var $s2 = $Clone($GetBaseProperty(this, 'P'), {to_S});
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name), GetTypeSemantics = t => t.TypeKind == TypeKind.Struct ? TypeScriptSemantics.MutableValueType(t.Name) : TypeScriptSemantics.NormalType(t.Name) });
		}

		[Test]
		public void NonVirtualSetOfFieldLikePropertyWorks() {
			AssertCorrect(
@"class B {
	public virtual int P { get; set; }
}

class D : B {
	public override int P { get; set; }
	public int P2 { get; set; }

	public void M() {
		// BEGIN
		P = 1;
		base.P = 2;
		int i1 = base.P = 3;
		int i2 = P2 = base.P;
		int i3 = base.P = P2;
		base.P += 2;
		base.P++;
		int i4 = base.P += 2;
		int i5 = base.P++;
		int i6 = ++base.P;
		// END
	}
}",
@"	this.$P = 1;
	$SetBaseProperty(this, 'P', 2);
	$SetBaseProperty(this, 'P', 3);
	var $i1 = 3;
	var $i2 = this.$P2 = $GetBaseProperty(this, 'P');
	$SetBaseProperty(this, 'P', this.$P2);
	var $i3 = this.$P2;
	$SetBaseProperty(this, 'P', $GetBaseProperty(this, 'P') + 2);
	$SetBaseProperty(this, 'P', $GetBaseProperty(this, 'P') + 1);
	var $tmp1 = $GetBaseProperty(this, 'P') + 2;
	$SetBaseProperty(this, 'P', $tmp1);
	var $i4 = $tmp1;
	var $tmp2 = $GetBaseProperty(this, 'P');
	$SetBaseProperty(this, 'P', $tmp2 + 1);
	var $i5 = $tmp2;
	var $tmp3 = $GetBaseProperty(this, 'P') + 1;
	$SetBaseProperty(this, 'P', $tmp3);
	var $i6 = $tmp3;
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) });
		}

		[Test]
		public void NonVirtualSetOfFieldLikePropertyWorksStruct() {
			AssertCorrect(
@"struct S {
	public static S operator++(S a) { return default(S); }
	public static S operator+(S a, int b) { return default(S); }
}
class B {
	public virtual S P { get; set; }
}

class D : B {
	public override S P { get; set; }
	public S P2 { get; set; }

	public void M() {
		var s1 = default(S);
		var s2 = default(S);
		var s3 = default(S);
		// BEGIN
		P = s1;
		base.P = s2;
		var i1 = base.P = s3;
		var i2 = P2 = base.P;
		var i3 = base.P = P2;
		base.P += 2;
		base.P++;
		var i4 = base.P += 2;
		var i5 = base.P++;
		var i6 = ++base.P;
		// END
	}
}",
@"	this.$P = $Clone($s1, {to_S});
	$SetBaseProperty(this, 'P', $Clone($s2, {to_S}));
	$SetBaseProperty(this, 'P', $Clone($s3, {to_S}));
	var $i1 = $Clone($s3, {to_S});
	this.$P2 = $Clone($GetBaseProperty(this, 'P'), {to_S});
	var $i2 = $Clone(this.$P2, {to_S});
	$SetBaseProperty(this, 'P', $Clone(this.$P2, {to_S}));
	var $i3 = $Clone(this.$P2, {to_S});
	$SetBaseProperty(this, 'P', {sm_S}.op_Addition($GetBaseProperty(this, 'P'), 2));
	$SetBaseProperty(this, 'P', {sm_S}.op_Increment($GetBaseProperty(this, 'P')));
	var $tmp1 = {sm_S}.op_Addition($GetBaseProperty(this, 'P'), 2);
	$SetBaseProperty(this, 'P', $Clone($tmp1, {to_S}));
	var $i4 = $Clone($tmp1, {to_S});
	var $tmp2 = $GetBaseProperty(this, 'P');
	$SetBaseProperty(this, 'P', $Clone({sm_S}.op_Increment($Clone($tmp2, {to_S})), {to_S}));
	var $i5 = $Clone($tmp2, {to_S});
	var $tmp3 = {sm_S}.op_Increment($GetBaseProperty(this, 'P'));
	$SetBaseProperty(this, 'P', $Clone($tmp3, {to_S}));
	var $i6 = $Clone($tmp3, {to_S});
", metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name), GetTypeSemantics = t => t.TypeKind == TypeKind.Struct ? TypeScriptSemantics.MutableValueType(t.Name) : TypeScriptSemantics.NormalType(t.Name) });
		}

		[Test]
		public void ReadingNotUsablePropertyGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { int UnusableProperty { get; set; } public void M() { int i = UnusableProperty; } }" }, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableProperty")));
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
		public void UsingEventAddAccessorWorksStruct() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h = null;
	// BEGIN
	MyEvent += h;
	// END
}",
@"	this.add_$MyEvent($h);
", mutableValueTypes: true);
		}

		[Test]
		public void UsingEventAddAccessorWorksMultidimArrayStruct() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	C[,] arr = null;
	System.EventHandler h = null;
	// BEGIN
	arr[1, 2].MyEvent += h;
	// END
}",
@"	$MultidimArrayGet($arr, 1, 2).add_$MyEvent($h);
", mutableValueTypes: true);
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
@"	add_(this)._($h);
", metadataImporter: new MockMetadataImporter() { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.InlineCode("add_({this})._({value})"), MethodScriptSemantics.InlineCode("remove_({this})._({value})")) });
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
		public void UsingEventRemoveAccessorWorksStruct() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	System.EventHandler h = null;
	// BEGIN
	MyEvent -= h;
	// END
}",
@"	this.remove_$MyEvent($h);
", mutableValueTypes: true);
		}

		[Test]
		public void UsingEventRemoveAccessorWorksMultidimArrayStruct() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	C[,] arr = null;
	System.EventHandler h = null;
	// BEGIN
	arr[1, 2].MyEvent -= h;
	// END
}",
@"	$MultidimArrayGet($arr, 1, 2).remove_$MyEvent($h);
", mutableValueTypes: true);
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
@"	remove_(this)._($h);
", metadataImporter: new MockMetadataImporter() { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.InlineCode("add_({this})._({value})"), MethodScriptSemantics.InlineCode("remove_({this})._({value})")) });
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
@"	var $b = {sm_Delegate}.$op_Inequality(this.$MyEvent, null);
");
		}

		[Test]
		public void SettingEventBackingFieldWorks() {
			AssertCorrect(
@"event System.EventHandler MyEvent;
public void M() {
	// BEGIN
	MyEvent = null;
	// END
}",
@"	this.$MyEvent = null;
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
	$CallBase({bind_B}, '$add_MyEvent', [], [this, $h]);
	$CallBase({bind_B}, '$remove_MyEvent', [], [this, $h]);
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
	$CallBase({bind_B}, '$set_Item', [], [this, 3, 4]);
	var $i2 = $CallBase({bind_B}, '$get_Item', [], [this, 5]);
", addSkeleton: false);
		}

		[Test]
		public void SubscribingToNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent += null; } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void UnsubscribingFromNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent -= null; } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void  RaisingNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { UnusableEvent(null, null); } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableEvent")));
		}

		[Test]
		public void ReadingNotUsableEventGivesAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] { "class Class { event System.EventHandler UnusableEvent; public void M() { bool b = UnusableEvent != null; } }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.NotUsableFromScript() }, errorReporter: er);
			Assert.That(er.AllMessages.Any(msg => msg.Severity == DiagnosticSeverity.Error && msg.FormattedMessage.Contains("Class.UnusableEvent")));
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
			
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.FormattedMessage.Contains("C.F1") && m.FormattedMessage.Contains("cannot be assigned to")));

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
			
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.FormattedMessage.Contains("C.F1") && m.FormattedMessage.Contains("cannot be assigned to")));
		}

		[Test]
		public void UsingBaseStaticFieldFromDerivedClassWorks1() {
			AssertCorrect(@"
public class Class1 {
    public static int Test1;
}

public class Class2 : Class1 {
	static void M() {
		// BEGIN
		Test1 = Test1 + 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 = {sm_Class1}.$Test1 + 1;
", addSkeleton: false);
		}

		[Test]
		public void UsingBaseStaticFieldThroughDerivedClassWorks1() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1;
}

public class Class2 : Class1 {
}

public class C {
	static void M() {
		// BEGIN
		Class2.Test1 = Class2.Test1 + 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 = {sm_Class1}.$Test1 + 1;
", addSkeleton: false);
		}

		[Test]
		public void UsingBaseStaticFieldFromDerivedClassWorks2() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1;
}

public class Class2 : Class1 {
	static void M() {
		// BEGIN
		Test1 += 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 += 1;
", addSkeleton: false);
		}

		[Test]
		public void UsingBaseStaticFieldThroughDerivedClassWorks2() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1;
}

public class Class2 : Class1 {
}

public class C {
	static void M() {
		// BEGIN
		Class2.Test1 += 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 += 1;
", addSkeleton: false);
		}

		[Test]
		public void UsingBaseStaticPropertyFromDerivedClassWorks1() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1 { get; set; }
}

public class Class2 : Class1 {
	static void M() {
		// BEGIN
		Test1 = Test1 + 1;
		// END
	}
}",
@"	{sm_Class1}.set_$Test1({sm_Class1}.get_$Test1() + 1);
", addSkeleton: false);
		}

		[Test]
		public void UsingBaseStaticPropertyThroughDerivedClassWorks1() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1 { get; set; }
}

public class Class2 : Class1 {
}

public class C {
	static void M() {
		// BEGIN
		Class2.Test1 = Class2.Test1 + 1;
		// END
	}
}",
@"	{sm_Class1}.set_$Test1({sm_Class1}.get_$Test1() + 1);
", addSkeleton: false);
		}

		[Test]
		public void UsingBaseStaticPropertyFromDerivedClassWorks2() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1 { get; set; }
}

public class Class2 : Class1 {
	static void M() {
		// BEGIN
		Test1 = Test1 + 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 = {sm_Class1}.$Test1 + 1;
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) });
		}

		[Test]
		public void UsingBaseStaticPropertyThroughDerivedClassWorks2() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1 { get; set; }
}

public class Class2 : Class1 {
}

public class C {
	static void M() {
		// BEGIN
		Class2.Test1 = Class2.Test1 + 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 = {sm_Class1}.$Test1 + 1;
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) });
		}

		[Test]
		public void UsingBaseStaticPropertyFromDerivedClassWorks3() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1 { get; set; }
}

public class Class2 : Class1 {
	static void M() {
		// BEGIN
		Test1 += 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 += 1;
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) });
		}

		[Test]
		public void UsingBaseStaticPropertyThroughDerivedClassWorks3() {
			AssertCorrect(@"
public class Class1 {
	public static int Test1 { get; set; }
}

public class Class2 : Class1 {
}

public class C {
	static void M() {
		// BEGIN
		Class2.Test1 += 1;
		// END
	}
}",
@"	{sm_Class1}.$Test1 += 1;
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetPropertySemantics = p => PropertyScriptSemantics.Field("$" + p.Name) });
		}

		[Test]
		public void UsingBaseStaticEventFromDerivedClassWorks() {
			AssertCorrect(@"
using System;
public class Class1 {
	public static event Action Test1;
}

public class Class2 : Class1 {
	static void M() {
		Action a = null, b = null;
		// BEGIN
		Test1 += a;
		Test1 -= b;
		// END
	}
}",
@"	{sm_Class1}.add_$Test1($a);
	{sm_Class1}.remove_$Test1($b);
", addSkeleton: false);
		}

		[Test]
		public void UsingBaseStaticEventThroughDerivedClassWorks() {
			AssertCorrect(@"
using System;
public class Class1 {
	public static event Action Test1;
}

public class Class2 : Class1 {
}

public class C {
	static void M() {
		Action a = null, b = null;
		// BEGIN
		Class2.Test1 += a;
		Class2.Test1 -= b;
		// END
	}
}",
@"	{sm_Class1}.add_$Test1($a);
	{sm_Class1}.remove_$Test1($b);
", addSkeleton: false);
		}

		[Test]
		public void UsingStaticMembersInGenericClassWorks() {
			AssertCorrect(@"
using System;
public class C<T> {
	static int F;
	static void A() {}
	static event System.EventHandler E;
	static int P { get; set; }
	
	static void M() {
		// BEGIN
		F = 0;
		A();
		E += null;
		P += 1;
		// END
	}
}",
@"	sm_$InstantiateGenericType({C}, $T).$F = 0;
	sm_$InstantiateGenericType({C}, $T).$A();
	sm_$InstantiateGenericType({C}, $T).add_$E(null);
	sm_$InstantiateGenericType({C}, $T).set_$P(sm_$InstantiateGenericType({C}, $T).get_$P() + 1);
", addSkeleton: false);
		}

		[Test]
		public void CanUseStaticMembersOfGenericTypes() {
			AssertCorrect(@"
class X<T> {
	public static int F;
	public static event System.EventHandler E;
	public static int P { get; set; }
	public static void M() {}
}
public void M() {
	System.EventHandler h1 = null, h2 = null;
	// BEGIN
	X<int>.F = 10;
	int f = X<int>.F;
	X<int>.E += h1;
	X<int>.E -= h2;
	X<int>.P = 10;
	int p = X<int>.P;
	X<int>.M();
	// END
}
",
@"	sm_$InstantiateGenericType({X}, {ga_Int32}).$F = 10;
	var $f = sm_$InstantiateGenericType({X}, {ga_Int32}).$F;
	sm_$InstantiateGenericType({X}, {ga_Int32}).add_$E($h1);
	sm_$InstantiateGenericType({X}, {ga_Int32}).remove_$E($h2);
	sm_$InstantiateGenericType({X}, {ga_Int32}).set_$P(10);
	var $p = sm_$InstantiateGenericType({X}, {ga_Int32}).get_$P();
	sm_$InstantiateGenericType({X}, {ga_Int32}).$M();
");
		}
	}
}
