using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class FieldTests : MetadataImporterTestBase {
		[Test]
		public void FieldsWork() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class C1 {
	public int Field1;
}");

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("field1"));
		}

		[Test]
		public void NameIsPreservedForImportedTypes() {
			Prepare(
@"using System.Runtime.CompilerServices;

[Imported]
class C1 {
	int Field1;
}");

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("field1"));
		}

		[Test]
		public void FieldHidingBaseMemberGetsAUniqueName() {
			Prepare(
@"using System.Runtime.CompilerServices;

public class B {
	public int Field;
}

public class D : B {
	public new int Field;
}");

			var f1 = FindField("D.Field");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("field$1"));
		}

		[Test]
		public void RenamingFieldWorks() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	[ScriptName(""Renamed"")]
	public int Field1;
	[PreserveName]
	public int Field2;
	[PreserveCase]
	public int Field3;
}");

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("Renamed"));

			var f2 = FindField("C1.Field2");
			Assert.That(f2.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f2.Name, Is.EqualTo("field2"));

			var f3 = FindField("C1.Field3");
			Assert.That(f3.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f3.Name, Is.EqualTo("Field3"));
		}

		[Test]
		public void NonPublicFieldsArePrefixedWithADollarIfSymbolsAreNotMinimized() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public int Field1;
}

public class C2 {
	private int Field2;
	internal int Field3;
}", minimizeNames: false);

			var f1 = FindField("C1.Field1");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("$field1"));

			var f2 = FindField("C2.Field2");
			Assert.That(f2.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f2.Name, Is.EqualTo("$field2"));

			var f3 = FindField("C2.Field3");
			Assert.That(f3.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f3.Name, Is.EqualTo("$field3"));
		}

		[Test]
		public void NonScriptableAttributeCausesFieldToNotBeUsableFromScript() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[NonScriptable]
	public int Field1;
}
");

			var impl = FindField("C1.Field1");
			Assert.That(impl.Type, Is.EqualTo(FieldScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void ScriptNameCannotBeBlank() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	[ScriptName("""")]
	public int Field;
}", expectErrors: true);

			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C.Field") && m.Contains("ScriptNameAttribute") && m.Contains("field") && m.Contains("cannot be empty")));
		}

		[Test]
		public void ConstantFieldIsImplementedAsFieldAccessWhenNotMinimizingCode() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C {
	public const int MyField = 123;
}", minimizeNames: false);

			var f = FindField("C.MyField");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f.Name, Is.EqualTo("$myField"));
		}

		[Test]
		public void ConstantFieldIsImplementedAsConstantButInTheOutputWhenMinimizingCode() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class C {
	public const int MyField1 = 123;
	public const double MyField2 = 123.5;
	public const string MyField3 = ""x"";
	public const char MyField4 = 'A';
	public const object MyField5 = null;
	public const byte MyField6 = 13;
}", minimizeNames: true);

			var f = FindField("C.MyField1");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
			Assert.That(f.Value, Is.EqualTo(123.0));
			Assert.That(f.Name, Is.EqualTo("myField1"));
			Assert.That(f.GenerateCode, Is.True);

			Assert.That(FindField("C.MyField2").Value, Is.EqualTo(123.5));
			Assert.That(FindField("C.MyField3").Value, Is.EqualTo("x"));
			Assert.That(FindField("C.MyField4").Value, Is.EqualTo(65.0));
			Assert.That(FindField("C.MyField5").Value, Is.EqualTo(null));
			Assert.That(FindField("C.MyField6").Value, Is.EqualTo(13.0));
		}

		[Test]
		public void NumericValuesEnumFieldsAreTreatedAsConstants() {
			foreach (var min in new[] { true, false }) {
				Prepare(
@"using System.Runtime.CompilerServices;
public enum MyEnum {
	MyValue1 = 0,
	MyValue2,
	MyValue3 = 15,
	MyValue4,
}", minimizeNames: min);

				var f = FindField("MyEnum.MyValue1");
				Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
				Assert.That(f.Value, Is.EqualTo(0.0));
				Assert.That(f.Name, Is.EqualTo("myValue1"));
				Assert.That(f.GenerateCode, Is.True);

				Assert.That(FindField("MyEnum.MyValue2").Value, Is.EqualTo(1.0));
				Assert.That(FindField("MyEnum.MyValue3").Value, Is.EqualTo(15.0));
				Assert.That(FindField("MyEnum.MyValue4").Value, Is.EqualTo(16.0));
			}
		}

		[Test]
		public void NamedValuesValuesEnumFieldsAreTreatedAsConstantStringsWithTheValueFromTheScriptName() {
			foreach (var min in new[] { true, false }) {
				Prepare(
@"using System.Runtime.CompilerServices;
[NamedValues]
public enum MyEnum {
	MyValue1 = 0,
	[PreserveName]
	MyValue2,
	[PreserveCase]
	MyValue3 = 15,
	[ScriptName(""Renamed"")]
	MyValue4,
}", minimizeNames: min);

				var f = FindField("MyEnum.MyValue1");
				Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
				Assert.That(f.Value, Is.EqualTo("myValue1"));
				Assert.That(f.Name, Is.EqualTo("myValue1"));
				Assert.That(f.GenerateCode, Is.True);

				Assert.That(FindField("MyEnum.MyValue2").Value, Is.EqualTo("myValue2"));
				Assert.That(FindField("MyEnum.MyValue3").Value, Is.EqualTo("MyValue3"));
				Assert.That(FindField("MyEnum.MyValue4").Value, Is.EqualTo("Renamed"));
			}
		}

		[Test]
		public void InvalidIdentifierInNamedValuesEnumFieldCausesTheFieldToBeUnchangedButAffectsTheValue() {
			Prepare(
@"using System.Runtime.CompilerServices;
[NamedValues]
enum MyEnum {
	[ScriptName(""invalid-identifier"")]
	MyValue1,
}", minimizeNames: false);

			var f = FindField("MyEnum.MyValue1");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
			Assert.That(f.Value, Is.EqualTo("invalid-identifier"));
			Assert.That(f.Name, Is.EqualTo("$myValue1"));
			Assert.That(f.GenerateCode, Is.True);

			Prepare(
@"using System.Runtime.CompilerServices;
[NamedValues]
enum MyEnum {
	[ScriptName(""invalid-identifier"")]
	MyValue1,
}", minimizeNames: true);

			f = FindField("MyEnum.MyValue1");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
			Assert.That(f.Value, Is.EqualTo("invalid-identifier"));
			Assert.That(f.Name, Is.EqualTo("$0"));
			Assert.That(f.GenerateCode, Is.True);
		}

		[Test]
		public void EmptyNameInNamedValuesEnumFieldCausesTheFieldToBeUnchangedButAffectsTheValue() {
			Prepare(
@"using System.Runtime.CompilerServices;
[NamedValues]
enum MyEnum {
	[ScriptName("""")]
	MyValue1,
}", minimizeNames: false);

			var f = FindField("MyEnum.MyValue1");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
			Assert.That(f.Value, Is.EqualTo(""));
			Assert.That(f.Name, Is.EqualTo("$myValue1"));
			Assert.That(f.GenerateCode, Is.True);

			Prepare(
@"using System.Runtime.CompilerServices;
[NamedValues]
enum MyEnum {
	[ScriptName("""")]
	MyValue1,
}", minimizeNames: true);

			f = FindField("MyEnum.MyValue1");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
			Assert.That(f.Value, Is.EqualTo(""));
			Assert.That(f.Name, Is.EqualTo("$0"));
			Assert.That(f.GenerateCode, Is.True);
		}

		[Test]
		public void StaticFieldCannotBeCalledName() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class C {
	public static string Name;
}", minimizeNames: false);

			var f = FindField("C.Name");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f.Name, Is.EqualTo("name$1"));
			Assert.That(f.GenerateCode, Is.True);
		}

		[Test]
		public void InstanceFieldCannotBeCalledConstructor() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class C {
	public static string Constructor;
}", minimizeNames: false);

			var f = FindField("C.Constructor");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f.Name, Is.EqualTo("constructor$1"));
			Assert.That(f.GenerateCode, Is.True);
		}

		[Test]
		public void InlineConstantAttributeCausesAConstantImplementation() {
			Prepare(
@"using System.Runtime.CompilerServices;
public class C {
	[InlineConstant]
	public const int Value = 42;
}", minimizeNames: false);

			var f = FindField("C.Value");
			Assert.That(f.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Constant));
			Assert.That(f.Value, Is.EqualTo(42));
			Assert.That(f.GenerateCode, Is.False);
		}

		[Test]
		public void InlineConstantAttributeCannotBeAppliedToNonConstField() {
			Prepare(@"using System.Runtime.CompilerServices; public class C1 { [InlineConstant] public static int Value = 42; }", expectErrors: true);

			Assert.That(AllErrorTexts.Count, Is.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1.Value")));
			Assert.That(AllErrorTexts.Any(m => m.Contains("InlineConstantAttribute")));
		}

		[Test]
		public void ImportedEnumWithScriptNameDoesNotGenerateCodeButItsFieldsAreAccessedByName() {
			Prepare(
@"using System;
using System.Runtime.CompilerServices;

[Imported, ScriptName(""MyEnum"")]
public enum E {
	FirstValue,
	[ScriptName(""Renamed"")] SecondValue
}");

			var e = FindType("E");
			Assert.IsFalse(e.GenerateCode);
			var f1 = FindField("E.FirstValue");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("firstValue"));
			var f2 = FindField("E.SecondValue");
			Assert.That(f2.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f2.Name, Is.EqualTo("Renamed"));
		}

		[Test]
		public void NoInlineAttributeCausesConstantToNotBeInlinedInMinimizedScript() {
			Prepare("public class C { [System.Runtime.CompilerServices.NoInline] public const int X = 0; }", minimizeNames: true);

			var f1 = FindField("C.X");
			Assert.That(f1.Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
			Assert.That(f1.Name, Is.EqualTo("x"));
		}

		[Test]
		public void NoInlineAttributeCannotBeAppliedToNonConstField() {
			Prepare("public class C1 { [System.Runtime.CompilerServices.NoInline] public static int X1 = 0; }", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors.Any(m => m.Code == 7160 && m.FormattedMessage.Contains("C1.X1") && m.FormattedMessage.Contains("NoInlineAttribute")));
		}

		[Test]
		public void CustomInitializationAttributeOnNonConstFieldIsNotAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{$System.DateTime} + {value} + {T} + {this}\")] public T f; }");
			// No error is good enough
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"\")] public T f; }");
			// No error is good enough
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(null)] public T f; }");
			// No error is good enough
		}

		[Test, Category("Wait")]
		public void CustomInitializationAttributeOnConstFieldIsAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"null\")] public const T f1; }", expectErrors: true);

			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7164 && AllErrors[0].FormattedMessage.Contains("C1.f1") && AllErrors[0].FormattedMessage.Contains("const"));
		}

		[Test, Category("Wait")]
		public void ErrorInCustomInitializationAttributeCodeIsAnError() {
			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{x}\")] public T f1; }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1.f1"));

			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"{this}\")] public static T f1; }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1.f1"));

			Prepare("public class C1<T> { [System.Runtime.CompilerServices.CustomInitialization(\"a b\")] public T f1; }", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors[0].Code == 7163 && AllErrors[0].FormattedMessage.Contains("C1.f1"));
		}
	}
}