using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.MetadataImporter;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpOOPEmulatorTests {
	[TestFixture]
	public class StaticInitializationOrderTests : ScriptSharpOOPEmulatorTestBase {
		private JsFunctionDefinitionExpression CreateFunction(params ITypeDefinition[] referencedTypes) {
			return JsExpression.FunctionDefinition(new string[0], new JsExpressionStatement(JsExpression.ArrayLiteral(referencedTypes.Select(t => new JsTypeReferenceExpression(t)))));
		}

		private string[] SplitLines(string s) {
			return s.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
		}

		[Test]
		public void StaticInitStatementsAreSortedByAllReferencesInTypes() {
			var c1 = CreateMockTypeDefinition("C1");
			var c2 = CreateMockTypeDefinition("C2");
			var c3 = CreateMockTypeDefinition("C3");
			var c4 = CreateMockTypeDefinition("C4");
			var c5 = CreateMockTypeDefinition("C5");
			var c6 = CreateMockTypeDefinition("C6");

			var lines = SplitLines(Process(
				new JsClass(c1, JsClass.ClassTypeEnum.Class, null, null, null) {
					UnnamedConstructor = CreateFunction(),
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x1")) }
				},
				new JsClass(c2, JsClass.ClassTypeEnum.Class, null, null, null) {
					UnnamedConstructor = CreateFunction(c1),
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x2")) }
				},
				new JsClass(c3, JsClass.ClassTypeEnum.Class, null, null, null) {
					NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction(c2)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x3")) }
				},
				new JsClass(c4, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticMethods = { new JsMethod(CreateMockMethod("M"), "m", null, CreateFunction(c3)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x4")) }
				},
				new JsClass(c5, JsClass.ClassTypeEnum.Class, null, null, null) {
					InstanceMethods = { new JsMethod(CreateMockMethod("M"), "m", null, CreateFunction(c4)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x5")) }
				},
				new JsClass(c6, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Add(JsExpression.Identifier("x6"), new JsTypeReferenceExpression(c5))) }
				})).Where(l => l.StartsWith("x"));

			Assert.That(lines, Is.EqualTo(new[] { "x6 + {C5};", "x5;", "x4;", "x3;", "x2;", "x1;" }));
		}

		[Test]
		public void StaticMethodsOnlyAreUsedAsTieBreakersWhenCyclicDependenciesOccur() {
			var c1 = CreateMockTypeDefinition("C1");
			var c2 = CreateMockTypeDefinition("C2");
			var c3 = CreateMockTypeDefinition("C3");
			var c4 = CreateMockTypeDefinition("C4");
			var c5 = CreateMockTypeDefinition("C5");

			var lines = SplitLines(Process(
				new JsClass(c1, JsClass.ClassTypeEnum.Class, null, null, null) {
					UnnamedConstructor = CreateFunction(),
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x1")) }
				},
				new JsClass(c2, JsClass.ClassTypeEnum.Class, null, null, null) {
					UnnamedConstructor = CreateFunction(c4),
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x2")) }
				},
				new JsClass(c3, JsClass.ClassTypeEnum.Class, null, null, null) {
					NamedConstructors = { new JsNamedConstructor("ctor1", CreateFunction(c2)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Add(JsExpression.Identifier("x3"), new JsTypeReferenceExpression(c2))) }
				},
				new JsClass(c4, JsClass.ClassTypeEnum.Class, null, null, null) {
					InstanceMethods = { new JsMethod(CreateMockMethod("M1"), "m1", null, CreateFunction(c3)) },
					StaticMethods = { new JsMethod(CreateMockMethod("M2"), "m2", null, CreateFunction(c3)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x4")) }
				},
				new JsClass(c5, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticMethods = { new JsMethod(CreateMockMethod("M"), "m", null, CreateFunction(c3)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x5")) }
				})).Where(l => l.StartsWith("x"));

			Assert.That(lines, Is.EqualTo(new[] { "x5;", "x4;", "x3 + {C2};", "x2;", "x1;" }));
		}

		[Test]
		public void StaticInitStatementsOnlyAreUsedAsATieBreakerWhenCyclicDependenciesInStaticMethodsOccur() {
			var c1 = CreateMockTypeDefinition("C1");
			var c2 = CreateMockTypeDefinition("C2");
			var c3 = CreateMockTypeDefinition("C3");
			var c4 = CreateMockTypeDefinition("C4");
			var c5 = CreateMockTypeDefinition("C5");

			var lines = SplitLines(Process(
				new JsClass(c1, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x1")) }
				},
				new JsClass(c2, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticMethods = { new JsMethod(CreateMockMethod("M"), "m", null, CreateFunction(c4)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x2")) }
				},
				new JsClass(c3, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticMethods = { new JsMethod(CreateMockMethod("M"), "m", null, CreateFunction(c2)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Add(JsExpression.Identifier("x3"), new JsTypeReferenceExpression(c2))) }
				},
				new JsClass(c4, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticMethods = { new JsMethod(CreateMockMethod("M"), "m", null, CreateFunction(c3)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Add(JsExpression.Identifier("x4"), new JsTypeReferenceExpression(c3))) }
				},
				new JsClass(c5, JsClass.ClassTypeEnum.Class, null, null, null) {
					StaticMethods = { new JsMethod(CreateMockMethod("M"), "m", null, CreateFunction(c3)) },
					StaticInitStatements = { new JsExpressionStatement(JsExpression.Identifier("x5")) }
				})).Where(l => l.StartsWith("x"));

			Assert.That(lines, Is.EqualTo(new[] { "x5;", "x4 + {C3};", "x3 + {C2};", "x2;", "x1;" }));
		}
	}
}
