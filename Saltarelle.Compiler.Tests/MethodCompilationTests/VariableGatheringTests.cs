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
    public class VariableGatheringTests : CompilerTestBase {
        protected IMethod Method { get; private set; }
        protected MethodCompiler MethodCompiler { get; private set; }
        protected JsFunctionDefinitionExpression CompiledMethod { get; private set; }

        private void CompileMethod(string source, INamingConventionResolver namingConvention = null, IErrorReporter errorReporter = null, string methodName = "M") {
            Compile(new[] { "class C { " + source + "}" }, namingConvention, errorReporter, (m, res, mc) => {
				if (m.Name == methodName) {
					Method = m;
					MethodCompiler = mc;
					CompiledMethod = res;
				}
            });

			Assert.That(Method, Is.Not.Null, "Method " + methodName + " was not compiled");
        }

		[SetUp]
		public void Setup() {
			Method = null;
			MethodCompiler = null;
			CompiledMethod = null;
		}

        [Test]
        public void ParameterGetCorrectNamesForSimpleMethods() {
            CompileMethod("public void M(int i, string s, int i2) {} }");
            CompiledMethod.ParameterNames.Should().Equal(new[] { "$i", "$s", "$i2" });
        }

        [Test]
        public void TypeParametersAreConsideredUsedDuringParameterNameDetermination() {
            CompileMethod("class C<TX> { public class C2<TY> { public void M<TZ>(int TX, int TY, int TZ) {} } }");
            CompiledMethod.ParameterNames.Should().Equal(new[] { "$TX2", "$TY2", "$TZ2" });
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$a", "$b", "$c", "$d", "$e", "$f", "$f2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$i", "$a", "$i2", "$j", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$i", "$a" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$i", "$a", "$i2", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$ms", "$a", "$ms2", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$ms", "$a" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$a", "$ex", "$a2", "$ex2", "$a3", "$a4", "$ex3", "$a5", "$ex4", "$a6" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$a", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
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

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$a", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
        }

		[Test]
		public void ImplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = (a, b) => (a + b).ToString();
					System.Func<int, int, string> f2 = (a, b) => (a + b).ToString();
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test]
		public void ExplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = (int a, int b) => (a + b).ToString();
					System.Func<int, int, string> f2 = (int a, int b) => (a + b).ToString();
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test]
		public void OldStyleDelegateParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = delegate(int a, int b) { return (a + b).ToString(); };
					System.Func<int, int, string> f2 = delegate(int a, int b)  { return (a + b).ToString(); };
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test]
		public void OldStyleDelegateWithoutArgumentListIsNotRegistered() {
			CompileMethod(@"
				public void M() {
					System.Func<int, int, string> f = delegate { return """"; };
					System.Func<int, int, string> f2 = delegate { return """"; };
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$f2" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test]
		public void PropertyGetterDoesNotHaveAnyParameters() {
            CompileMethod(@"public int P { get { return 0; } }", methodName: "get_P");
			MethodCompiler.variables.Should().BeEmpty();
		}

		[Test]
		public void ImplicitValueParameterToPropertySetterIsCorrectlyRegistered() {
            CompileMethod(@"public int P { set {} }", methodName: "set_P");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$value" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test]
		public void ImplicitValueParameterToEventAdderIsCorrectlyRegistered() {
            CompileMethod(@"public event System.EventHandler E { add {} remove {} }", methodName: "add_E");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$value" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test]
		public void ImplicitValueParameterToEventRemoverIsCorrectlyRegistered() {
            CompileMethod(@"public event System.EventHandler E { remove {} add {} }", methodName: "remove_E");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$value" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test, Ignore("NRefactory bug")]
		public void IndexerGetterParametersAreCorrectlyRegistered() {
            CompileMethod(@"public int this[int a, string b] { get { return 0; } }");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$a", "$b" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test, Ignore("NRefactory bug")]
		public void IndexerSetterParametersAreCorrectlyRegistered() {
            CompileMethod(@"public int this[int a, string b] { set {} }");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$a", "$b", "$value" });
			MethodCompiler.variables.Where(x => x.Value.IsUsedByRef).Should().BeEmpty();
		}

		[Test]
		public void ByRefAndOutParametersToMethodAreConsideredUsedByReference() {
			CompileMethod(@"
				public void M(int x, ref int y, out int z) {
					z = 0;
				}
			");
			MethodCompiler.variables.Single(v => v.Key.Name == "x").Value.IsUsedByRef.Should().BeFalse();
			MethodCompiler.variables.Single(v => v.Key.Name == "y").Value.IsUsedByRef.Should().BeTrue();
			MethodCompiler.variables.Single(v => v.Key.Name == "z").Value.IsUsedByRef.Should().BeTrue();
		}

		[Test]
		public void VariableUsedAsARefMethodArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				public void OtherMethod(int a, ref int b) {}
				public void M(int x, int y) {
					OtherMethod(x, ref y);
				}
			");
			MethodCompiler.variables.Single(v => v.Key.Name == "x").Value.IsUsedByRef.Should().BeFalse();
			MethodCompiler.variables.Single(v => v.Key.Name == "y").Value.IsUsedByRef.Should().BeTrue();
		}

		[Test]
		public void VariableUsedAsAnOutMethodArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				public void OtherMethod(int a, out int b) { b = 0; }
				public void M(int x, int y) {
					OtherMethod(x, out y);
				}
			");
			MethodCompiler.variables.Single(v => v.Key.Name == "x").Value.IsUsedByRef.Should().BeFalse();
			MethodCompiler.variables.Single(v => v.Key.Name == "y").Value.IsUsedByRef.Should().BeTrue();
		}

		[Test]
		public void VariableUsedAsARefDelegateInvocationArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				public delegate void D(int a, ref int b);
				public void OtherMethod(int a, ref int b) {}
				public void M(int x, int y) {
					D d = OtherMethod;
					d(x, ref y);
				}
			");
			MethodCompiler.variables.Single(v => v.Key.Name == "x").Value.IsUsedByRef.Should().BeFalse();
			MethodCompiler.variables.Single(v => v.Key.Name == "y").Value.IsUsedByRef.Should().BeTrue();
		}

		[Test]
		public void VariableUsedAsAnOutDelegateInvocationArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				public delegate void D(int a, out int b);
				public void OtherMethod(int a, out int b) {}
				public void M(int x, int y) {
					D d = OtherMethod;
					d(x, out y);
				}
			");
			MethodCompiler.variables.Single(v => v.Key.Name == "x").Value.IsUsedByRef.Should().BeFalse();
			MethodCompiler.variables.Single(v => v.Key.Name == "y").Value.IsUsedByRef.Should().BeTrue();
		}

		[Test]
		public void VariableUsedAsARefConstructorArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				class X { public X(int a, ref int b) {} }
				public void M(int x, int y) {
					new X(x, ref y);
				}
			");
			MethodCompiler.variables.Single(v => v.Key.Name == "x").Value.IsUsedByRef.Should().BeFalse();
			MethodCompiler.variables.Single(v => v.Key.Name == "y").Value.IsUsedByRef.Should().BeTrue();
		}

		[Test]
		public void VariableUsedAsAnOutConstructorArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				class X { public X(int a, out int b) { b = 0; } }
				public void M(int x, int y) {
					new X(x, ref y);
				}
			");
			MethodCompiler.variables.Single(v => v.Key.Name == "x").Value.IsUsedByRef.Should().BeFalse();
			MethodCompiler.variables.Single(v => v.Key.Name == "y").Value.IsUsedByRef.Should().BeTrue();
		}

		[Test]
		public void PassingAFieldByReferenceGivesAnError() {
			var er = new MockErrorReporter(false);
			CompileMethod(@"
				public int f;
				public void OtherMethod(int a, ref int b) {}
				public void M(int x) {
					OtherMethod(x, ref f);
				}
			", errorReporter: er);

			er.AllMessages.Where(m => m.StartsWith("Error:")).Should().NotBeEmpty();
		}
    }
}
