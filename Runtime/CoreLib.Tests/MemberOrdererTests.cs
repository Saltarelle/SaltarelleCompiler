using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Tests {
	[TestFixture]
	public class MemberOrdererTests {
		private void AssertCorrect(string definitions, params string[] expected) {
			var c = Common.CreateCompilation("class C { " + definitions + "}");
			var members = c.GetTypeByMetadataName("C").GetNonAccessorNonTypeMembers();
			var actual = members.Where(m => !m.IsImplicitlyDeclared && !m.IsAccessor())
			                    .OrderBy(m => m, MemberOrderer.Instance)
			                    .Select(m => {
			                                     if (m is IMethodSymbol) {
			                                         var method = (IMethodSymbol)m;
			                                         if (method.MethodKind == MethodKind.Conversion)
			                                             return method.ReturnType.Name + " " + method.MetadataName + "(" + string.Join(", ", method.Parameters.Select(p => p.Type.Name)) + ")";
			                                         else
			                                             return m.MetadataName + (method.Arity > 0 ? "<" + string.Join(", ", method.TypeParameters.Select(tp => tp.Name)) + ">" : "") + "(" + string.Join(", ", method.Parameters.Select(p => p.Type.Name)) + ")";
			                                     }
			                                     else if (m is IPropertySymbol)
			                                         return m.MetadataName + (((IPropertySymbol)m).Parameters.Length > 0 ? "[" + string.Join(", ", ((IPropertySymbol)m).Parameters.Select(p => p.Type.Name)) + "]" : "");
			                                     else
			                                         return m.MetadataName;
			                                 })
			                    .ToList();
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void MethodsAreOrderedByName() {
			AssertCorrect("public void M1() {} public void M2() {}", "M1()", "M2()");
		}

		[Test]
		public void MethodsAreOrderedByArity() {
			AssertCorrect("public void M<T1, T2>() {} public void M(int i) {} public void M<T>() {}", "M(Int32)", "M<T>()", "M<T1, T2>()");
		}

		[Test]
		public void MethodsAreOrderedByParameterCount() {
			AssertCorrect("public void M(int a, int b) {} public void M() {} public void M(int a) {}", "M()", "M(Int32)", "M(Int32, Int32)");
		}

		[Test]
		public void MethodsAreOrderedByParameterTypes() {
			AssertCorrect("public void M(string a) {} public void M(int b) {} public void M(float c) {}", "M(Int32)", "M(Single)", "M(String)");
		}

		[Test]
		public void OrdinaryMethodsAreSortedBeforeConstructorsAndOperators() {
			AssertCorrect("public void M(C a, C b) {} public C(C a, C b) {} public static C operator+(C a, C b) { return null; }", "M(C, C)", ".ctor(C, C)", "op_Addition(C, C)");
		}

		[Test]
		public void MethodsAreOrderedByReturnType() {
			AssertCorrect("public static implicit operator string(C a) { return null; } public static implicit operator int(C b) { return 0; } public static implicit operator float(C c) { return 0; }", "Int32 op_Implicit(C)", "Single op_Implicit(C)", "String op_Implicit(C)");
		}

		[Test]
		public void PropertiesAreOrderedByName() {
			AssertCorrect("public int P3 { get; set; } public int P1 { get; set; } public int P2 { get; set; }", "P1", "P2", "P3");
		}

		[Test]
		public void PropertiesAreOrderedByParameterCount() {
			AssertCorrect("public int this[int a, int b] { get { return 0; } } public int X2 { get; set; } public int this[int a] { get { return 0; } }", "X2", "Item[Int32]", "Item[Int32, Int32]");
		}

		[Test]
		public void PropertiesAreOrderedByParameterTypes() {
			AssertCorrect("public int this[string a] { get { return 0; } } public int this[int b] { get { return 0; } } public int this[float c] { get { return 0; } }", "Item[Int32]", "Item[Single]", "Item[String]");
		}

		[Test]
		public void FieldsAreOrderedByName() {
			AssertCorrect("public int F3, F1, F2;", "F1", "F2", "F3");
		}

		[Test]
		public void EventsAreOrderedByName() {
			AssertCorrect("public event System.Action E3, E1, E2;", "E1", "E2", "E3");
		}

		[Test]
		public void OrderIsMethodsPropertiesFieldsEvents() {
			AssertCorrect("public int B; public event System.Action A; public void D() {} public int C1 { get; set; }", "D()", "C1", "B", "A");
		}

		[Test]
		public void MorePublicSymbolsAreOrderedBeforeLessPublicOnes() {
			AssertCorrect("public event System.Action D; internal int B { get; set; } private void A() {} protected int C1;", "D", "C1", "B", "A()");

		}
	}
}
