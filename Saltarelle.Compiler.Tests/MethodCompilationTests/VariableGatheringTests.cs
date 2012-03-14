using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests {
    [TestFixture]
    public class VariableGatheringTests : MethodCompilerTestBase {
		private void AssertUsedByReference(string scriptVariableName) {
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == scriptVariableName).UseByRefSemantics, Is.True);
		}

		private void AssertNotUsedByReference(string scriptVariableName) {
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == scriptVariableName).UseByRefSemantics, Is.False);
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
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
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
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
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
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
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
				.Where(name => !name.StartsWith("$tmp"))
                .Should()
                .Equal(new[] { "$i", "$a", "$i2", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
        }

        [Test]
        public void VariableDeclaredInUsingStatementIsCorrectlyRegistered() {
            CompileMethod(@"
                public void M() {
                    using (var ms = new System.IO.MemoryStream()) {
                        int a = 1;
                    }
                    using (var ms = new System.IO.MemoryStream()) {
                        int a = 1;
                    }
                }
            ");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$ms", "$a", "$ms2", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
        }

        [Test]
        public void UsingStatementWithoutVariableDeclarationDoesNotCauseRegistration() {
            CompileMethod(@"
                public void M() {
					IDisposable ms;
                    using (ms = new System.IO.MemoryStream()) {
                        int a = 1;
                    }
                }
            ");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Where(name => !name.StartsWith("$tmp"))
                .Should()
                .Equal(new[] { "$ms", "$a" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
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
				.Where(name => !name.StartsWith("$tmp"))
                .Should()
                .Equal(new[] { "$a", "$ex", "$a2", "$ex2", "$a3", "$a4", "$ex3", "$a5", "$ex4", "$a6" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
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
				.Where(name => !name.StartsWith("$tmp"))
                .Should()
                .Equal(new[] { "$a", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
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
				.Where(name => !name.StartsWith("$tmp"))
                .Should()
                .Equal(new[] { "$a", "$a2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
        }

		[Test]
		public void ImplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = (a, b) => (a + b).ToString();
					Func<int, int, string> f2 = (a, b) => (a + b).ToString();
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void ExplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = (int a, int b) => (a + b).ToString();
					Func<int, int, string> f2 = (int a, int b) => (a + b).ToString();
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void OldStyleDelegateParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = delegate(int a, int b) { return (a + b).ToString(); };
					Func<int, int, string> f2 = delegate(int a, int b)  { return (a + b).ToString(); };
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void OldStyleDelegateWithoutArgumentListIsNotRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = delegate { return """"; };
					Func<int, int, string> f2 = delegate { return """"; };
				}
			");

            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
                .Should()
                .Equal(new[] { "$f", "$f2" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
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
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void ImplicitValueParameterToEventAdderIsCorrectlyRegistered() {
            CompileMethod(@"public event System.EventHandler E { add {} remove {} }", methodName: "add_E");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$value" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void ImplicitValueParameterToEventRemoverIsCorrectlyRegistered() {
            CompileMethod(@"public event System.EventHandler E { remove {} add {} }", methodName: "remove_E");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$value" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void IndexerGetterParametersAreCorrectlyRegistered() {
            CompileMethod(@"public int this[int a, string b] { get { return 0; } }", methodName: "get_Item");
            MethodCompiler.variables
                .OrderBy(kvp => kvp.Key.Region.Begin)
                .Select(kvp => kvp.Value.Name)
				.Should()
				.Equal(new[] { "$a", "$b" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void IndexerSetterParametersAreCorrectlyRegistered() {
            CompileMethod(@"public int this[int a, string b] { set {} }", methodName: "set_Item");
            MethodCompiler.variables
                .Select(kvp => kvp.Value.Name)
				.Should()
				.BeEquivalentTo(new[] { "$a", "$b", "$value" });
			MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics).Should().BeEmpty();
		}

		[Test]
		public void ByReferenceSemanticGatheringWorksWithNamedArguments() {
			CompileMethod(
@"
void F(ref int x, out int y, int z) { y = 0; }
public void M() {
	int a = 0, b = 0, c = 0;
	// BEGIN
	F(z: c, x: ref a, y: out b);
	// END
");
			MethodCompiler.variables.Single(kvp => kvp.Key.Name == "a").Value.UseByRefSemantics.Should().BeTrue();
			MethodCompiler.variables.Single(kvp => kvp.Key.Name == "b").Value.UseByRefSemantics.Should().BeTrue();
			MethodCompiler.variables.Single(kvp => kvp.Key.Name == "c").Value.UseByRefSemantics.Should().BeFalse();
		}

		[Test]
		public void ByRefAndOutParametersToMethodAreConsideredUsedByReference() {
			CompileMethod(@"
				public void M(int x, ref int y, out int z) {
					z = 0;
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
			AssertUsedByReference("$z");
		}

		[Test]
		public void VariableUsedAsARefMethodArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				public void OtherMethod(int a, ref int b) {}
				public void M(int x, int y) {
					OtherMethod(x, ref y);
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
		}

		[Test]
		public void VariableUsedAsAnOutMethodArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				public void OtherMethod(int a, out int b) { b = 0; }
				public void M(int x, int y) {
					OtherMethod(x, out y);
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
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
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
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
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
		}

		[Test]
		public void VariableUsedAsARefConstructorArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				class X { public X(int a, ref int b) {} }
				public void M(int x, int y) {
					new X(x, ref y);
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
		}

		[Test]
		public void VariableUsedAsAnOutConstructorArgumentIsConsideredUsedByReference() {
			CompileMethod(@"
				class X { public X(int a, out int b) { b = 0; } }
				public void M(int x, int y) {
					new X(x, ref y);
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
		}

		[Test]
		public void CapturedVariableDoesUsuallyNotMeanByReference() {
			CompileMethod(@"
				public void M(int x) {
					int y;
					Func<int, int> f = t => x + y;
				}
			");
			AssertNotUsedByReference("$x");
			AssertNotUsedByReference("$y");
			AssertNotUsedByReference("$f");
		}

		[Test]
		public void CapturedVariableDeclaredInsideForLoopIsConsideredUsedByReference() {
			CompileMethod(@"
				public void M() {
					int x;
					for (int y = 0; y < 10; y++) {
						int z = y;
						Func<int, int> f = t => x + y + z;
					}
					int a = 0;
					Func<int> f2 = () => x + a;
				}
			");
			AssertNotUsedByReference("$x");
			AssertNotUsedByReference("$y");
			AssertNotUsedByReference("$t");
			AssertUsedByReference("$z");
			AssertNotUsedByReference("$a");
			AssertNotUsedByReference("$f");
			AssertNotUsedByReference("$f2");
		}

		[Test]
		public void CapturedVariableDeclaredInsideForeachLoopIsConsideredUsedByReference() {
			CompileMethod(@"
				public void M() {
					int x;
					foreach (int y in new[] { 1, 2, 3 }) {
						int z = y;
						Func<int, int> f = t => x + y + z;
					}
					int a = 0;
					Func<int> f2 = () => x + a;
				}
			");
			AssertNotUsedByReference("$x");
			AssertNotUsedByReference("$y");
			AssertNotUsedByReference("$t");
			AssertUsedByReference("$z");
			AssertNotUsedByReference("$a");
			AssertNotUsedByReference("$f");
			AssertNotUsedByReference("$f2");
		}

		[Test]
		public void CapturedVariableDeclaredInsideWhileLoopIsConsideredUsedByReference() {
			CompileMethod(@"
				public void M() {
					int x;
					while (1 == 0) {
						int y = x;
						Func<int, int> f = t => x + y;
					}
					int a = 0;
					Func<int> f2 = () => x + a;
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
			AssertNotUsedByReference("$t");
			AssertNotUsedByReference("$a");
			AssertNotUsedByReference("$f");
			AssertNotUsedByReference("$f2");
		}

		[Test]
		public void CapturedVariableDeclaredInsideDoWhileLoopIsConsideredUsedByReference() {
			CompileMethod(@"
				public void M() {
					int x;
					do {
						int y = x;
						Func<int, int> f = t => x + y;
					} while (1 == 0);
					int a = 0;
					Func<int> f2 = () => x + a;
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
			AssertNotUsedByReference("$t");
			AssertNotUsedByReference("$a");
			AssertNotUsedByReference("$f");
			AssertNotUsedByReference("$f2");
		}

		[Test]
		public void LocalVariableInNestedFunctionIsNotConsideredUsedByReference() {
			CompileMethod(@"
				public void M() {
					int x;
					while (1 == 0) {
						int y = x;
						Func<int, int> f = t => { int a = 0; return a + x + y; };
					}
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
			AssertNotUsedByReference("$t");
			AssertNotUsedByReference("$a");
			AssertNotUsedByReference("$f");
		}

        [Test]
        public void DeclaringMethodsAreCorrect() {
            CompileMethod(
@"
public void M(int p) {
    Func<int, int> f = p2 => {
        Func<string, double> f2 = delegate(string s) {
            Func<int, int, int> f3 = (int i, int j) => (i + j);
            Action<double> a1 = x => {};
            return f3(1, 4);
        };
        Action<string> f4 = s2 => {
            Func<int, string> f5 = y => y.ToString();
        };
		return 0;
    };
    Action<Type> a = delegate {};
}");

			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$p").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(2, 1)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(2, 1)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$p2").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(3, 24)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f2").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(3, 24)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$s").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(4, 35)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f3").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(4, 35)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$i").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(5, 38)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$j").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(5, 38)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$a1").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(4, 35)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f4").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(3, 24)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f5").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(9, 29)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$a").DeclaringMethod.StartLocation, Is.EqualTo(new TextLocation(2, 1)));
        }
    }
}
