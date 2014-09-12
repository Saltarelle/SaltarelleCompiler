using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Roslyn;

namespace Saltarelle.Compiler.Tests.CompilerTests {
	[TestFixture]
	public class TypeConversionTests : CompilerTestBase {
		[Test]
		public void EmptyGlobalClassIsCorrectlyReturned() {
			Compile(@"class TestClass { }");
			Assert.That(CompiledTypes, Has.Count.EqualTo(1));
			Assert.That(CompiledTypes[0], Is.AssignableTo<JsClass>());
		}

		[Test]
		public void NestedClassesWork() {
			Compile(@"class TestClass1 { class TestClass2 { class TestClass3 { } } class TestClass4 {} }");
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.FullyQualifiedName()), Is.EquivalentTo(new[] { "TestClass1",
			                                                                                                           "TestClass1.TestClass2",
			                                                                                                           "TestClass1.TestClass2.TestClass3",
			                                                                                                           "TestClass1.TestClass4",
			                                                                                                        }));
			Assert.That(CompiledTypes.All(c => c is JsClass));
		}

		[Test]
		public void PartialClassesAreOnlyReturnedOnce() {
			Compile(@"partial class TestClass { }", @"partial class TestClass { }");
			Assert.That(CompiledTypes, Has.Count.EqualTo(1));
			Assert.That(CompiledTypes[0].CSharpTypeDefinition.Name, Is.EqualTo("TestClass"));
			Assert.That(CompiledTypes.All(c => c is JsClass));
		}

		[Test]
		public void NamespacingWorks() {
			Compile(@"class Test1 {
			          }
			          namespace Nmspace1.Nmspace2 {
			              namespace Nmspace3 {
			                  class Test2 {}
			              }
			              class Test3 {}
			          }
			          namespace Nmspace4 {
			              class Test4 {}
			              namespace Nmspace5 {
			                  class Test5 {}
			              }
			              namespace Nmspace5 {
			                  class Test6 {}
			              }
			              class Test7 {}
			          }
			          class Test8 {}
			          namespace Nmspace1 {
			              namespace Nmspace2 {
			                  class Test9 {}
			              }
			          }
			          class Test10 {}
			        ");
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.FullyQualifiedName()), Is.EquivalentTo(new[] { "Test1",
			                                                                                                            "Nmspace1.Nmspace2.Nmspace3.Test2",
			                                                                                                            "Nmspace1.Nmspace2.Test3",
			                                                                                                            "Nmspace4.Test4",
			                                                                                                            "Nmspace4.Nmspace5.Test5",
			                                                                                                            "Nmspace4.Nmspace5.Test6",
			                                                                                                            "Nmspace4.Test7",
			                                                                                                            "Test8",
			                                                                                                            "Nmspace1.Nmspace2.Test9",
			                                                                                                            "Test10",
			                                                                                                          }));
			Assert.That(CompiledTypes.All(c => c is JsClass));
		}

		[Test]
		public void EnumsWork() {
			Compile(@"enum Test1 {}");
			Assert.That(CompiledTypes, Has.Count.EqualTo(1));
			Assert.That(CompiledTypes[0], Is.AssignableTo<JsEnum>());
			Assert.That(CompiledTypes[0].CSharpTypeDefinition.Name, Is.EqualTo("Test1"));
		}

		[Test]
		public void EnumNestedInGenericTypeWorks() {
			Compile("class Test1<T1> { enum Test2 {} }");
		}

		[Test]
		public void ClassesWithGenerateCodeSetToFalseAndTheirNestedClassesAreNotInTheOutput() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = type => TypeScriptSemantics.NormalType(type.Name, generateCode: type.Name != "C2") };
			Compile(new[] { "class C1 {} class C2 { class C3 {} }" }, metadataImporter: metadataImporter);
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "C1", "C3" }));

			metadataImporter = new MockMetadataImporter { GetTypeSemantics = type => type.Name != "C2" ? TypeScriptSemantics.NormalType(type.Name) : TypeScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C1 {} class C2 { class C3 {} }" }, metadataImporter: metadataImporter);
			Assert.That(CompiledTypes.Select(t => t.CSharpTypeDefinition.Name), Is.EquivalentTo(new[] { "C1", "C3" }));
		}

		[Test]
		public void EnumsWithGenerateCodeSetToFalseAreNotInTheOutput() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = type => TypeScriptSemantics.NormalType(type.Name, generateCode: type.Name != "C2") };
			Compile(new[] { "enum C1 {} enum C2 {}" }, metadataImporter);
			Assert.That(CompiledTypes, Has.Count.EqualTo(1));
			Assert.That(CompiledTypes[0].CSharpTypeDefinition.Name, Is.EqualTo("C1"));

			metadataImporter = new MockMetadataImporter { GetTypeSemantics = type => type.Name != "C2" ? TypeScriptSemantics.NormalType(type.Name) : TypeScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "enum C1 {} enum C2 {}" }, metadataImporter);
			Assert.That(CompiledTypes, Has.Count.EqualTo(1));
			Assert.That(CompiledTypes[0].CSharpTypeDefinition.Name, Is.EqualTo("C1"));
		}

		[Test]
		public void DelegatesAreNotImported() {
			Compile(new[] { "delegate void D(int i);" });
			Assert.That(CompiledTypes, Is.Empty);
		}

		[Test]
		public void ClassThatIsNotUsableFromScriptCannotBeUsedAsABaseClassForAUsableClass() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "B1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);
			Compile(new[] { "class B1 {} class D1 : B1 {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("inheritance list") && er.AllMessages[0].FormattedMessage.Contains("B1") && er.AllMessages[0].FormattedMessage.Contains("D1"));

			metadataImporter = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "B1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			er = new MockErrorReporter(false);
			Compile(new[] { "class B1<T> {} class D1 : B1<int> {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("inheritance list") && er.AllMessages[0].FormattedMessage.Contains("B1") && er.AllMessages[0].FormattedMessage.Contains("D1"));
		}

		[Test]
		public void InterfaceThatIsNotUsableFromScriptCannotBeImplementedByAUsableClass() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "I1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);
			Compile(new[] { "interface I1 {} class C1 : I1 {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("inheritance list") && er.AllMessages[0].FormattedMessage.Contains("I1") && er.AllMessages[0].FormattedMessage.Contains("C1"));
		}

		[Test]
		public void ClassThatIsNotUsableFromScriptCannotBeUsedAsGenericArgumentForABaseClassOrImplementedInterface() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "C1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] { "class C1 {} class B1<T> {} class D1 : B1<C1> {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("inheritance list") && er.AllMessages[0].FormattedMessage.Contains("C1") && er.AllMessages[0].FormattedMessage.Contains("D1"));

			er = new MockErrorReporter(false);
			Compile(new[] { "class C1 {} interface I1<T> {} class D1 : I1<C1> {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("inheritance list") && er.AllMessages[0].FormattedMessage.Contains("C1") && er.AllMessages[0].FormattedMessage.Contains("D1"));

			er = new MockErrorReporter(false);
			Compile(new[] { "class C1 {} interface I1<T> {} class D1 : I1<I1<C1>> {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].FormattedMessage.Contains("not usable from script") && er.AllMessages[0].FormattedMessage.Contains("inheritance list") && er.AllMessages[0].FormattedMessage.Contains("C1") && er.AllMessages[0].FormattedMessage.Contains("D1"));
		}

		[Test]
		public void MutableValueTypeCannotBeUsedAsGenericArgumentForABaseClassOrImplementedInterface() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = t => t.Name == "C1" ? TypeScriptSemantics.MutableValueType(t.Name) : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] { "struct C1 {} class B1<T> {} class D1 : B1<C1> {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Code == 7539 && er.AllMessages[0].FormattedMessage.Contains("mutable value type") && er.AllMessages[0].FormattedMessage.Contains("C1"));

			er = new MockErrorReporter(false);
			Compile(new[] { "struct C1 {} interface I1<T> {} class D1 : I1<C1> {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Code == 7539 && er.AllMessages[0].FormattedMessage.Contains("mutable value type") && er.AllMessages[0].FormattedMessage.Contains("C1"));

			er = new MockErrorReporter(false);
			Compile(new[] { "struct C1 {} interface I1<T> {} class D1 : I1<I1<C1>> {}" }, metadataImporter: metadataImporter, errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages[0].Code == 7539 && er.AllMessages[0].FormattedMessage.Contains("mutable value type") && er.AllMessages[0].FormattedMessage.Contains("C1"));
		}

		[Test]
		public void UsingUnusableClassAsABaseClassForAnotherUnusableClassIsNotAnError() {
			var metadataImporter = new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class B {} class D : B {}" }, metadataImporter: metadataImporter);
			// No errors is good enough
		}
	}
}
