using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
    [TestFixture]
    public class AllTests : CompilerTestBase {
        protected IMethod Method { get; private set; }
        protected MethodCompiler MethodCompiler { get; private set; }
        protected JsFunctionDefinitionExpression CompiledMethod { get; private set; }

        private void CompileMethod(string source, INamingConventionResolver namingConvention = null, IErrorReporter errorReporter = null) {
            Compile(new[] { "class C { " + source + "}" }, namingConvention, errorReporter, (m, res, mc) => {
				if (Method == null) {
					Method = m;
					MethodCompiler = mc;
					CompiledMethod = res;
				}
            });

			Assert.That(Method, Is.Not.Null, "No method was compiled");
        }

		[SetUp]
		public void Setup() {
			Method = null;
			MethodCompiler = null;
			CompiledMethod = null;
		}

        [Test]
        public void ParameterGetCorrectNamesForSimpleMethods() {
            var namingConvention = new MockNamingConventionResolver() {
                                       GetVariableName = (v, used) => {
                                           switch (v.Name) {
                                               case "i":
                                                   used.Should().BeEmpty();
                                                   return "$i";
                                               case "s":
                                                   used.Should().BeEquivalentTo(new object[] { "$i" });
                                                   return "$x";
                                               case "i2":
                                                   used.Should().BeEquivalentTo(new object[] { "$i", "$x" });
                                                   return "$i2";
                                               default:
                                                   Assert.Fail("Unexpected name");
                                                   return null;
                                           }
                                       }
                                   };
            Compile(new[] { "class C { public void M(int i, string s, int i2) {} }" }, namingConvention: namingConvention);
            FindInstanceMethod("C.M").Definition.ParameterNames.Should().Equal(new object[] { "$i", "$x", "$i2" });
        }

        [Test]
        public void TypeParametersAreConsideredUsedDuringParameterNameDetermination() {
            var namingConvention = new MockNamingConventionResolver() { GetTypeParameterName = p => "$" + p.Name,
                                                                        GetVariableName = (v, used) => { used.Should().BeEquivalentTo(new[] { "$P1", "$P2", "$P3" }); return "$i"; } };

            Compile(new[] { "class C<P1> { public class C2<P2> { public void M<P3>(int i) {} } }" }, namingConvention: namingConvention);
            FindInstanceMethod("C+C2.M").Definition.ParameterNames.Should().Equal(new object[] { "$i" });
        }

        [Test]
        public void VariablesAreCorrectlyRegistered() {
            CompileMethod(@"
                public void M(int a, int b) {
                    int c = a + b, d = c;
                    int e = c;

                    for (e = 0; e < c; e++) {
                        int f = a + e;
                    }
                    for (e = 0; e < d; e++) {
                        int f = a + e;
                    }
                }
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$a", "$b", "$c", "$d", "$e", "$f", "$f2" });
        }

        [Test]
        public void VariableDeclaredInForStatementIsCorrectlyRegistered() {
            CompileMethod(@"
                public void M() {
                    for (int i = 0; i < 1; i++) {
                        int a = i;
                    }
                    for (int i = 0, j = 0; i < 1; i++) {
                        int a = i;
                    }
                }
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$i", "$a", "$i2", "$j", "$a2" });
        }

        [Test]
        public void ForStatementWithoutVariableDeclarationDoesNotCauseRegistration() {
            CompileMethod(@"
                public void M() {
					int i;
                    for (i = 0; i < 1; i++) {
                        int a = i;
                    }
                }
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$i", "$a" });
        }


        [Test]
        public void VariableDeclaredInForeachStatementIsCorrectlyRegistered() {
            CompileMethod(@"
                public void M() {
                    foreach (int i in new[] { 1, 2, 3 }) {
                        int a = i;
                    }
                    foreach (int i in new[] { 1, 2, 3 }) {
                        int a = i;
                    }
                }
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$i", "$a", "$i2", "$a2" });
        }

        [Test]
        public void VariableDeclaredInUsingStatementIsCorrectlyRegistered() {
            CompileMethod(@"
                public void M() {
                    using (var ms = new MemoryStream()) {
                        int a = 1;
                    }
                    using (var ms = new MemoryStream()) {
                        int a = 1;
                    }
                }
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$ms", "$a", "$ms2", "$a2" });
        }

        [Test]
        public void UsingStatementWithoutVariableDeclarationDoesNotCauseRegistration() {
            CompileMethod(@"
                public void M() {
					IDisposable ms;
                    using (ms = new MemoryStream()) {
                        int a = 1;
                    }
                }
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$ms", "$a" });
        }

        [Test]
        public void VariableDeclaredInCatchBlockIsCorrectlyRegistered() {
            CompileMethod(@"
                public void M() {
					try {
						int a = 0;
					}
					catch (System.InvalidOperationException ex) {
						int a = 0;
					}
					catch (System.ArgumentException ex) {
						int a = 0;
					}

					try {
						int a = 0;
					}
					catch (System.InvalidOperationException ex) {
						int a = 0;
					}
					catch (System.ArgumentException ex) {
						int a = 0;
					}
                }
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$a", "$ex", "$a2", "$ex2", "$a3", "$a4", "$ex3", "$a5", "$ex4", "$a6" });
        }

        [Test]
        public void CatchBlockWithoutVariableDeclarationDoesNotCauseRegistration() {
            CompileMethod(@"
                public void M() {
					try {
						int a = 0;
					}
					catch (System.InvalidOperationException) {
						int a = 0;
					}
				}
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$a", "$a2" });
        }

        [Test]
        public void CatchAllBlockWorks() {
            CompileMethod(@"
                public void M() {
					try {
						int a = 0;
					}
					catch {
						int a = 0;
					}
				}
            ");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$a", "$a2" });
        }

		[Test]
		public void ImplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = (a, b) => (a + b).ToString();
					System.Func<int, int, string> f2 = (a, b) => (a + b).ToString();
				}
			");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
		}

		[Test]
		public void ExplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = (int a, int b) => (a + b).ToString();
					System.Func<int, int, string> f2 = (int a, int b) => (a + b).ToString();
				}
			");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
		}

		[Test]
		public void OldStyleDelegateParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = delegate(int a, int b) { return (a + b).ToString(); };
					System.Func<int, int, string> f2 = delegate(int a, int b)  { return (a + b).ToString(); };
				}
			");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
		}

		[Test]
		public void OldStyleDelegateWithoutArgumentListIsNotRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = delegate { return """"; };
					System.Func<int, int, string> f2 = delegate { return """"; };
				}
			");

            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
                .Should()
                .Equal(new[] { "$f", "$f2" });
		}

		[Test]
		public void PropertyGetterDoesNotHaveAnyParameters() {
            CompileMethod(@"public int P { get { return 0; } }");
			MethodCompiler.variableNameMap.Should().BeEmpty();
		}

		[Test]
		public void ImplicitValueParameterToPropertySetterIsCorrectlyRegistered() {
            CompileMethod(@"public int P { set {} }");
            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
				.Should()
				.Equal(new[] { "$value" });
		}

		[Test]
		public void ImplicitValueParameterToEventAdderIsCorrectlyRegistered() {
            CompileMethod(@"public event System.EventHandler E { add {} remove {} }");
            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
				.Should()
				.Equal(new[] { "$value" });
		}

		[Test]
		public void ImplicitValueParameterToEventRemoverIsCorrectlyRegistered() {
            CompileMethod(@"public event System.EventHandler E { remove {} add {} }");
            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
				.Should()
				.Equal(new[] { "$value" });
		}

		[Test, Ignore("NRefactory bug")]
		public void IndexerGetterParametersAreCorrectlyRegistered() {
            CompileMethod(@"public int this[int a, string b] { get { return 0; } }");
            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
				.Should()
				.Equal(new[] { "$a", "$b" });
		}

		[Test, Ignore("NRefactory bug")]
		public void IndexerSetterParametersAreCorrectlyRegistered() {
            CompileMethod(@"public int this[int a, string b] { set {} }");
            MethodCompiler.variableNameMap
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value)
				.Should()
				.Equal(new[] { "$a", "$b", "$value" });
		}
    }
}
