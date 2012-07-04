using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;
using FluentAssertions;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler {
    [TestFixture]
    public class TypeConversionTests : CompilerTestBase {
        [Test]
        public void EmptyGlobalClassIsCorrectlyReturned() {
            Compile(@"class TestClass { }");
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.Should().Be("TestClass");
            CompiledTypes[0].Should().BeAssignableTo<JsClass>();
        }

        [Test]
        public void NestedClassesWork() {
            Compile(@"class TestClass1 { class TestClass2 { class TestClass3 { } } class TestClass4 {} }");
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] {
                                                                              "TestClass1",
                                                                              "TestClass1$TestClass2",
                                                                              "TestClass1$TestClass2$TestClass3",
                                                                              "TestClass1$TestClass4",
                                                                            });
            CompiledTypes.Should().ContainItemsAssignableTo<JsClass>();
        }

        [Test]
        public void PartialClassesAreOnlyReturnedOnce() {
            Compile(@"partial class TestClass { }", @"partial class TestClass { }");
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.Should().Be("TestClass");
            CompiledTypes.Should().ContainItemsAssignableTo<JsClass>();
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
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] {
                                                                                "Test1",
                                                                                "Nmspace1.Nmspace2.Nmspace3.Test2",
                                                                                "Nmspace1.Nmspace2.Test3",
                                                                                "Nmspace4.Test4",
                                                                                "Nmspace4.Nmspace5.Test5",
                                                                                "Nmspace4.Nmspace5.Test6",
                                                                                "Nmspace4.Test7",
                                                                                "Test8",
                                                                                "Nmspace1.Nmspace2.Test9",
                                                                                "Test10",
                                                                            });
            CompiledTypes.Should().ContainItemsAssignableTo<JsClass>();
        }

        [Test]
        public void EnumsWork() {
            Compile(@"enum Test1 {}");
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Should().BeAssignableTo<JsEnum>();
            CompiledTypes[0].Name.Should().Be("Test1");
        }

        [Test]
        public void InheritanceWorks() {
            Compile(@"class BaseClass {}
                      class Test : BaseClass { }");
            Stringify(FindClass("Test").BaseClass).Should().Be("{BaseClass}");
        }

        [Test]
        public void InheritingNothingOrObjectShouldGiveObjectAsBaseType() {
            Compile(@"class Test1 : object {}
                      class Test2 {}");
            CompiledTypes.Should().HaveCount(2);
            Stringify(FindClass("Test1").BaseClass).Should().Be("{Object}");
            Stringify(FindClass("Test2").BaseClass).Should().Be("{Object}");
        }

        [Test]
        public void StructsShouldInheritValueObject() {
            Compile(@"struct Test {}");
            CompiledTypes.Should().HaveCount(1);
            Stringify(FindClass("Test").BaseClass).Should().Be("{ValueType}");
        }

        [Test]
        public void InterfacesShouldNotInheritAnything() {
            Compile(@"interface ITest {}");
            FindClass("ITest").BaseClass.Should().BeNull();
        }

        [Test]
        public void ClassCanImplementInterfaces() {
            Compile(@"interface ITest1 {}
                      interface ITest2 {}
                      class Test : ITest1, ITest2 { }");
            var cls = FindClass("Test");
            Stringify(cls.BaseClass).Should().Be("{Object}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void ClassCanImplementInterfacesAndInheritTypes() {
            Compile(@"interface ITest1 {}
                      interface ITest2 {}
                      class BaseClass {}
                      class Test : ITest1, BaseClass, ITest2 { }");
            var cls = FindClass("Test");
            Stringify(cls.BaseClass).Should().Be("{BaseClass}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void ClassCanInheritGenericClass() {
            Compile("class Base<T> {} class Test : Base<int> {}");
            Stringify(FindClass("Test").BaseClass).Should().Be("$InstantiateGenericType({Base}, {Int32})");
        }

        [Test]
        public void ClassCanImplementGenericInterface() {
            Compile("interface IMyInterface<T> {} class Test : IMyInterface<int> {}");
            Stringify(FindClass("Test").ImplementedInterfaces[0]).Should().Be("$InstantiateGenericType({IMyInterface}, {Int32})");
        }

        [Test]
        public void ClassCanImplementGenericInterfaceConstructedWithTypeParameter() {
            Compile("interface IMyInterface<T> {} class Test<T> : IMyInterface<T> {}");
            Stringify(FindClass("Test").ImplementedInterfaces[0]).Should().Be("$InstantiateGenericType({IMyInterface}, $T)");
        }

        [Test]
        public void ClassCanImplementGenericInterfaceConstructedWithSelf() {
            Compile("interface IMyInterface<T> {} class Test : IMyInterface<Test> {}");
            Stringify(FindClass("Test").ImplementedInterfaces[0]).Should().Be("$InstantiateGenericType({IMyInterface}, {Test})");
        }

        [Test]
        public void ClassCanUseOwnTypeParameterInBaseClass() {
            Compile("class Base<T> {} class Test<U> : Base<U> {}");
            Stringify(FindClass("Test").BaseClass).Should().Be("$InstantiateGenericType({Base}, $U)");
        }

        [Test]
        public void ClassCanUseSelfAsTypeParameterToBaseClass() {
            Compile("class Base<T> {} class Test : Base<Test> {}");
            Stringify(FindClass("Test").BaseClass).Should().Be("$InstantiateGenericType({Base}, {Test})");
        }

        [Test]
        public void GenericClassCanUseSelfAsTypeParameterToBaseClass() {
            Compile("class Base<T> {} class Test<T> : Base<Test<T>> {}");
            Stringify(FindClass("Test").BaseClass).Should().Be("$InstantiateGenericType({Base}, $InstantiateGenericType({Test}, $T))");
        }

        [Test]
        public void InterfaceCanImplementInterfaces() {
            Compile(@"interface ITest1 {}
                      interface ITest2 {}
                      interface ITest : ITest1, ITest2 { }");
            var cls = FindClass("ITest");
            cls.BaseClass.Should().BeNull();
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void StructsCanImplementInterfaces() {
            Compile(@"interface ITest1 {}
                      interface ITest2 {}
                      struct Test : ITest1, ITest2 { }");
            var cls = FindClass("Test");
            Stringify(cls.BaseClass).Should().Be("{ValueType}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void ClassTypeIsSetCorrectly() {
            Compile("class Test1{} struct Test2{} interface Test3{}");
            CompiledTypes.Should().HaveCount(3);
            Assert.That(((JsClass)CompiledTypes.Single(tp => tp.Name == "Test1")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Class));
            Assert.That(((JsClass)CompiledTypes.Single(tp => tp.Name == "Test2")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Struct));
            Assert.That(((JsClass)CompiledTypes.Single(tp => tp.Name == "Test3")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Interface));
        }

        [Test]
        public void PartialClassesInheritanceAndImplementationIsCorrect1() {
            Compile(
@"interface IEnumerable {}
interface ICollection : IEnumerable {}
interface IList : ICollection {}
interface ICloneable {}
interface ISerializable {}
class ArrayList : IList, ICloneable {}",
@"partial class TestClass : ArrayList, ISerializable { }",
@"partial class TestClass : IEnumarator { }");
            var cls = FindClass("TestClass");
            Stringify(cls.BaseClass).Should().Be("{ArrayList}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{IList}",
                                                                                        "{ICollection}",
                                                                                        "{IEnumerable}",
                                                                                        "{ICloneable}",
                                                                                        "{ISerializable}" });
        }

        [Test]
        public void PartialClassesInheritanceAndImplementationIsCorrect2() {
            Compile(
@"interface IEnumerable {}
interface IEnumerable<T> : IEnumerable {}
interface ICollection : IEnumerable {}
interface ICollection<T> : ICollection, IEnumerable<T> {}
interface IList : ICollection {}
interface IList<T> : IList, ICollection<T> {}
interface ICloneable {}
interface ISerializable {}
class ArrayList : IList, ICloneable {}",
@"partial class TestClass : ArrayList, System.Collections.IEnumerable { }",
@"partial class TestClass : IList<string> { }");
            var cls = FindClass("TestClass");
            Stringify(cls.BaseClass).Should().Be("{ArrayList}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{IList}",
                                                                                        "{ICollection}",
                                                                                        "{IEnumerable}",
                                                                                        "{ICloneable}",
                                                                                        "$InstantiateGenericType({IList}, {String})",
                                                                                        "$InstantiateGenericType({ICollection}, {String})",
                                                                                        "$InstantiateGenericType({IEnumerable}, {String})" });
        }

        [Test]
        public void NonGenericInheritedDefinedInterfacesAreRecordedInTheInterfaceList() {
            Compile(@"interface ITest1 {}
                      interface ITest2 {}
                      interface ITest3 : ITest1, ITest2 {}
                      class Test : ITest3 {}
                      interface ITest : ITest3 {}");
            FindClass("Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}", "{ITest3}" });
            FindClass("ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}", "{ITest3}" });
        }

        [Test]
        public void GenericInheritedDefinedInterfacesAreRecordedInTheInterfaceList() {
            Compile(@"interface ITest1<T> {}
                      interface ITest2<T> {}
                      interface ITest3<T1, T2> : ITest1<T1>, ITest2<T2> {}
                      class Test : ITest3<int, string> {}
                      interface ITest<T> : ITest3<T, T> {}");
            FindClass("Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "$InstantiateGenericType({ITest1}, {Int32})", "$InstantiateGenericType({ITest2}, {String})", "$InstantiateGenericType({ITest3}, {Int32}, {String})" });
            FindClass("ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "$InstantiateGenericType({ITest1}, $T)", "$InstantiateGenericType({ITest2}, $T)", "$InstantiateGenericType({ITest3}, $T, $T)" });
        }

        [Test]
        public void InheritedInterfacesAreRecordedInTheInterfaceListForClasses() {
            Compile(
@"class KeyValuePair<T1, T2> {}
interface IEnumerable {}
interface IEnumerable<T> : IEnumerable {}
interface ICollection {}
interface ICollection<T> : ICollection, IEnumerable<T> {}
interface IDictionary {}
interface IDictionary<T1, T2> : IDictionary, ICollection<KeyValuePair<T1, T2>> {}
interface IDeserializationCallback {}
interface ISerializable {}
class Dictionary<T1, T2> : IDictionary<T1, T2>, IDeserializationCallback, ISerializable {}
			class Test<T> : Dictionary<T, int> {}");
            FindClass("Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "$InstantiateGenericType({IDictionary}, $T, {Int32})",
                                                                                                      "$InstantiateGenericType({ICollection}, $InstantiateGenericType({KeyValuePair}, $T, {Int32}))",
                                                                                                      "$InstantiateGenericType({IEnumerable}, $InstantiateGenericType({KeyValuePair}, $T, {Int32}))",
                                                                                                      "{IDictionary}",
                                                                                                      "{ICollection}",
                                                                                                      "{IEnumerable}",
                                                                                                      "{ISerializable}",
                                                                                                      "{IDeserializationCallback}" });
        }

        [Test]
        public void IndirectlyImplementedInterfacesAreRecordedInTheInterfaceListForClasses() {
            Compile(@"
class KeyValuePair<T1, T2> {}
interface IEnumerable {}
interface IEnumerable<T> : IEnumerable {}
interface ICollection<T> : IEnumerable<T> {}
interface IDictionary<T1, T2> : ICollection<KeyValuePair<T1, T2>> {}
class Test<T> : IDictionary<T, int> {}");
            FindClass("Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "$InstantiateGenericType({IDictionary}, $T, {Int32})",
                                                                                                      "$InstantiateGenericType({ICollection}, $InstantiateGenericType({KeyValuePair}, $T, {Int32}))",
                                                                                                      "$InstantiateGenericType({IEnumerable}, $InstantiateGenericType({KeyValuePair}, $T, {Int32}))",
                                                                                                      "{IEnumerable}" });
        }

        [Test]
        public void IndirectlyImplementedInterfacesAreRecordedInTheInterfaceListForInterfaces() {
            Compile(@"
class KeyValuePair<T1, T2> {}
interface IEnumerable {}
interface IEnumerable<T> : IEnumerable {}
interface ICollection<T> : IEnumerable<T> {}
interface IDictionary<T1, T2> : ICollection<KeyValuePair<T1, T2>> {}
interface ITest : IDictionary<string, int> {}");
            FindClass("ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "$InstantiateGenericType({IDictionary}, {String}, {Int32})",
                                                                                                       "$InstantiateGenericType({ICollection}, $InstantiateGenericType({KeyValuePair}, {String}, {Int32}))",
                                                                                                       "$InstantiateGenericType({IEnumerable}, $InstantiateGenericType({KeyValuePair}, {String}, {Int32}))",
                                                                                                       "{IEnumerable}" });
        }

        [Test]
        public void InheritingNestedGenericTypesWorks() {
            Compile(@"
class KeyValuePair<T1, T2> {}
interface IEnumerable {}
interface IEnumerable<T> : IEnumerable {}
interface ICollection<T> : IEnumerable<T>, ICollection {}
interface ICollection {}
interface IList {}
interface IList<T> : IList, ICollection<T> {}
interface IDictionary<T1, T2> : ICollection<KeyValuePair<T1, T2>> {}
class Dictionary<T1, T2> : IDictionary<T1, T2> {}
class List<T> : IList<T> {}
class Test<T1, T2> : List<Dictionary<T1, T2>> {}");

            var cls = FindClass("Test");
            Stringify(cls.BaseClass).Should().Be("$InstantiateGenericType({List}, $InstantiateGenericType({Dictionary}, $T1, $T2))");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "$InstantiateGenericType({IList}, $InstantiateGenericType({Dictionary}, $T1, $T2))",
                                                                                        "$InstantiateGenericType({ICollection}, $InstantiateGenericType({Dictionary}, $T1, $T2))",
                                                                                        "$InstantiateGenericType({IEnumerable}, $InstantiateGenericType({Dictionary}, $T1, $T2))",
                                                                                        "{IList}",
                                                                                        "{ICollection}",
                                                                                        "{IEnumerable}" });
            
        }

        [Test]
        public void NestedTypesInheritTheGenericParametersOfTheirParents() {
            Compile("class Test1<T1> { class Test2<T2, T3> { class Test3<T4> {} } }");
            var test1 = FindClass("Test1");
            var test2 = FindClass("Test1$Test2");
            var test3 = FindClass("Test1$Test2$Test3");
            test1.TypeArgumentNames.Should().Equal(new[] { "$T1" });
            test2.TypeArgumentNames.Should().Equal(new[] { "$T1", "$T2", "$T3" });
            test3.TypeArgumentNames.Should().Equal(new[] { "$T1", "$T2", "$T3", "$T4" });
        }

        [Test]
        public void NonGenericTypeNestedInGenericTypeWorks() {
            Compile("class Test1<T1> { class Test2 {} }");
            var test1 = FindClass("Test1");
            var test2 = FindClass("Test1$Test2");
            test1.TypeArgumentNames.Should().Equal(new[] { "$T1" });
            test2.Should().NotBeNull();
        }

        [Test]
        public void EnumNestedInGenericTypeWorks() {
            Compile("class Test1<T1> { enum Test2 {} }");
        }

        [Test]
        public void ClassesWithGenerateCodeSetToFalseAndTheirNestedClassesAreNotInTheOutput() {
            var namingConvention = new MockNamingConventionResolver { GetTypeSemantics = type => TypeScriptSemantics.NormalType(type.Name, generateCode: type.Name != "C2") };
            Compile(new[] { "class C1 {} class C2 { class C3 {} }" }, namingConvention: namingConvention);
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "C1", "C3" });

            namingConvention = new MockNamingConventionResolver { GetTypeSemantics = type => type.Name != "C2" ? TypeScriptSemantics.NormalType(type.Name) : TypeScriptSemantics.NotUsableFromScript() };
            Compile(new[] { "class C1 {} class C2 { class C3 {} }" }, namingConvention: namingConvention);
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] { "C1", "C3" });
        }

        [Test]
        public void EnumsWithGenerateCodeSetToFalseAreNotInTheOutput() {
            var namingConvention = new MockNamingConventionResolver { GetTypeSemantics = type => TypeScriptSemantics.NormalType(type.Name, generateCode: type.Name != "C2") };
            Compile(new[] { "enum C1 {} enum C2 {}" }, namingConvention);
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.Should().Be("C1");

            namingConvention = new MockNamingConventionResolver { GetTypeSemantics = type => type.Name != "C2" ? TypeScriptSemantics.NormalType(type.Name) : TypeScriptSemantics.NotUsableFromScript() };
            Compile(new[] { "enum C1 {} enum C2 {}" }, namingConvention);
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.Should().Be("C1");
        }

        [Test]
        public void DelegatesAreNotImported() {
            Compile(new[] { "delegate void D(int i);" });
            CompiledTypes.Should().BeEmpty();
        }

		[Test]
		public void ClassThatIsNotUsableFromScriptCannotBeUsedAsABaseClassForAUsableClass() {
			var nc = new MockNamingConventionResolver { GetTypeSemantics = t => t.Name == "B1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);
			Compile(new[] { "class B1 {} class D1 : B1 {}" }, namingConvention: nc, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("inheritance list") && er.AllMessagesText[0].Contains("B1") && er.AllMessagesText[0].Contains("D1"));

			nc = new MockNamingConventionResolver { GetTypeSemantics = t => t.Name == "B1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			er = new MockErrorReporter(false);
			Compile(new[] { "class B1<T> {} class D1 : B1<int> {}" }, namingConvention: nc, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("inheritance list") && er.AllMessagesText[0].Contains("B1") && er.AllMessagesText[0].Contains("D1"));
		}

		[Test]
		public void InterfaceThatIsNotUsableFromScriptCannotBeImplementedByAUsableClass() {
			var nc = new MockNamingConventionResolver { GetTypeSemantics = t => t.Name == "I1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);
			Compile(new[] { "interface I1 {} class C1 : I1 {}" }, namingConvention: nc, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("inheritance list") && er.AllMessagesText[0].Contains("I1") && er.AllMessagesText[0].Contains("C1"));
		}

		[Test]
		public void ClassThatIsNotUsableFromScriptCannotBeUsedAsGenericArgumentForABaseClassOrImplementedInterface() {
			var nc = new MockNamingConventionResolver { GetTypeSemantics = t => t.Name == "C1" ? TypeScriptSemantics.NotUsableFromScript() : TypeScriptSemantics.NormalType(t.Name) };
			var er = new MockErrorReporter(false);

			Compile(new[] { "class C1 {} class B1<T> {} class D1 : B1<C1> {}" }, namingConvention: nc, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("inheritance list") && er.AllMessagesText[0].Contains("C1") && er.AllMessagesText[0].Contains("D1"));

			er = new MockErrorReporter(false);
			Compile(new[] { "class C1 {} interface I1<T> {} class D1 : I1<C1> {}" }, namingConvention: nc, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("inheritance list") && er.AllMessagesText[0].Contains("C1") && er.AllMessagesText[0].Contains("D1"));

			er = new MockErrorReporter(false);
			Compile(new[] { "class C1 {} interface I1<T> {} class D1 : I1<I1<C1>> {}" }, namingConvention: nc, errorReporter: er);
			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText[0].Contains("not usable from script") && er.AllMessagesText[0].Contains("inheritance list") && er.AllMessagesText[0].Contains("C1") && er.AllMessagesText[0].Contains("D1"));
		}


		[Test]
		public void UsingUnusableClassAsABaseClassForAnotherUnusableClassIsNotAnError() {
			var nc = new MockNamingConventionResolver { GetTypeSemantics = t => TypeScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class B {} class D : B {}" }, namingConvention: nc);
			// No errors is good enough
		}

		[Test]
		public void IgnoreGenericArgumentsInTheTypeSemanticsRemovesGenericArgumentsFromTheType() {
			var nc = new MockNamingConventionResolver { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name, ignoreGenericArguments: true) };
            Compile(new[] { "class C1<T1, T2> {}" }, namingConvention: nc);
            FindClass("C1").TypeArgumentNames.Should().BeEmpty();
		}

		[Test]
		public void VirtualInterfaceDoesNotAppearInTheImplementedInterfaces() {
			var nc = new MockNamingConventionResolver { GetTypeSemantics = t => t.Name == "I1" || t.Name == "I3" ? TypeScriptSemantics.VirtualInterface() : TypeScriptSemantics.NormalType(t.Name) };
            Compile(new[] { "interface I1 {} interface I2 {} interface I3<T> {} class C1 : I1, I2, I3<int> {}" }, namingConvention: nc);
            FindClass("C1").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new object[] { "{I2}" });
		}
    }
}
