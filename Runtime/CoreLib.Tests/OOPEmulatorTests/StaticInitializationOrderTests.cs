using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class StaticInitializationOrderTests : OOPEmulatorTestBase {
		 private IEnumerable<T> Shuffle<T>(IEnumerable<T> source, int seed) {
			var rnd = new Random(seed);
			var array = source.ToArray();
			var n = array.Length;
			while (n > 1) {
				var k = rnd.Next(n);
				n--;
				var temp = array[n];
				array[n] = array[k];
				array[k] = temp;
			}
			return array;
		}

		private JsFunctionDefinitionExpression CreateFunction(params ITypeDefinition[] referencedTypes) {
			return JsExpression.FunctionDefinition(new string[0], JsExpression.ArrayLiteral(referencedTypes.Select(t => new JsTypeReferenceExpression(t))));
		}

		private string[] SplitLines(string s) {
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

		[Test]
		public void StaticInitStatementsAreSortedByAllReferencesInTypes() {
			var lines = SplitLines(Process(@"
public class C6 { static C6() { var x6 = typeof(C5); } }
public class C1 { static C1() { int x1 = 0; } }
public class C4 { public static void M() { var t = typeof(C3); } static C4() { int x4 = 0; } }
public class C2 { public C2() { var t = typeof(C1); } static C2() { int x2 = 0; } }
public class C5 { public void M() { var t = typeof(C4); } static C5() { int x5 = 0; } }
public class C3 { [System.Runtime.CompilerServices.ScriptName(""someName"")] public C3(int x) { var t = typeof(C2); } static C3() { int x3 = 0; } } }
")).Where(l => l.StartsWith("var x"));

			Assert.That(lines, Is.EqualTo(new[] { "var x1 = 0;", "var x2 = 0;", "var x3 = 0;", "var x4 = 0;", "var x5 = 0;", "var x6 = {C5};" }));
		}

		[Test]
		public void StaticMethodsOnlyAreUsedAsTieBreakersWhenCyclicDependenciesOccur() {
			var lines = SplitLines(Process(@"
public class C5 { public void M() { var t = typeof(C3); } static C5() { int x5 = 0; } }
public class C3 { [System.Runtime.CompilerServices.ScriptName(""someName"")] public C3(int x) { var t = typeof(C2); } static C3() { var x3 = typeof(C2); } }
public class C2 { public C2() { var t = typeof(C4); } static C2() { int x2 = 0; } }
public class C4 { public void M1() { var t = typeof(C3); } public static void M2() { var t = typeof(C3); } static C4() { int x4 = 0; } }
public class C1 { static C1() { int x1 = 0; } }
")).Where(l => l.StartsWith("var x"));

			Assert.That(lines, Is.EqualTo(new[] { "var x1 = 0;", "var x2 = 0;", "var x3 = {C2};", "var x4 = 0;", "var x5 = 0;" }));
		}

		[Test]
		public void StaticInitStatementsOnlyAreUsedAsATieBreakerWhenCyclicDependenciesInStaticMethodsOccur() {
			var lines = SplitLines(Process(@"
public class C5 { static void M() { var t = typeof(C3); } static C5() { int x5 = 0; } }
public class C3 { static void M() { var t = typeof(C2); } static C3() { var x3 = typeof(C2); } }
public class C2 { static void M() { var t = typeof(C4); } static C2() { int x2 = 0; } }
public class C4 { static void M() { var t = typeof(C3); } static C4() { var x4 = typeof(C3); } }
public class C1 { static C1() { int x1 = 0; } }
")).Where(l => l.StartsWith("var x"));

			Assert.That(lines, Is.EqualTo(new[] { "var x1 = 0;", "var x2 = 0;", "var x3 = {C2};", "var x4 = {C3};", "var x5 = 0;" }));
		}
	}
}
