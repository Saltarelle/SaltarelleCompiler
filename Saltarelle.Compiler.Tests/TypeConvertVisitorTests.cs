using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.JSModel;
using FluentAssertions;

namespace Saltarelle.Compiler.Tests {
    [TestFixture]
    public class TypeConvertVisitorTests {
        class MockSourceFile : ISourceFile {
            private readonly string _fileName;
            private readonly string _content;

            public MockSourceFile(string fileName, string content) {
                _fileName = fileName;
                _content  = content;
            }

            public string FileName {
                get { return _fileName; }
            }

            public TextReader Open() {
                return new StringReader(_content);
            }
        }

        private static readonly Lazy<IProjectContent> _mscorlibLazy = new Lazy<IProjectContent>(() => new CecilLoader().LoadAssemblyFile(typeof(object).Assembly.Location));
        private IProjectContent Mscorlib { get { return _mscorlibLazy.Value; } }

        private ReadOnlyCollection<JsType> Compile(params string[] sources) {
            var sourceFiles = sources.Select((s, i) => new MockSourceFile("File" + i + ".cs", s)).ToList();
            return new Compiler().Compile(sourceFiles, new[] { Mscorlib }).AsReadOnly();
        }

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
    }
}
