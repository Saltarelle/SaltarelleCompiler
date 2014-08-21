using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.OOPEmulation;
using System.Linq;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.OOPEmulatorInvokerTests {
	[TestFixture]
	public class OOPEmulatorInvokerTests {
		private void AssertCorrect(IList<JsType> types, string expected, IOOPEmulator emulator, IMethodSymbol entryPoint) {
			var invoker = new OOPEmulatorInvoker(emulator, new MockMetadataImporter(), new MockErrorReporter());
			var result = invoker.Process(types, entryPoint);
			var actual = OutputFormatter.Format(result, allowIntermediates: true).Replace("\r\n", "\n");
			Assert.That(actual, Is.EqualTo(expected.Replace("\r\n", "\n")));
		}

		private IList<JsType> Compile(string program) {
			var compilation = PreparedCompilation.CreateCompilation("X", OutputKind.DynamicallyLinkedLibrary, new[] { new MockSourceFile("file.cs", program) }, new[] { Common.Mscorlib }, new string[0]);
			var compiler = new Compiler.Compiler(new MockMetadataImporter(), new MockNamer(), new MockRuntimeLibrary(), new MockErrorReporter());
			return compiler.Compile(compilation).ToList();
		}

		[Test]
		public void InvokesTheCorrectMethodsAndAddsTheResultInTheCorrectOrder() {
			var types = Compile("class X {} class Y { void Main() {} }");
			AssertCorrect(types,
@"before(X, Y);
phase1(X);
phase1(Y);
phase2(X);
phase2(Y);
after(X, Y);
init(X);
init(Y);
{Y}.Main();
", new MockOOPEmulator {
	GetCodeBeforeFirstType  = t => new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier("before"), t.Select(x => JsExpression.Identifier(x.CSharpTypeDefinition.Name))) },
	GetStaticInitStatements = t => new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier("init"), JsExpression.Identifier(t.CSharpTypeDefinition.Name)) },
	GetCodeAfterLastType    = t => new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier("after"), t.Select(x => JsExpression.Identifier(x.CSharpTypeDefinition.Name))) },
	EmulateType             = t => new TypeOOPEmulation(new[] { new TypeOOPEmulationPhase(null, new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier("phase1"), JsExpression.Identifier(t.CSharpTypeDefinition.Name)) }),
	                                                            new TypeOOPEmulationPhase(null, new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier("phase2"), JsExpression.Identifier(t.CSharpTypeDefinition.Name)) }),
	                                                          })
}, types.Single(t => t.CSharpTypeDefinition.Name == "Y").CSharpTypeDefinition.GetMembers().OfType<IMethodSymbol>().Single(m => m.Name == "Main"));
		}

		[Test]
		public void StatementsForEachPhaseAreSortedIndividuallyFirstByDependencyThenByTypeName() {
			var asm = Common.CreateMockAssembly();
			var a = Common.CreateMockTypeDefinition("A", asm);
			var b = Common.CreateMockTypeDefinition("B", asm);
			var c = Common.CreateMockTypeDefinition("C", asm);
			var d = Common.CreateMockTypeDefinition("D", asm);

			var phase1Deps = new Dictionary<INamedTypeSymbol, IEnumerable<INamedTypeSymbol>> {
				{ a, new[] { b, c } },
				{ b, new[] { d } },
				{ c, new[] { d } },
				{ d, new INamedTypeSymbol[0] },
			};
			var phase2Deps = new Dictionary<INamedTypeSymbol, IEnumerable<INamedTypeSymbol>> {
				{ a, new[] { b, d } },
				{ b, new[] { c } },
				{ c, new INamedTypeSymbol[0] },
				{ d, new[] { c } },
			};
			var phase3Deps = new Dictionary<INamedTypeSymbol, IEnumerable<INamedTypeSymbol>> {
				{ a, new INamedTypeSymbol[0] },
				{ b, new INamedTypeSymbol[0] },
				{ c, new INamedTypeSymbol[0] },
				{ d, new INamedTypeSymbol[0] },
			};

			AssertCorrect(new[] { new JsClass(a), new JsClass(b), new JsClass(c), new JsClass(d) },
@"D(1);
B(1);
C(1);
A(1);
C(2);
B(2);
D(2);
A(2);
A(3);
B(3);
C(3);
D(3);
", new MockOOPEmulator {
	EmulateType = t => new TypeOOPEmulation(new[] {
		new TypeOOPEmulationPhase(phase1Deps[t.CSharpTypeDefinition], new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier(t.CSharpTypeDefinition.Name), JsExpression.Number(1)) }),
		new TypeOOPEmulationPhase(phase2Deps[t.CSharpTypeDefinition], new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier(t.CSharpTypeDefinition.Name), JsExpression.Number(2)) }),
		new TypeOOPEmulationPhase(phase3Deps[t.CSharpTypeDefinition], new[] { (JsStatement)JsExpression.Invocation(JsExpression.Identifier(t.CSharpTypeDefinition.Name), JsExpression.Number(3)) }),
	})
}, null);
		}

		[Test]
		public void SortingByNameWorksWithNamespaces() {
			var asm = Common.CreateMockAssembly();
			var names = new[] { "A", "B", "C", "A.B", "A.BA", "A.C", "A.BAA.A", "B.A", "B.B", "B.C", "B.A.A", "B.A.B", "B.B.A" };
			var rnd = new Random(42);
			var types = names.Select(n => new { n, r = rnd.Next() }).OrderBy(x => x.r).Select(x => (JsType)new JsClass(Common.CreateMockTypeDefinition(x.n, asm))).ToList();

			AssertCorrect(types, string.Join("\n", names.Select(x => x.Replace(".", "_") + ";")) + "\n", new MockOOPEmulator { EmulateType = t => new TypeOOPEmulation(new[] { new TypeOOPEmulationPhase(null, new[] { (JsStatement)JsExpression.Identifier(t.CSharpTypeDefinition.Name.Replace(".", "_")) }) }) }, null);
		}

		[Test]
		public void SortByDependencyWorksWithGenericTypes() {
			var types = Compile(
@"public abstract class Base {}
public abstract class EBase : Base { }
public abstract class GenericBase<T> : EBase { }
public sealed class C : GenericBase<object> {}");
			AssertCorrect(types,
@"Base;
EBase;
C;
GenericBase;
", new MockOOPEmulator { EmulateType = t => new TypeOOPEmulation(new[] { new TypeOOPEmulationPhase(t.CSharpTypeDefinition.GetAllBaseTypes().Select(b => (INamedTypeSymbol)b.OriginalDefinition).Where(b => b.TypeParameters.Length == 0), new[] { (JsStatement)JsExpression.Identifier(t.CSharpTypeDefinition.Name) }) }) }, null);
		}

		[Test]
		public void AnErrorIsIssuedIfTheMainMethodHasParameters() {
			Assert.Fail("TODO");
			//var er = new MockErrorReporter();
			//var invoker = new OOPEmulatorInvoker(new MockOOPEmulator(), new MockMetadataImporter(), er);
			//var cu = new CSharpParser().Parse(@"class MyClass { public void Main(string[] args) { } }", "file.cs").ToTypeSystem();
			//var compilation = new CSharpProjectContent().AddOrUpdateFiles(new IUnresolvedFile[] { cu }).AddAssemblyReferences(new[] { MinimalCorlib.Instance }).CreateCompilation();
			//var typeResolveContext = new SimpleTypeResolveContext(compilation.MainAssembly);
			//
			//invoker.Process(cu.GetAllTypeDefinitions().Select(t => new JsClass(t.Resolve(typeResolveContext).GetDefinition())).ToList<JsType>(), compilation.FindType(new FullTypeName("MyClass")).GetMethods().Single(m => m.Name == "Main"));
			//
			//Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			//Assert.That(er.AllMessages.Any(m => m.Code == 7800 && (string)m.Args[0] == "MyClass.Main"));
		}

		[Test]
		public void AnErrorIsIssuedIfTheMainMethodIsNotImplementedAsANormalMethod() {
			Assert.Fail("TODO");
			//var er = new MockErrorReporter();
			//var invoker = new OOPEmulatorInvoker(new MockOOPEmulator(), new MockMetadataImporter { GetMethodSemantics = m => m.Name == "Main" ? MethodScriptSemantics.InlineCode("X") : MethodScriptSemantics.NormalMethod(m.Name) }, er);
			//var cu = new CSharpParser().Parse(@"class MyClass { public void Main() { } }", "file.cs").ToTypeSystem();
			//var compilation = new CSharpProjectContent().AddOrUpdateFiles(new IUnresolvedFile[] { cu }).AddAssemblyReferences(new[] { MinimalCorlib.Instance }).CreateCompilation();
			//var typeResolveContext = new SimpleTypeResolveContext(compilation.MainAssembly);
			//
			//invoker.Process(cu.GetAllTypeDefinitions().Select(t => new JsClass(t.Resolve(typeResolveContext).GetDefinition())).ToList<JsType>(), compilation.FindType(new FullTypeName("MyClass")).GetMethods().Single(m => m.Name == "Main"));
			//
			//Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			//Assert.That(er.AllMessages.Any(m => m.Code == 7801 && (string)m.Args[0] == "MyClass.Main"));
		}

		[Test]
		public void StaticInitStatementsAreSortedByAllReferencesInTypes() {
			var types = Compile(
@"public class C6 { static C6() { var x6 = typeof(C5); } }
public class C1 { static C1() { int x1 = 0; } }
public class C4 { public static void M() { var t = typeof(C3); } static C4() { int x4 = 0; } }
public class C2 { public C2() { var t = typeof(C1); } static C2() { int x2 = 0; } }
public class C5 { public void M() { var t = typeof(C4); } static C5() { int x5 = 0; } }
public class C3 { public C3(int x) { var t = typeof(C2); } static C3() { int x3 = 0; } } }
");
			AssertCorrect(types,
@"C1;
C2;
C3;
C4;
C5;
C6;
", new MockOOPEmulator {
	GetStaticInitStatements = t => new[] { (JsStatement)JsExpression.Identifier(t.CSharpTypeDefinition.Name) }
}, null);
		}

		[Test]
		public void StaticMethodsOnlyAreUsedAsTieBreakersWhenCyclicDependenciesOccur() {
			var types = Compile(
@"public class C5 { public void M() { var t = typeof(C3); } static C5() { int x5 = 0; } }
public class C3 { [System.Runtime.CompilerServices.ScriptName(""someName"")] public C3(int x) { var t = typeof(C2); } static C3() { var x3 = typeof(C2); } }
public class C2 { public C2() { var t = typeof(C4); } static C2() { int x2 = 0; } }
public class C4 { public void M1() { var t = typeof(C3); } public static void M2() { var t = typeof(C3); } static C4() { int x4 = 0; } }
public class C1 { static C1() { int x1 = 0; } }");

			AssertCorrect(types,
@"C1;
C2;
C3;
C4;
C5;
", new MockOOPEmulator {
	GetStaticInitStatements = t => new[] { (JsStatement)JsExpression.Identifier(t.CSharpTypeDefinition.Name) }
}, null);
		}

		[Test]
		public void StaticInitStatementsOnlyAreUsedAsATieBreakerWhenCyclicDependenciesInStaticMethodsOccur() {
			var types = Compile(
@"public class C5 { static void M() { var t = typeof(C3); } static C5() { int x5 = 0; } }
public class C3 { static void M() { var t = typeof(C2); } static C3() { var x3 = typeof(C2); } }
public class C2 { static void M() { var t = typeof(C4); } static C2() { int x2 = 0; } }
public class C4 { static void M() { var t = typeof(C3); } static C4() { var x4 = typeof(C3); } }
public class C1 { static C1() { int x1 = 0; } }");
			AssertCorrect(types,
@"C1;
C2;
C3;
C4;
C5;
", new MockOOPEmulator {
	GetStaticInitStatements = t => new[] { (JsStatement)JsExpression.Identifier(t.CSharpTypeDefinition.Name) }
}, null);
		}

		[Test]
		public void CyclesInDependencyGraphAreHandledGracefully() {
			var asm = Common.CreateMockAssembly();
			var a = Common.CreateMockTypeDefinition("A1", asm);
			var b = Common.CreateMockTypeDefinition("B1", asm);
			var c = Common.CreateMockTypeDefinition("C1", asm);
			var d = Common.CreateMockTypeDefinition("D1", asm);

			var deps = new Dictionary<INamedTypeSymbol, IEnumerable<INamedTypeSymbol>> {
				{ a, new[] { b } },
				{ b, new[] { c } },
				{ c, new[] { a } },
				{ d, new INamedTypeSymbol[0] },
			};

			var er = new MockErrorReporter();
			var invoker = new OOPEmulatorInvoker(new MockOOPEmulator { EmulateType = t => new TypeOOPEmulation(new[] { new TypeOOPEmulationPhase(deps[t.CSharpTypeDefinition], new[] { (JsStatement)JsExpression.Null }) }) }, new MockMetadataImporter(), er);
			invoker.Process(new[] { new JsClass(a), new JsClass(b), new JsClass(c), new JsClass(d) }, null);

			Assert.That(er.AllMessages, Has.Count.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Code == 7802 && ((string)m.Args[0]).Contains("A1") && ((string)m.Args[0]).Contains("B1") && ((string)m.Args[0]).Contains("C1")));
		}
	}
}
