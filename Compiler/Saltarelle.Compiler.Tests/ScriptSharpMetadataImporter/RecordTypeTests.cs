using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class RecordTypeTests : ScriptSharpMetadataImporterTestBase {
		private void TestBothKinds(string content, Action asserter, bool expectErrors = false) {
			Prepare(@"using System.Runtime.CompilerServices; [Record] public sealed class C1 { " + content + " }", expectErrors: expectErrors);
			asserter();

			Prepare(@"using System.Runtime.CompilerServices; public sealed class C1 : System.Record { " + content + " }", expectErrors: expectErrors);
			asserter();
		}

		[Test]
		public void RecordTypesMustBeSealed() {
			Prepare(@"using System.Runtime.CompilerServices; [Record] class C1 {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("record type") && m.Contains("must be sealed")));

			Prepare(@"class C1 : System.Record {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("record type") && m.Contains("must be sealed")));
		}

		[Test]
		public void TypeWithRecordAttributeMustNotHaveABaseClass() {
			Prepare(@"using System.Runtime.CompilerServices; class B {} [Record] sealed class C1 : B {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("record type") && m.Contains("must inherit from either System.Object or System.Record")));
		}

		[Test]
		public void RecordTypesCannotImplementInterfaces() {
			Prepare(@"using System.Runtime.CompilerServices; interface I {} [Record] sealed class C1 : I {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("record type") && m.Contains("cannot implement interface")));

			Prepare(@"interface I {} sealed class C1 : System.Record, I {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("record type") && m.Contains("cannot implement interface")));
		}

		[Test]
		public void RecordTypesCannotDeclareInstanceEvents() {
			Prepare(@"using System.Runtime.CompilerServices; [Record] sealed class C1 { event System.EventHandler Evt; }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("record type") && m.Contains("cannot declare instance event")));

			Prepare(@"using System.Runtime.CompilerServices; sealed class C1 : System.Record { event System.EventHandler Evt; }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("record type") && m.Contains("cannot declare instance event")));

			// But static events are OK
			Prepare(@"using System.Runtime.CompilerServices; [Record] sealed class C1 { static event System.EventHandler Evt; }", expectErrors: false);
			Prepare(@"using System.Runtime.CompilerServices; sealed class C1 : System.Record { static event System.EventHandler Evt; }", expectErrors: false);
		}

		[Test]
		public void InstanceFieldsOnRecordTypesCanBeRenamedButUsesPreserveNameIfNoOtherAttributeWasSpecified() {
			TestBothKinds(@"[PreserveName] int Field1; [PreserveCase] int Field2; [ScriptName(""Renamed"")] int Field3; int Field4;", () => {
				Assert.That(FindField("C1.Field1").Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
				Assert.That(FindField("C1.Field1").Name, Is.EqualTo("field1"));
				Assert.That(FindField("C1.Field2").Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
				Assert.That(FindField("C1.Field2").Name, Is.EqualTo("Field2"));
				Assert.That(FindField("C1.Field3").Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
				Assert.That(FindField("C1.Field3").Name, Is.EqualTo("Renamed"));
				Assert.That(FindField("C1.Field4").Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
				Assert.That(FindField("C1.Field4").Name, Is.EqualTo("field4"));
			});
		}

		[Test]
		public void StaticFieldsOnRecordTypesDoNotUsePreserveNameByDefault() {
			TestBothKinds("static int Field1;", () => {
				Assert.That(FindField("C1.Field1").Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
				Assert.That(FindField("C1.Field1").Name, Is.EqualTo("$0"));
			});
		}

		[Test]
		public void InstancePropertiessOnRecordTypesUseFieldSemanticsAndCanBeRenamedButUsesPreserveNameIfNoOtherAttributeWasSpecified() {
			TestBothKinds(@"[PreserveName] int Prop1 { get; set; } [PreserveCase] int Prop2 { get; set; } [ScriptName(""Renamed"")] int Prop3 { get; set; } int Prop4 { get; set; } }", () => {
				Assert.That(FindProperty("C1.Prop1").Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
				Assert.That(FindProperty("C1.Prop1").FieldName, Is.EqualTo("prop1"));
				Assert.That(FindProperty("C1.Prop2").Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
				Assert.That(FindProperty("C1.Prop2").FieldName, Is.EqualTo("Prop2"));
				Assert.That(FindProperty("C1.Prop3").Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
				Assert.That(FindProperty("C1.Prop3").FieldName, Is.EqualTo("Renamed"));
				Assert.That(FindProperty("C1.Prop4").Type, Is.EqualTo(PropertyScriptSemantics.ImplType.Field));
				Assert.That(FindProperty("C1.Prop4").FieldName, Is.EqualTo("prop4"));
			});
		}

		[Test]
		public void StaticPropertiesOnRecordTypesDoNotUseFieldSemanticsAndDoNotPreserveName() {
			TestBothKinds("static int Prop1 { get; set; }", () => {
				Assert.That(FindProperty("C1.Prop1").Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
				Assert.That(FindProperty("C1.Prop1").GetMethod.Name, Is.EqualTo("$0"));
				Assert.That(FindProperty("C1.Prop1").SetMethod.Name, Is.EqualTo("$1"));
			});
		}

		[Test]
		public void InstanceMethodsAreConvertedToStaticMethodsWithThisAsFirstArgumentButStaticMethodsAreNormal() {
			TestBothKinds("public void SomeMethod() {} public static void SomeMethod(int x) {} public void SomeMethod(string s) {} public void SomeMethod(int a, int b) {}", () => {
				var methods = FindMethods("C1.SomeMethod");
				var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
				Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m1.Name, Is.EqualTo("someMethod"));
				Assert.That(m1.IgnoreGenericArguments, Is.False);
				Assert.That(m1.GenerateCode, Is.True);

				var m2 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.Int32).Item2;
				Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
				Assert.That(m2.Name, Is.EqualTo("someMethod$1"));
				Assert.That(m2.IgnoreGenericArguments, Is.False);
				Assert.That(m2.GenerateCode, Is.True);

				var m3 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.String).Item2;
				Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m3.Name, Is.EqualTo("someMethod$2"));
				Assert.That(m3.IgnoreGenericArguments, Is.False);
				Assert.That(m3.GenerateCode, Is.True);

				var m4 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
				Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m4.Name, Is.EqualTo("someMethod$3"));
				Assert.That(m4.IgnoreGenericArguments, Is.False);
				Assert.That(m4.GenerateCode, Is.True);
			});
		}

		[Test]
		public void NonPublicMethodNamesAreMinimized() {
			TestBothKinds("void SomeMethod() {} static void SomeMethod(int x) {} void SomeMethod(string s) {} void SomeMethod(int a, int b) {}", () => {
				var methods = FindMethods("C1.SomeMethod");
				var m1 = methods.Single(x => x.Item1.Parameters.Count == 0).Item2;
				Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m1.Name, Is.EqualTo("$0"));

				var m2 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.Int32).Item2;
				Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
				Assert.That(m2.Name, Is.EqualTo("$1"));

				var m3 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.String).Item2;
				Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m3.Name, Is.EqualTo("$2"));

				var m4 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
				Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m4.Name, Is.EqualTo("$3"));
			});
		}

		[Test]
		public void InlineCodeAndScriptSkipAttributesCanBeUsedOnMethods() {
			TestBothKinds(@"[InlineCode(""X"")] void SomeMethod1() {} [ScriptSkip] void SomeMethod2() {}", () => {
				var m1 = FindMethod("C1.SomeMethod1");
				Assert.That(m1.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
				Assert.That(m1.LiteralCode, Is.EqualTo("X"));

				var m2 = FindMethod("C1.SomeMethod2");
				Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
				Assert.That(m2.LiteralCode, Is.EqualTo("{this}"));
			});
		}

		[Test]
		public void ConstructorsAreStaticMethodsAndCanBeNamed() {
			TestBothKinds(@"public C1() {} [ScriptName(""Renamed"")] public C1(int i) {} public C1(int i, int j) {}", () => {
				var c1 = FindConstructor("C1", 0);
				Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c1.Name, Is.EqualTo("$ctor"));
				Assert.That(c1.GenerateCode, Is.True);

				var c2 = FindConstructor("C1", 1);
				Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c2.Name, Is.EqualTo("Renamed"));
				Assert.That(c2.GenerateCode, Is.True);

				var c3 = FindConstructor("C1", 2);
				Assert.That(c3.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c3.Name, Is.EqualTo("$ctor1"));
				Assert.That(c3.GenerateCode, Is.True);
			});
		}

		[Test]
		public void NonPublicConstructorNamesAreMinimized() {
			TestBothKinds(@"C1() {} C1(int i) {} [ScriptName(""Renamed"")] C1(int i, int j) {}", () => {
				var c1 = FindConstructor("C1", 0);
				Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c1.Name, Is.EqualTo("$0"));
				Assert.That(c1.GenerateCode, Is.True);

				var c2 = FindConstructor("C1", 1);
				Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c2.Name, Is.EqualTo("$1"));
				Assert.That(c2.GenerateCode, Is.True);

				var c3 = FindConstructor("C1", 2);
				Assert.That(c3.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c3.Name, Is.EqualTo("Renamed"));
				Assert.That(c3.GenerateCode, Is.True);
			});
		}

		[Test]
		public void CanSpecifyInlineCodeForConstructor() {
			TestBothKinds(@"[InlineCode(""X"")] public C1() {}", () => {
				var c1 = FindConstructor("C1", 0);
				Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.InlineCode));
				Assert.That(c1.LiteralCode, Is.EqualTo("X"));
			});
		}

		[Test]
		public void SpecityingAnEmptyScriptNameForAConstructorMakesTheDefault() {
			TestBothKinds(@"public C1() {} public C1(int i) {} [ScriptName("""")] public C1(int i, int j) {}", () => {
				var c1 = FindConstructor("C1", 0);
				Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c1.Name, Is.EqualTo("$ctor1"));
				Assert.That(c1.GenerateCode, Is.True);

				var c2 = FindConstructor("C1", 1);
				Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c2.Name, Is.EqualTo("$ctor2"));
				Assert.That(c2.GenerateCode, Is.True);

				var c3 = FindConstructor("C1", 2);
				Assert.That(c3.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.StaticMethod));
				Assert.That(c3.Name, Is.EqualTo("$ctor"));
				Assert.That(c3.GenerateCode, Is.True);
			});
		}

		[Test]
		public void ObjectLiteralAttributeMakesTheConstructorAJsonConstructor() {
			TestBothKinds(@"public int MyProperty { get; set; } public int MyField; [ObjectLiteral] public C1(int myProperty, int myField) {} }", () => {
				var ctor = FindConstructor("C1", 2);
				Assert.That(ctor.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.Json));
				Assert.That(ctor.ParameterToMemberMap.Select(m => m.Name), Is.EqualTo(new[] { "MyProperty", "MyField" }));
			});
		}

		[Test]
		public void ConstructorForImportedRecordTypeBecomesJsonConstructor() {
			Prepare(@"using System.Runtime.CompilerServices; [Record, Imported] public sealed class C1 { public int MyProperty { get; set; } public int MyField; public C1(int myProperty, int myField) {} }");
			var ctor = FindConstructor("C1", 2);
			Assert.That(ctor.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.Json));
			Assert.That(ctor.ParameterToMemberMap.Select(m => m.Name), Is.EqualTo(new[] { "MyProperty", "MyField" }));

			Prepare(@"using System.Runtime.CompilerServices; [Imported] public sealed class C1 : System.Record { public int MyProperty { get; set; } public int MyField; public C1(int myProperty, int myField) {} }");
			ctor = FindConstructor("C1", 2);
			Assert.That(ctor.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.Json));
			Assert.That(ctor.ParameterToMemberMap.Select(m => m.Name), Is.EqualTo(new[] { "MyProperty", "MyField" }));
		}

		[Test]
		public void JsonConstructorMustHaveAllParametersMatchingMemberNamesCaseInsensitive() {
			TestBothKinds(@"[ObjectLiteral] public C1(int someParameter) {}", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("parameter") && m.Contains("matching") && m.Contains("someParameter")));
			}, expectErrors: true);
		}

		[Test]
		public void ArgumentTypesForJsonConstructorMustMatchMemberTypes() {
			TestBothKinds(@"[ObjectLiteral] public C1(int someParameter) {} public string SomeParameter;", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("someParameter") && m.Contains("System.String") && m.Contains("System.Int32")));
			}, expectErrors: true);
		}

		[Test]
		public void JsonConstructorCannotHaveRefOrOutParametersMustMatchMemberTypes() {
			TestBothKinds(@"[ObjectLiteral] public C1(ref int someParameter) {} public string SomeParameter;", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("someParameter") && m.Contains("ref")));
			}, expectErrors: true);

			TestBothKinds(@"[ObjectLiteral] public C1(out int someParameter) {} public string SomeParameter;", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("someParameter") && m.Contains("out")));
			}, expectErrors: true);
		}
	}
}
