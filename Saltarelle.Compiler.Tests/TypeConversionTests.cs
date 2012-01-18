using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel;
using FluentAssertions;

namespace Saltarelle.Compiler.Tests {
    [TestFixture]
    public class TypeConversionTests : CompilerTestBase {
        [Test]
        public void EmptyGlobalClassIsCorrectlyReturned() {
            Compile(@"class TestClass { }");
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.Should().Be(ScopedName.Global(null, "TestClass"));
            CompiledTypes[0].Should().BeAssignableTo<JsClass>();
        }

        [Test]
        public void NestedClassesWork() {
            Compile(@"class TestClass1 { class TestClass2 { class TestClass3 { } } class TestClass4 {} }");
            CompiledTypes.Select(t => t.Name).Should().BeEquivalentTo(new[] {
                                                                              ScopedName.Global(null, "TestClass1"),
                                                                              ScopedName.Nested(ScopedName.Global(null, "TestClass1"), "TestClass2"),
                                                                              ScopedName.Nested(ScopedName.Nested(ScopedName.Global(null, "TestClass1"), "TestClass2"), "TestClass3"),
                                                                              ScopedName.Nested(ScopedName.Global(null, "TestClass1"), "TestClass4"),
                                                                            });
            CompiledTypes.Should().ContainItemsAssignableTo<JsClass>();
        }

        [Test]
        public void PartialClassesAreOnlyReturnedOnce() {
            Compile(@"partial class TestClass { }", @"partial class TestClass { }");
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.Should().Be(ScopedName.Global(null, "TestClass"));
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
                                                                                ScopedName.Global(null, "Test1"),
                                                                                ScopedName.Global("Nmspace1.Nmspace2.Nmspace3", "Test2"),
                                                                                ScopedName.Global("Nmspace1.Nmspace2", "Test3"),
                                                                                ScopedName.Global("Nmspace4", "Test4"),
                                                                                ScopedName.Global("Nmspace4.Nmspace5", "Test5"),
                                                                                ScopedName.Global("Nmspace4.Nmspace5", "Test6"),
                                                                                ScopedName.Global("Nmspace4", "Test7"),
                                                                                ScopedName.Global(null, "Test8"),
                                                                                ScopedName.Global("Nmspace1.Nmspace2", "Test9"),
                                                                                ScopedName.Global(null, "Test10"),
                                                                            });
            CompiledTypes.Should().ContainItemsAssignableTo<JsClass>();
        }

        [Test]
        public void EnumsWork() {
            Compile(@"enum Test1 {}");
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Should().BeAssignableTo<JsEnum>();
            CompiledTypes[0].Name.Should().Be(ScopedName.Global(null, "Test1"));
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
            Stringify(FindClass("Test1").BaseClass).Should().Be("{System.Object}");
            Stringify(FindClass("Test2").BaseClass).Should().Be("{System.Object}");
        }

        [Test]
        public void StructsShouldInheritValueObject() {
            Compile(@"struct Test {}");
            CompiledTypes.Should().HaveCount(1);
            Stringify(FindClass("Test").BaseClass).Should().Be("{System.ValueType}");
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
            Stringify(cls.BaseClass).Should().Be("{System.Object}");
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
            Stringify(FindClass("Test").BaseClass).Should().Be("{Base`1}<{System.Int32}>");
        }

        [Test]
        public void ClassCanImplementGenericInterface() {
            Compile("class Test : System.Collections.Generic.IEqualityComparer<int> {}");
            Stringify(FindClass("Test").ImplementedInterfaces[0]).Should().Be("{System.Collections.Generic.IEqualityComparer`1}<{System.Int32}>");
        }

        [Test]
        public void ClassCanImplementGenericInterfaceConstructedWithTypeParameter() {
            Compile("class Test<T> : System.Collections.Generic.IEqualityComparer<T> {}");
            Stringify(FindClass("Test").ImplementedInterfaces[0]).Should().Be("{System.Collections.Generic.IEqualityComparer`1}<T>");
        }

        [Test]
        public void ClassCanImplementGenericInterfaceConstructedWithSelf() {
            Compile("class Test : System.Collections.Generic.IEqualityComparer<Test> {}");
            Stringify(FindClass("Test").ImplementedInterfaces[0]).Should().Be("{System.Collections.Generic.IEqualityComparer`1}<{Test}>");
        }

        [Test]
        public void ClassCanUseOwnTypeParameterInBaseClass() {
            Compile("class Base<T> {} class Test<U> : Base<U> {}");
            Stringify(FindClass("Test").BaseClass).Should().Be("{Base`1}<U>");
        }

        [Test]
        public void ClassCanUseSelfAsTypeParameterToBaseClass() {
            Compile("class Base<T> {} class Test : Base<Test> {}");
            Stringify(FindClass("Test").BaseClass).Should().Be("{Base`1}<{Test}>");
        }

        [Test]
        public void GenericClassCanUseSelfAsTypeParameterToBaseClass() {
            Compile("class Base<T> {} class Test<T> : Base<Test<T>> {}");
            Stringify(FindClass("Test").BaseClass).Should().Be("{Base`1}<{Test`1}<T>>");
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
            Stringify(cls.BaseClass).Should().Be("{System.ValueType}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void ClassCanInheritInnerClassAndImplementInnerInterface() {
            Compile(@"class Test : Test.Base, Test.ITest { public class Base {} interface ITest {} }");
            var cls = FindClass("Test");
            Stringify(cls.BaseClass).Should().Be("{Test+Base}");
        }

        [Test]
        public void NamingConventionIsCorrectlyApplied() {
            var namingConvention = new MockNamingConventionResolver { GetTypeName = def => "$" + def.Name, GetTypeParameterName = def => "$$" + def.Name };
            Compile(new[] { @"using System.Collections.Generic;
                              class Test<T> : List<T>, IEnumerable<string>, IList<float> { }" }, namingConvention: namingConvention);
            var cls = FindClass("$Test");
            cls.TypeArgumentNames.Should().Equal(new[] { "$$T" });
            Stringify(cls.BaseClass).Should().Be("{System.Collections.Generic.List`1}<$$T>");
        }

        [Test]
        public void ClassTypeIsSetCorrectly() {
            Compile("class Test1{} struct Test2{} interface Test3{}");
            CompiledTypes.Should().HaveCount(3);
            Assert.That(((JsClass)CompiledTypes.Single(tp => tp.Name.UnqualifiedName == "Test1")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Class));
            Assert.That(((JsClass)CompiledTypes.Single(tp => tp.Name.UnqualifiedName == "Test2")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Struct));
            Assert.That(((JsClass)CompiledTypes.Single(tp => tp.Name.UnqualifiedName == "Test3")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Interface));
        }

        [Test]
        public void PartialClassesInheritanceAndImplementationIsCorrect1() {
            Compile(@"partial class TestClass : System.Collections.ArrayList, System.Runtime.Serialization.ISerializable { }", @"partial class TestClass : System.Collections.IEnumarator { }");
            CompiledTypes.Should().HaveCount(1);
            var cls = FindClass("TestClass");
            Stringify(cls.BaseClass).Should().Be("{System.Collections.ArrayList}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.IList}",
                                                                                        "{System.Collections.ICollection}",
                                                                                        "{System.Collections.IEnumerable}",
                                                                                        "{System.ICloneable}",
                                                                                        "{System.Runtime.Serialization.ISerializable}" });
        }

        [Test]
        public void PartialClassesInheritanceAndImplementationIsCorrect2() {
            Compile(@"partial class TestClass : System.Collections.ArrayList, System.Collections.IEnumerable { }", @"partial class TestClass : System.Collections.List, System.Collections.Generic.IList<string> { }");
            CompiledTypes.Should().HaveCount(1);
            var cls = FindClass("TestClass");
            Stringify(cls.BaseClass).Should().Be("{System.Collections.ArrayList}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.IList}",
                                                                                        "{System.Collections.ICollection}",
                                                                                        "{System.Collections.IEnumerable}",
                                                                                        "{System.ICloneable}",
                                                                                        "{System.Collections.Generic.IList`1}<{System.String}>",
                                                                                        "{System.Collections.Generic.ICollection`1}<{System.String}>",
                                                                                        "{System.Collections.Generic.IEnumerable`1}<{System.String}>" });
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
            FindClass("Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1`1}<{System.Int32}>", "{ITest2`1}<{System.String}>", "{ITest3`2}<{System.Int32},{System.String}>" });
            FindClass("ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1`1}<T>", "{ITest2`1}<T>", "{ITest3`2}<T,T>" });
        }

        [Test]
        public void InheritedImportedInterfacesAreRecordedInTheInterfaceListForClasses() {
            Compile("class Test<T> : System.Collections.Generic.Dictionary<T, int> {}");
            FindClass("Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.Generic.IDictionary`2}<T,{System.Int32}>",
                                                                                                      "{System.Collections.Generic.ICollection`1}<{System.Collections.Generic.KeyValuePair`2}<T,{System.Int32}>>",
                                                                                                      "{System.Collections.Generic.IEnumerable`1}<{System.Collections.Generic.KeyValuePair`2}<T,{System.Int32}>>",
                                                                                                      "{System.Collections.IDictionary}",
                                                                                                      "{System.Collections.ICollection}",
                                                                                                      "{System.Collections.IEnumerable}",
                                                                                                      "{System.Runtime.Serialization.ISerializable}",
                                                                                                      "{System.Runtime.Serialization.IDeserializationCallback}" });
        }

        [Test]
        public void IndirectlyImplementedInterfacesAreRecordedInTheInterfaceListForClasses() {
            Compile("class Test<T> : System.Collections.Generic.IDictionary<T, int> {}");
            FindClass("Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.Generic.IDictionary`2}<T,{System.Int32}>",
                                                                                                      "{System.Collections.Generic.ICollection`1}<{System.Collections.Generic.KeyValuePair`2}<T,{System.Int32}>>",
                                                                                                      "{System.Collections.Generic.IEnumerable`1}<{System.Collections.Generic.KeyValuePair`2}<T,{System.Int32}>>",
                                                                                                      "{System.Collections.IEnumerable}" });
        }

        [Test]
        public void IndirectlyImplementedInterfacesAreRecordedInTheInterfaceListForInterfaces() {
            Compile("interface ITest : System.Collections.Generic.IDictionary<string, int> {}");
            FindClass("ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.Generic.IDictionary`2}<{System.String},{System.Int32}>",
                                                                                                              "{System.Collections.Generic.ICollection`1}<{System.Collections.Generic.KeyValuePair`2}<{System.String},{System.Int32}>>",
                                                                                                              "{System.Collections.Generic.IEnumerable`1}<{System.Collections.Generic.KeyValuePair`2}<{System.String},{System.Int32}>>",
                                                                                                              "{System.Collections.IEnumerable}" });
        }

        [Test]
        public void InheritingNestedGenericTypesWorks() {
            Compile("using System.Collections.Generic; class Test<T1, T2> : List<Dictionary<T1, T2>> {}");
            var cls = FindClass("Test");
            Stringify(cls.BaseClass).Should().Be("{System.Collections.Generic.List`1}<{System.Collections.Generic.Dictionary`2}<T1,T2>>");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.Generic.IList`1}<{System.Collections.Generic.Dictionary`2}<T1,T2>>",
                                                                                        "{System.Collections.Generic.ICollection`1}<{System.Collections.Generic.Dictionary`2}<T1,T2>>",
                                                                                        "{System.Collections.Generic.IEnumerable`1}<{System.Collections.Generic.Dictionary`2}<T1,T2>>",
                                                                                        "{System.Collections.IList}",
                                                                                        "{System.Collections.ICollection}",
                                                                                        "{System.Collections.IEnumerable}" });
            
        }

        [Test]
        public void NestedTypesInheritTheGenericParametersOfTheirParents() {
            Compile("class Test1<T1> { class Test2<T2, T3> { class Test3<T4> {} } }");
            var test1 = FindClass("Test1");
            var test2 = FindClass("Test1+Test2");
            var test3 = FindClass("Test1+Test2+Test3");
            test1.TypeArgumentNames.Should().Equal(new[] { "T1" });
            test2.TypeArgumentNames.Should().Equal(new[] { "T1", "T2", "T3" });
            test3.TypeArgumentNames.Should().Equal(new[] { "T1", "T2", "T3", "T4" });
        }

        [Test]
        public void IsPublicFlagIsCorrectlySetForClasses() {
            Compile(@"class C1 {}
                      internal class C2 {}
                      public class C3 {}
                      public class C4 { internal class C5 { public class C6 {} } }
                      internal class C7 { public class C8 { public class C9 {} } }
                      public class C10 { private class C11 {} protected class C12 {} protected internal class C13 {} }
                     ");
                                 
            FindClass("C1").IsPublic.Should().BeFalse();
            FindClass("C2").IsPublic.Should().BeFalse();
            FindClass("C3").IsPublic.Should().BeTrue();
            FindClass("C4").IsPublic.Should().BeTrue();
            FindClass("C4+C5").IsPublic.Should().BeFalse();
            FindClass("C4+C5+C6").IsPublic.Should().BeFalse();
            FindClass("C7").IsPublic.Should().BeFalse();
            FindClass("C7+C8").IsPublic.Should().BeFalse();
            FindClass("C7+C8+C9").IsPublic.Should().BeFalse();
            FindClass("C10").IsPublic.Should().BeTrue();
            FindClass("C10+C11").IsPublic.Should().BeFalse();
            FindClass("C10+C12").IsPublic.Should().BeTrue();
            FindClass("C10+C13").IsPublic.Should().BeTrue();
        }

        [Test]
        public void IsPublicFlagIsCorrectlySetForEnums() {
            Compile(@"enum C1 {}
                      internal enum C2 {}
                      public enum C3 {}
                      public class C4 { internal class C5 { public enum C6 {} } }
                      internal class C7 { public class C8 { public enum C9 {} } }
                      public class C10 { private enum C11 {} protected enum C12 {} protected internal enum C13 {} }
                     ");
                                 
            CompiledTypes.Single(tp => tp.Name.ToString() == "C1").IsPublic.Should().BeFalse();
            CompiledTypes.Single(tp => tp.Name.ToString() == "C2").IsPublic.Should().BeFalse();
            CompiledTypes.Single(tp => tp.Name.ToString() == "C3").IsPublic.Should().BeTrue();
            CompiledTypes.Single(tp => tp.Name.ToString() == "C4+C5+C6").IsPublic.Should().BeFalse();
            CompiledTypes.Single(tp => tp.Name.ToString() == "C7+C8+C9").IsPublic.Should().BeFalse();
            CompiledTypes.Single(tp => tp.Name.ToString() == "C10+C11").IsPublic.Should().BeFalse();
            CompiledTypes.Single(tp => tp.Name.ToString() == "C10+C12").IsPublic.Should().BeTrue();
            CompiledTypes.Single(tp => tp.Name.ToString() == "C10+C13").IsPublic.Should().BeTrue();
        }

        [Test]
        public void ClassesForWhichTheNamingConventionReturnsNulllAreNotInTheOutput() {
            var namingConvention = new MockNamingConventionResolver { GetTypeName = type => type.Name == "C2" ? null : type.Name };
            Compile(new[] { "class C1 {} class C2 { class C3 {} }" }, namingConvention: namingConvention);
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.ToString().Should().Be("C1");
        }

        [Test]
        public void EnumsForWhichTheNamingConventionReturnsNulllAreNotInTheOutput() {
            var namingConvention = new MockNamingConventionResolver { GetTypeName = type => type.Name == "C2" ? null : type.Name };
            Compile(new[] { "enum C1 {} enum C2 {}" }, namingConvention);
            CompiledTypes.Should().HaveCount(1);
            CompiledTypes[0].Name.ToString().Should().Be("C1");
        }

        [Test]
        public void DelegatesAreNotImported() {
            Compile(new[] { "delegate void D(int i);" });
            CompiledTypes.Should().BeEmpty();
        }
    }
}
