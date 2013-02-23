using System;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace CoreLib.Tests.MetadataImporterTests {
	[TestFixture]
	public class SerializableTypeTests : MetadataImporterTestBase {
		private void TestBothKinds(string content, Action asserter, bool expectErrors = false) {
			Prepare(@"using System; using System.Runtime.CompilerServices; [Serializable] public sealed class C1 { " + content + " }", expectErrors: expectErrors);
			asserter();

			Prepare(@"using System.Runtime.CompilerServices; public sealed class C1 : System.Record { " + content + " }", expectErrors: expectErrors);
			asserter();
		}

		[Test]
		public void TypeWithSerializableAttributeCanInheritFromObjectOrRecordOrAnotherSerializableTypeButNotFromNonSerializableType() {
			Prepare(@"using System; using System.Runtime.CompilerServices; class B {} [Serializable] class C1 : Object {}", expectErrors: false);
			// No error is good enough
			Prepare(@"using System; using System.Runtime.CompilerServices; class B {} [Serializable] class C1 : Record {}", expectErrors: false);
			// No error is good enough
			Prepare(@"using System; using System.Runtime.CompilerServices; [Serializable] class B {} [Serializable] class C1 : B {}", expectErrors: false);
			// No error is good enough

			Prepare(@"using System; using System.Runtime.CompilerServices; class B {} [Serializable] class C1 : B {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("must inherit from another serializable type, System.Object or System.Record")));
		}

		[Test]
		public void TypeWithoutSerializableAttributeCanInheritRecordAndSerializableType() {
			Prepare(@"using System; using System.Runtime.CompilerServices; class C1 : Record {}", expectErrors: false);
			// No error is good enough

			Prepare(@"using System; using System.Runtime.CompilerServices; [Serializable] class B {} class C1 : B {}", expectErrors: false);
			// No error is good enough
		}

		[Test]
		public void SerializableTypesCannotDeclareVirtualMembers() {
			TestBothKinds(@"public virtual int M1() {}", () => {
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("M1") && m.Contains("cannot declare") && m.Contains("virtual")));
			}, expectErrors: true);

			TestBothKinds(@"public virtual int P1 { get; set; }", () => {
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("P1") && m.Contains("cannot declare") && m.Contains("virtual")));
			}, expectErrors: true);
		}

		[Test]
		public void SerializableTypesCannotOverrideMembers() {
			TestBothKinds(@"public override string ToString() { return null; }", () => {
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("ToString") && m.Contains("cannot override")));
			}, expectErrors: true);
		}

		[Test]
		public void SerializableTypesCannotImplementInterfaces() {
			Prepare(@"using System; using System.Runtime.CompilerServices; interface I {} [Serializable] sealed class C1 : I {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("serializable type") && m.Contains("cannot implement interface")));

			Prepare(@"interface I {} sealed class C1 : System.Record, I {}", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("serializable type") && m.Contains("cannot implement interface")));
		}

		[Test]
		public void SerializableTypesCannotDeclareInstanceEvents() {
			Prepare(@"using System; using System.Runtime.CompilerServices; [Serializable] sealed class C1 { event System.EventHandler Evt; }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("serializable type") && m.Contains("cannot declare instance event")));

			Prepare(@"using System.Runtime.CompilerServices; sealed class C1 : System.Record { event System.EventHandler Evt; }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("C1") && m.Contains("serializable type") && m.Contains("cannot declare instance event")));

			// But static events are OK
			Prepare(@"using System.Runtime.CompilerServices; [Record] sealed class C1 { static event System.EventHandler Evt; }", expectErrors: false);
			Prepare(@"using System.Runtime.CompilerServices; sealed class C1 : System.Record { static event System.EventHandler Evt; }", expectErrors: false);
		}

		[Test]
		public void InstanceFieldsOnSerializableTypesCanBeRenamedButUsesPreserveNameIfNoOtherAttributeWasSpecified() {
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
		public void StaticFieldsOnSerializableTypesDoNotUsePreserveNameByDefault() {
			TestBothKinds("static int Field1;", () => {
				Assert.That(FindField("C1.Field1").Type, Is.EqualTo(FieldScriptSemantics.ImplType.Field));
				Assert.That(FindField("C1.Field1").Name, Is.EqualTo("$0"));
			});
		}

		[Test]
		public void InstancePropertiesOnSerializableTypesUseFieldSemanticsAndCanBeRenamedButUsesPreserveNameIfNoOtherAttributeWasSpecified() {
			TestBothKinds(@"[PreserveName] int Prop1 { get; set; } [PreserveCase] int Prop2 { get; set; } [ScriptName(""Renamed"")] int Prop3 { get; set; } int Prop4 { get; set; }", () => {
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
		public void InstancePropertiesOnSerializableTypesCanHaveInlineCode() {
			TestBothKinds(@"int Prop1 { [InlineCode(""_({this}).X"")] get; [InlineCode(""_({this})._({value})"")] set; } int Prop2 { [InlineCode(""_({this}).X2"")] get { return 0; } } int Prop3 { [InlineCode(""_({this})._({value}).X2"")] set {} }", () => {
				var prop1 = FindProperty("C1.Prop1");
				Assert.That(prop1.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
				Assert.That(prop1.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
				Assert.That(prop1.GetMethod.LiteralCode, Is.EqualTo("_({this}).X"));
				Assert.That(prop1.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
				Assert.That(prop1.SetMethod.LiteralCode, Is.EqualTo("_({this})._({value})"));

				var prop2 = FindProperty("C1.Prop2");
				Assert.That(prop2.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
				Assert.That(prop2.GetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
				Assert.That(prop2.GetMethod.LiteralCode, Is.EqualTo("_({this}).X2"));
				Assert.That(prop2.SetMethod, Is.Null);

				var prop3 = FindProperty("C1.Prop3");
				Assert.That(prop3.Type, Is.EqualTo(PropertyScriptSemantics.ImplType.GetAndSetMethods));
				Assert.That(prop3.GetMethod, Is.Null);
				Assert.That(prop3.SetMethod.Type, Is.EqualTo(MethodScriptSemantics.ImplType.InlineCode));
				Assert.That(prop3.SetMethod.LiteralCode, Is.EqualTo("_({this})._({value}).X2"));
			});
		}

		[Test]
		public void IfInlineCodeIsSpecifiedForOneAccessorOfSerializableTypeInstancePropertyItMustAlsoBeSpecifiedOnTheOther() {
			TestBothKinds(@"int Prop1 { [InlineCode(""X"")] get; set; }", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1.Prop1") && m.Contains("InlineCodeAttribute")));
			}, expectErrors: true);

			TestBothKinds(@"int Prop1 { get; [InlineCode(""X"")] set; }", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1.Prop1") && m.Contains("InlineCodeAttribute")));
			}, expectErrors: true);
		}

		[Test]
		public void ErrorInInlineCodeForSerializableTypePropertyAccessorIsReported() {
			TestBothKinds(@"int Prop1 { [InlineCode(""{a}"")] get; [InlineCode(""X"")] set; }", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1.get_Prop1") && m.Contains("{a}")));
			}, expectErrors: true);

			TestBothKinds(@"int Prop1 { [InlineCode(""X"")] get; [InlineCode(""{a}"")] set; }", () => {
				Assert.That(AllErrors.Count, Is.EqualTo(1));
				Assert.That(AllErrorTexts.Any(m => m.Contains("C1.set_Prop1") && m.Contains("{a}")));
			}, expectErrors: true);
		}

		[Test]
		public void StaticPropertiesOnSerializableTypesDoNotUseFieldSemanticsAndDoNotPreserveName() {
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
				Assert.That(m1.GeneratedMethodName, Is.EqualTo(m1.Name));

				var m2 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.Int32).Item2;
				Assert.That(m2.Type, Is.EqualTo(MethodScriptSemantics.ImplType.NormalMethod));
				Assert.That(m2.Name, Is.EqualTo("someMethod$1"));
				Assert.That(m2.GeneratedMethodName, Is.EqualTo(m2.Name));

				var m3 = methods.Single(x => x.Item1.Parameters.Count == 1 && x.Item1.Parameters[0].Type.GetDefinition().KnownTypeCode == KnownTypeCode.String).Item2;
				Assert.That(m3.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m3.Name, Is.EqualTo("someMethod$2"));
				Assert.That(m3.GeneratedMethodName, Is.EqualTo(m3.Name));

				var m4 = methods.Single(x => x.Item1.Parameters.Count == 2).Item2;
				Assert.That(m4.Type, Is.EqualTo(MethodScriptSemantics.ImplType.StaticMethodWithThisAsFirstArgument));
				Assert.That(m4.Name, Is.EqualTo("someMethod$3"));
				Assert.That(m4.GeneratedMethodName, Is.EqualTo(m4.Name));
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
		public void ConstructorForImportedSerializableTypeBecomesJsonConstructor() {
			Prepare(@"using System; using System.Runtime.CompilerServices; [Serializable, Imported] public sealed class C1 { public int MyProperty { get; set; } public int MyField; public C1(int myProperty, int myField) {} }");
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
