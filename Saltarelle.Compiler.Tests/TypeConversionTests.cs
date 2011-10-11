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
            var types = Compile(@"class TestClass { }");
            types.Should().HaveCount(1);
            types[0].Name.Should().Be(ScopedName.Global(null, "TestClass"));
            types[0].Should().BeAssignableTo<JsClass>();
        }

        [Test]
        public void NestedClassesWork() {
            var types = Compile(@"class TestClass1 { class TestClass2 { class TestClass3 { } } class TestClass4 {} }");
            types.Select(t => t.Name).Should().BeEquivalentTo(new[] {
                                                                        ScopedName.Global(null, "TestClass1"),
                                                                        ScopedName.Nested(ScopedName.Global(null, "TestClass1"), "TestClass2"),
                                                                        ScopedName.Nested(ScopedName.Nested(ScopedName.Global(null, "TestClass1"), "TestClass2"), "TestClass3"),
                                                                        ScopedName.Nested(ScopedName.Global(null, "TestClass1"), "TestClass4"),
                                                                    });
            types.Should().ContainItemsAssignableTo<JsClass>();
        }

        [Test]
        public void PartialClassesAreOnlyReturnedOnce() {
            var types = Compile(@"partial class TestClass { }", @"partial class TestClass { }");
            types.Should().HaveCount(1);
            types[0].Name.Should().Be(ScopedName.Global(null, "TestClass"));
            types.Should().ContainItemsAssignableTo<JsClass>();
        }

        [Test]
        public void NamespacingWorks() {
            var types = Compile(@"class Test1 {
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
            types.Select(t => t.Name).Should().BeEquivalentTo(new[] {
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
            types.Should().ContainItemsAssignableTo<JsClass>();
        }

        [Test]
        public void EnumsWork() {
            var types = Compile(@"enum Test1 {}");
            types.Should().HaveCount(1);
            types[0].Should().BeAssignableTo<JsEnum>();
            types[0].Name.Should().Be(ScopedName.Global(null, "Test1"));
        }

        [Test]
        public void InheritanceWorks() {
            var types = Compile(@"class BaseClass {}
                                  class Test : BaseClass { }");
            Stringify(FindClass(types, "Test").BaseClass).Should().Be("{BaseClass}");
        }

        [Test]
        public void InheritingNothingOrObjectShouldGiveObjectAsBaseType() {
            var types = Compile(@"class Test1 : object {}
                                  class Test2 {}");
            types.Should().HaveCount(2);
            Stringify(FindClass(types, "Test1").BaseClass).Should().Be("{System.Object}");
            Stringify(FindClass(types, "Test2").BaseClass).Should().Be("{System.Object}");
        }

        [Test]
        public void StructsShouldInheritValueObject() {
            var types = Compile(@"struct Test {}");
            types.Should().HaveCount(1);
            Stringify(FindClass(types, "Test").BaseClass).Should().Be("{System.ValueType}");
        }

        [Test]
        public void InterfacesShouldNotInheritAnything() {
            var types = Compile(@"interface ITest {}");
            FindClass(types, "ITest").BaseClass.Should().BeNull();
        }

        [Test]
        public void ClassCanImplementInterfaces() {
            var types = Compile(@"interface ITest1 {}
                                  interface ITest2 {}
                                  class Test : ITest1, ITest2 { }");
            var cls = FindClass(types, "Test");
            Stringify(cls.BaseClass).Should().Be("{System.Object}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void ClassCanImplementInterfacesAndInheritTypes() {
            var types = Compile(@"interface ITest1 {}
                                  interface ITest2 {}
                                  class BaseClass {}
                                  class Test : ITest1, BaseClass, ITest2 { }");
            var cls = FindClass(types, "Test");
            Stringify(cls.BaseClass).Should().Be("{BaseClass}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void ClassCanInheritGenericClass() {
            var types = Compile("class Base<T> {} class Test : Base<int> {}");
            Stringify(FindClass(types, "Test").BaseClass).Should().Be("{Base`1}<{System.Int32}>");
        }

        [Test]
        public void ClassCanImplementGenericInterface() {
            var types = Compile("class Test : System.Collections.Generic.IEqualityComparer<int> {}");
            Stringify(FindClass(types, "Test").ImplementedInterfaces[0]).Should().Be("{System.Collections.Generic.IEqualityComparer`1}<{System.Int32}>");
        }

        [Test]
        public void ClassCanImplementGenericInterfaceConstructedWithTypeParameter() {
            var types = Compile("class Test<T> : System.Collections.Generic.IEqualityComparer<T> {}");
            Stringify(FindClass(types, "Test").ImplementedInterfaces[0]).Should().Be("{System.Collections.Generic.IEqualityComparer`1}<T>");
        }

        [Test]
        public void ClassCanImplementGenericInterfaceConstructedWithSelf() {
            var types = Compile("class Test : System.Collections.Generic.IEqualityComparer<Test> {}");
            Stringify(FindClass(types, "Test").ImplementedInterfaces[0]).Should().Be("{System.Collections.Generic.IEqualityComparer`1}<{Test}>");
        }

        [Test]
        public void ClassCanUseOwnTypeParameterInBaseClass() {
            var types = Compile("class Base<T> {} class Test<U> : Base<U> {}");
            Stringify(FindClass(types, "Test").BaseClass).Should().Be("{Base`1}<U>");
        }

        [Test]
        public void ClassCanUseSelfAsTypeParameterToBaseClass() {
            var types = Compile("class Base<T> {} class Test : Base<Test> {}");
            Stringify(FindClass(types, "Test").BaseClass).Should().Be("{Base`1}<{Test}>");
        }

        [Test]
        public void GenericClassCanUseSelfAsTypeParameterToBaseClass() {
            var types = Compile("class Base<T> {} class Test<T> : Base<Test<T>> {}");
            Stringify(FindClass(types, "Test").BaseClass).Should().Be("{Base`1}<{Test`1}<T>>");
        }

        [Test]
        public void InterfaceCanImplementInterfaces() {
            var types = Compile(@"interface ITest1 {}
                                  interface ITest2 {}
                                  interface ITest : ITest1, ITest2 { }");
            var cls = FindClass(types, "ITest");
            cls.BaseClass.Should().BeNull();
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void StructsCanImplementInterfaces() {
            var types = Compile(@"interface ITest1 {}
                                  interface ITest2 {}
                                  struct Test : ITest1, ITest2 { }");
            var cls = FindClass(types, "Test");
            Stringify(cls.BaseClass).Should().Be("{System.ValueType}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}" });
        }

        [Test]
        public void ClassCanInheritInnerClassAndImplementInnerInterface() {
            var types = Compile(@"class Test : Test.Base, Test.ITest { public class Base {} interface ITest {} }");
            var cls = FindClass(types, "Test");
            Stringify(cls.BaseClass).Should().Be("{Test+Base}");
        }

        [Test]
        public void NamingConventionIsCorrectlyApplied() {
            var namingConvention = new Mock<INamingConventionResolver>();
            namingConvention.Setup(_ => _.GetTypeName(It.IsAny<ITypeDefinition>())).Returns((ITypeDefinition def) => "$" + def.Name);
            namingConvention.Setup(_ => _.GetTypeParameterName(It.IsAny<ITypeParameter>())).Returns((ITypeParameter p) => "$$" + p.Name);
            var types = Compile(new[] { @"using System.Collections.Generic;
                                          class Test<T> : List<T>, IEnumerable<string>, IList<float> { }" }, namingConvention: namingConvention.Object);
            var cls = FindClass(types, "$Test");
            cls.TypeArgumentNames.Should().Equal(new[] { "$$T" });
            Stringify(cls.BaseClass).Should().Be("{System.Collections.Generic.List`1}<$$T>");
        }

        [Test]
        public void ClassTypeIsSetCorrectly() {
            var types = Compile("class Test1{} struct Test2{} interface Test3{}");
            types.Should().HaveCount(3);
            Assert.That(((JsClass)types.Single(tp => tp.Name.UnqualifiedName == "Test1")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Class));
            Assert.That(((JsClass)types.Single(tp => tp.Name.UnqualifiedName == "Test2")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Struct));
            Assert.That(((JsClass)types.Single(tp => tp.Name.UnqualifiedName == "Test3")).ClassType, Is.EqualTo(JsClass.ClassTypeEnum.Interface));
        }

        [Test]
        public void PartialClassesInheritanceAndImplementationIsCorrect1() {
            var types = Compile(@"partial class TestClass : System.Collections.ArrayList, System.Runtime.Serialization.ISerializable { }", @"partial class TestClass : System.Collections.IEnumarator { }");
            types.Should().HaveCount(1);
            var cls = FindClass(types, "TestClass");
            Stringify(cls.BaseClass).Should().Be("{System.Collections.ArrayList}");
            cls.ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.IList}",
                                                                                        "{System.Collections.ICollection}",
                                                                                        "{System.Collections.IEnumerable}",
                                                                                        "{System.ICloneable}",
                                                                                        "{System.Runtime.Serialization.ISerializable}" });
        }

        [Test]
        public void PartialClassesInheritanceAndImplementationIsCorrect2() {
            var types = Compile(@"partial class TestClass : System.Collections.ArrayList, System.Collections.IEnumerable { }", @"partial class TestClass : System.Collections.List, System.Collections.Generic.IList<string> { }");
            types.Should().HaveCount(1);
            var cls = FindClass(types, "TestClass");
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
            var types = Compile(@"interface ITest1 {}
                                  interface ITest2 {}
                                  interface ITest3 : ITest1, ITest2 {}
                                  class Test : ITest3 {}
                                  interface ITest : ITest3 {}");
            FindClass(types, "Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}", "{ITest3}" });
            FindClass(types, "ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1}", "{ITest2}", "{ITest3}" });
        }

        [Test]
        public void GenericInheritedDefinedInterfacesAreRecordedInTheInterfaceList() {
            var types = Compile(@"interface ITest1<T> {}
                                  interface ITest2<T> {}
                                  interface ITest3<T1, T2> : ITest1<T1>, ITest2<T2> {}
                                  class Test : ITest3<int, string> {}
                                  interface ITest<T> : ITest3<T, T> {}");
            FindClass(types, "Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1`1}<{System.Int32}>", "{ITest2`1}<{System.String}>", "{ITest3`2}<{System.Int32},{System.String}>" });
            FindClass(types, "ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{ITest1`1}<T>", "{ITest2`1}<T>", "{ITest3`2}<T,T>" });
        }

        [Test]
        public void InheritedImportedInterfacesAreRecordedInTheInterfaceListForClasses() {
            var types = Compile("class Test<T> : System.Collections.Generic.Dictionary<T, int> {}");
            FindClass(types, "Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.Generic.IDictionary`2}<T,{System.Int32}>",
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
            var types = Compile("class Test<T> : System.Collections.Generic.IDictionary<T, int> {}");
            FindClass(types, "Test").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.Generic.IDictionary`2}<T,{System.Int32}>",
                                                                                                             "{System.Collections.Generic.ICollection`1}<{System.Collections.Generic.KeyValuePair`2}<T,{System.Int32}>>",
                                                                                                             "{System.Collections.Generic.IEnumerable`1}<{System.Collections.Generic.KeyValuePair`2}<T,{System.Int32}>>",
                                                                                                             "{System.Collections.IEnumerable}" });
        }

        [Test]
        public void IndirectlyImplementedInterfacesAreRecordedInTheInterfaceListForInterfaces() {
            var types = Compile("interface ITest : System.Collections.Generic.IDictionary<string, int> {}");
            FindClass(types, "ITest").ImplementedInterfaces.Select(Stringify).Should().BeEquivalentTo(new[] { "{System.Collections.Generic.IDictionary`2}<{System.String},{System.Int32}>",
                                                                                                              "{System.Collections.Generic.ICollection`1}<{System.Collections.Generic.KeyValuePair`2}<{System.String},{System.Int32}>>",
                                                                                                              "{System.Collections.Generic.IEnumerable`1}<{System.Collections.Generic.KeyValuePair`2}<{System.String},{System.Int32}>>",
                                                                                                              "{System.Collections.IEnumerable}" });
        }

        [Test]
        public void InheritingNestedGenericTypesWorks() {
            var types = Compile("using System.Collections.Generic; class Test<T1, T2> : List<Dictionary<T1, T2>> {}");
            var cls = FindClass(types, "Test");
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
            var types = Compile("class Test1<T1> { class Test2<T2, T3> { class Test3<T4> {} } }");
            var test1 = FindClass(types, "Test1");
            var test2 = FindClass(types, "Test1+Test2");
            var test3 = FindClass(types, "Test1+Test2+Test3");
            test1.TypeArgumentNames.Should().Equal(new[] { "T1" });
            test2.TypeArgumentNames.Should().Equal(new[] { "T1", "T2", "T3" });
            test3.TypeArgumentNames.Should().Equal(new[] { "T1", "T2", "T3", "T4" });
        }

        [Test]
        public void IsPublicFlagIsCorrectlySetForClasses() {
            var types = Compile(@"class C1 {}
                                  internal class C2 {}
                                  public class C3 {}
                                  public class C4 { internal class C5 { public class C6 {} } }
                                  internal class C7 { public class C8 { public class C9 {} } }
                                  public class C10 { private class C11 {} protected class C12 {} protected internal class C13 {} }
                                 ");
                                 
            FindClass(types, "C1").IsPublic.Should().BeFalse();
            FindClass(types, "C2").IsPublic.Should().BeFalse();
            FindClass(types, "C3").IsPublic.Should().BeTrue();
            FindClass(types, "C4").IsPublic.Should().BeTrue();
            FindClass(types, "C4+C5").IsPublic.Should().BeFalse();
            FindClass(types, "C4+C5+C6").IsPublic.Should().BeFalse();
            FindClass(types, "C7").IsPublic.Should().BeFalse();
            FindClass(types, "C7+C8").IsPublic.Should().BeFalse();
            FindClass(types, "C7+C8+C9").IsPublic.Should().BeFalse();
            FindClass(types, "C10").IsPublic.Should().BeTrue();
            FindClass(types, "C10+C11").IsPublic.Should().BeFalse();
            FindClass(types, "C10+C12").IsPublic.Should().BeTrue();
            FindClass(types, "C10+C13").IsPublic.Should().BeTrue();
        }

        [Test]
        public void IsPublicFlagIsCorrectlySetForEnums() {
            var types = Compile(@"enum C1 {}
                                  internal enum C2 {}
                                  public enum C3 {}
                                  public class C4 { internal class C5 { public enum C6 {} } }
                                  internal class C7 { public class C8 { public enum C9 {} } }
                                  public class C10 { private enum C11 {} protected enum C12 {} protected internal enum C13 {} }
                                 ");
                                 
            types.Single(tp => tp.Name.ToString() == "C1").IsPublic.Should().BeFalse();
            types.Single(tp => tp.Name.ToString() == "C2").IsPublic.Should().BeFalse();
            types.Single(tp => tp.Name.ToString() == "C3").IsPublic.Should().BeTrue();
            types.Single(tp => tp.Name.ToString() == "C4+C5+C6").IsPublic.Should().BeFalse();
            types.Single(tp => tp.Name.ToString() == "C7+C8+C9").IsPublic.Should().BeFalse();
            types.Single(tp => tp.Name.ToString() == "C10+C11").IsPublic.Should().BeFalse();
            types.Single(tp => tp.Name.ToString() == "C10+C12").IsPublic.Should().BeTrue();
            types.Single(tp => tp.Name.ToString() == "C10+C13").IsPublic.Should().BeTrue();
        }

        [Test]
        public void ClassesForWhichTheNamingConventionReturnsNulllAreNotInTheOutput() {
            var naming = new Mock<INamingConventionResolver>();
            naming.Setup(_ => _.GetTypeName(It.IsAny<ITypeDefinition>())).Returns((ITypeDefinition type) => type.Name == "C2" ? null : type.Name);
            var types = Compile(new[] { "class C1 {} class C2 { class C3 {} }" }, naming.Object);
            types.Should().HaveCount(1);
            types[0].Name.ToString().Should().Be("C1");
        }

        [Test]
        public void EnumsForWhichTheNamingConventionReturnsNulllAreNotInTheOutput() {
            var naming = new Mock<INamingConventionResolver>();
            naming.Setup(_ => _.GetTypeName(It.IsAny<ITypeDefinition>())).Returns((ITypeDefinition type) => type.Name == "C2" ? null : type.Name);
            var types = Compile(new[] { "enum C1 {} enum C2 {}" }, naming.Object);
            types.Should().HaveCount(1);
            types[0].Name.ToString().Should().Be("C1");
        }
    }
}
