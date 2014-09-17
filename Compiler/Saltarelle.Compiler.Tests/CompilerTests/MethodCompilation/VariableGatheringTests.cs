using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class VariableGatheringTests : MethodCompilerTestBase {
		private void AssertUsedByReference(string scriptVariableName) {
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == scriptVariableName).UseByRefSemantics, Is.True, scriptVariableName + " should be used by reference");
		}

		private void AssertNotUsedByReference(string scriptVariableName) {
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == scriptVariableName).UseByRefSemantics, Is.False, scriptVariableName + " should not be used by reference");
		}

		[Test]
		public void ParameterGetCorrectNamesForSimpleMethods() {
			CompileMethod("public void M(int i, string s, int i2) {}");
			Assert.That(CompiledMethod.ParameterNames, Is.EqualTo(new[] { "$i", "$s", "$i2" }));
		}

		[Test]
		public void TypeParametersAreConsideredUsedDuringParameterNameDetermination() {
			CompileMethod("class C1<TX> { public class C2<TY> { public void M<TZ>(int TX, int TY) {} } }");
			Assert.That(CompiledMethod.ParameterNames, Is.EqualTo(new[] { "$TX2", "$TY2" }));
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

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$a", "$b", "$c", "$d", "$e", "$f", "$f2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
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

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$i", "$a", "$i2", "$j", "$a2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void RangeVariablesAreRegistered() {
			CompileMethod(@"
				using System.Linq;
				public class C {
					public void M() {
						var e1  = from x in new int[4] group x by x into y select y;
						var e2  = from x in new int[4] select x + 1 into y where y > 0 select y + 1;
						var e3  = from x in new int[4] select x + 1 into y from z in new int[4] select y + z;
						var e4  = from x in new int[4] join y in new int[4] on x equals y select x + y;
						var e5  = from x in new int[4] join y in new int[4] on x equals y into z select z;
						var e6  = from x in new int[4] let y = x + 1 select x + y;
						var e7  = from x in new int[4] from y in new int[4] where x > y select x + y;
						var e8  = from x in new int[4] from y in new int[4] group x by x into z select z;
						var e9  = from x in new int[4] join y in new int[4] on x equals y where x > y select x + y;
						var e10 = from x in new int[4] join y in new int[4] on x equals y into z where z.Count() > 0 select z;
						var e11 = from x in new int[4] from y in new int[4] group x + y by x;
						var e12 = from x in new int[4] join y in new int[4] on x equals y into z group z by z;
						var e13 = from x in (from y in new int[4] select (from z in new int[4] select z)) select x;
					}
				}
			", references: new[] { new MetadataFileReference(typeof(object).Assembly.Location), new MetadataFileReference(typeof(Enumerable).Assembly.Location) }, addSkeleton: false);

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name)
			                          .ToList(),
			            Is.EqualTo(new[] { "$e1",  "$x",   "$y",
			                               "$e2",  "$x2",  "$y2",
			                               "$e3",  "$x3",  "$y3",  "$z",
			                               "$e4",  "$x4",  "$y4",
			                               "$e5",  "$x5",  "$y5",  "$z2",
			                               "$e6",  "$x6",  "$y6",
			                               "$e7",  "$x7",  "$y7",
			                               "$e8",  "$x8",  "$y8",  "$z3",
			                               "$e9",  "$x9",  "$y9",
			                               "$e10", "$x10", "$y10", "$z4",
			                               "$e11", "$x11", "$y11",
			                               "$e12", "$x12", "$y12", "$z5",
			                               "$e13", "$x13", "$y13", "$z6",
			                             }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
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

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$i", "$a" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
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

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name)
			                          .Where(name => !name.StartsWith("$tmp")),
			            Is.EqualTo(new[] { "$i", "$a", "$i2", "$a2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void VariableDeclaredInUsingStatementIsCorrectlyRegistered() {
			CompileMethod(@"class MemoryStream : IDisposable { public void Dispose() {} }
				public void M() {
					using (var ms = new MemoryStream()) {
						int a = 1;
					}
					using (var ms = new MemoryStream()) {
						int a = 1;
					}
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$ms", "$a", "$ms2", "$a2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void UsingStatementWithoutVariableDeclarationDoesNotCauseRegistration() {
			CompileMethod(@"class MemoryStream : IDisposable { public void Dispose() {} }
				public void M() {
					IDisposable ms;
					using (ms = new MemoryStream()) {
						int a = 1;
					}
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name)
			                          .Where(name => !name.StartsWith("$tmp")),
			            Is.EqualTo(new[] { "$ms", "$a" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void VariableDeclaredInCatchBlockIsCorrectlyRegistered() {
			CompileMethod(@"class InvalidOperationException : Exception {} class ArgumentException : Exception {}
				public void M() {
					try {
						int a = 0;
					}
					catch (InvalidOperationException ex) {
						int a = 0;
					}
					catch (ArgumentException ex) {
						int a = 0;
					}

					try {
						int a = 0;
					}
					catch (InvalidOperationException ex) {
						int a = 0;
					}
					catch (ArgumentException ex) {
						int a = 0;
					}
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name)
			                          .Where(name => !name.StartsWith("$tmp")),
			            Is.EqualTo(new[] { "$a", "$ex", "$a2", "$ex2", "$a3", "$a4", "$ex3", "$a5", "$ex4", "$a6" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void CatchBlockWithoutVariableDeclarationDoesNotCauseRegistration() {
			CompileMethod(@"class InvalidOperationException : System.Exception {}
				public void M() {
					try {
						int a = 0;
					}
					catch (InvalidOperationException) {
						int a = 0;
					}
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name)
			                          .Where(name => !name.StartsWith("$tmp")),
			            Is.EqualTo(new[] { "$a", "$a2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
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

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name)
			                          .Where(name => !name.StartsWith("$tmp")),
			            Is.EqualTo(new[] { "$a", "$a2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void SimpleLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, string> f = a => a.ToString();
					Func<int, string> f2 = a => a.ToString();
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$f", "$a", "$f2", "$a2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void ImplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = (a, b) => (a + b).ToString();
					Func<int, int, string> f2 = (a, b) => (a + b).ToString();
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void ExplicitlyTypedLamdaExpressionParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = (int a, int b) => (a + b).ToString();
					Func<int, int, string> f2 = (int a, int b) => (a + b).ToString();
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void OldStyleDelegateParametersAreCorrectlyRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = delegate(int a, int b) { return (a + b).ToString(); };
					Func<int, int, string> f2 = delegate(int a, int b)  { return (a + b).ToString(); };
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$f", "$a", "$b", "$f2", "$a2", "$b2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void OldStyleDelegateWithoutArgumentListIsNotRegistered() {
			CompileMethod(@"
				public void M() {
					Func<int, int, string> f = delegate { return """"; };
					Func<int, int, string> f2 = delegate { return """"; };
				}
			");

			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$f", "$f2" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void PropertyGetterDoesNotHaveAnyParameters() {
			CompileMethod(@"public int P { get { return 0; } }", methodName: "get_P");
			Assert.That(MethodCompiler.variables, Is.Empty);
		}

		[Test]
		public void ImplicitValueParameterToPropertySetterIsCorrectlyRegistered() {
			CompileMethod(@"public int P { set {} }", methodName: "set_P");
			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$value" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void ImplicitValueParameterToEventAdderIsCorrectlyRegistered() {
			CompileMethod(@"public event System.EventHandler E { add {} remove {} }", methodName: "add_E");
			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$value" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void ImplicitValueParameterToEventRemoverIsCorrectlyRegistered() {
			CompileMethod(@"public event System.EventHandler E { remove {} add {} }", methodName: "remove_E");
			Assert.That(MethodCompiler.variables
			              .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			              .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$value" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void IndexerGetterParametersAreCorrectlyRegistered() {
			CompileMethod(@"public int this[int a, string b] { get { return 0; } }", methodName: "get_Item");
			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "$a", "$b" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
		}

		[Test]
		public void IndexerSetterParametersAreCorrectlyRegistered() {
			CompileMethod(@"public int this[int a, string b] { set {} }", methodName: "set_Item");
			Assert.That(MethodCompiler.variables
			                          .Select(kvp => kvp.Value.Name),
			            Is.EquivalentTo(new[] { "$a", "$b", "$value" }));
			Assert.That(MethodCompiler.variables.Where(x => x.Value.UseByRefSemantics), Is.Empty);
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
}");
			Assert.That(MethodCompiler.variables.Single(kvp => kvp.Value.Name == "$a").Value.UseByRefSemantics, Is.True);
			Assert.That(MethodCompiler.variables.Single(kvp => kvp.Value.Name == "$b").Value.UseByRefSemantics, Is.True);
			Assert.That(MethodCompiler.variables.Single(kvp => kvp.Value.Name == "$c").Value.UseByRefSemantics, Is.False);
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
				public void OtherMethod(int a, out int b) { b = 0; }
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
					new X(x, out y);
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
		}

		[Test]
		public void ByRefAndOutParametersToAnonymousDelegatesAreConsideredUsedByReference() {
			CompileMethod(@"
				delegate void D(int a, ref int b, out int c);
				public void M() {
					D d = delegate(int x, ref int y, out int z) { z = 0; };
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
			AssertUsedByReference("$z");
		}

		[Test]
		public void ByRefAndOutParametersToLambdasAreConsideredUsedByReference() {
			CompileMethod(@"
				delegate void D(int a, ref int b, out int c);
				public void M() {
					D d = (int x, ref int y, out int z) => { z = 0; };
				}
			");
			AssertNotUsedByReference("$x");
			AssertUsedByReference("$y");
			AssertUsedByReference("$z");
		}

		[Test]
		public void CapturedVariableDoesUsuallyNotMeanByReference() {
			CompileMethod(@"
				public void M(int x) {
					int y = 0;
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
					int x = 0;
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
					int x = 0;
					foreach (int y in new[] { 1, 2, 3 }) {
						int z = 0;
						Func<int, int> f = t => x + z;
					}
					int a = 0;
					Func<int> f2 = () => x + a;
				}
			");
			AssertNotUsedByReference("$x");
			AssertNotUsedByReference("$t");
			AssertUsedByReference("$z");
			AssertNotUsedByReference("$a");
			AssertNotUsedByReference("$f");
			AssertNotUsedByReference("$f2");
		}

		[Test]
		public void CapturedForeachIterationVariableIsConsideredUsedByReference() {
			CompileMethod(@"
				public void M() {
					int x;
					foreach (int y in new[] { 1, 2, 3 }) {
						Func<int, int> f = t => y;
					}
				}
			");
			AssertUsedByReference("$y");
		}

		[Test]
		public void CapturedVariableDeclaredInsideWhileLoopIsConsideredUsedByReference() {
			CompileMethod(@"
				public void M() {
					int x = 0;
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
					int x = 0;
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
		public void LocalVariableDeclaredInLoopInNestedFunctionIsNotConsideredUsedByReference() {
			CompileMethod(@"
public void M() {
	System.Action a = () => {
		for (int i = 0; i < 1; i++) {
			int x = 0;
			int y = x;
		}
	};
}");

			AssertNotUsedByReference("$x");
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

			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$p").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(1, 0)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(1, 0)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$p2").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(2, 20)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f2").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(2, 20)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$s").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(3, 28)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f3").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(3, 28)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$i").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(4, 28)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$j").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(4, 28)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$a1").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(3, 28)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f4").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(2, 20)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$f5").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(8, 22)));
			Assert.That(MethodCompiler.variables.Values.Single(v => v.Name == "$a").DeclaringMethod.GetLocation().GetMappedLineSpan().StartLinePosition, Is.EqualTo(new LinePosition(1, 0)));
		}

		[Test]
		public void UsedNamesIsCorrectWhenGeneratingTemporaryVariables() {
			CompileMethod("private int[] arr; public void M(int i, string s) { foreach (var e in arr) {} }", namer: new MockNamer { GetVariableName = (v, used) => new string('x', used.Count + 1) });
			Assert.That(MethodCompiler.variables
			                          .OrderBy(kvp => kvp.Key.Locations[0].GetMappedLineSpan().StartLinePosition)
			                          .Select(kvp => kvp.Value.Name),
			            Is.EqualTo(new[] { "x" /* i */, "xx" /* s */, "xxxx" /* e */, "xxx" /* temporary index */ }));
		}
	}
}
