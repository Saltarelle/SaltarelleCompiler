using System.Linq;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporterTests {
	[TestFixture]
	public class TestingTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void IsTestFixtureReturnsTrueForTypesDecoratedWithTestFixtureAttribute() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {}

public class C2 {}
");
			Assert.That(Metadata.IsTestFixture(AllTypes["C1"]), Is.True);
			Assert.That(Metadata.IsTestFixture(AllTypes["C2"]), Is.False);
		}

		[Test]
		public void TestFixtureClassCannotDeclareMethodWithScriptNameRunTests() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {
	public void RunTests() {
	}
}
", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestFixtureAttribute") && m.Contains("runTests")));
		}

		[Test]
		public void TestAttributeWorks() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {
	[Test]
	public void M1() {}

	[Test(""M2 description"")]
	public void M2() {}

	[Test(ExpectedAssertionCount = 2)]
	public void M3() {}

	[Test(""M4 description"", ExpectedAssertionCount = -1)]
	public void M4() {}

	[Test(Category = ""My category"")]
	public void M5() {}

	public void M6() {}
}
");
			var m1 = Metadata.GetTestData(FindMethods("C1.M1").Single().Item1);
			Assert.That(m1, Is.Not.Null);
			Assert.That(m1.IsAsync, Is.False);
			Assert.That(m1.Description, Is.EqualTo("M1"));
			Assert.That(m1.Category, Is.Null);
			Assert.That(m1.ExpectedAssertionCount, Is.Null);

			var m2 = Metadata.GetTestData(FindMethods("C1.M2").Single().Item1);
			Assert.That(m2, Is.Not.Null);
			Assert.That(m2.IsAsync, Is.False);
			Assert.That(m2.Description, Is.EqualTo("M2 description"));
			Assert.That(m2.Category, Is.Null);
			Assert.That(m2.ExpectedAssertionCount, Is.Null);

			var m3 = Metadata.GetTestData(FindMethods("C1.M3").Single().Item1);
			Assert.That(m3, Is.Not.Null);
			Assert.That(m3.IsAsync, Is.False);
			Assert.That(m3.Description, Is.EqualTo("M3"));
			Assert.That(m3.Category, Is.Null);
			Assert.That(m3.ExpectedAssertionCount, Is.EqualTo(2));

			var m4 = Metadata.GetTestData(FindMethods("C1.M4").Single().Item1);
			Assert.That(m4, Is.Not.Null);
			Assert.That(m4.IsAsync, Is.False);
			Assert.That(m4.Description, Is.EqualTo("M4 description"));
			Assert.That(m4.Category, Is.Null);
			Assert.That(m4.ExpectedAssertionCount, Is.Null);

			var m5 = Metadata.GetTestData(FindMethods("C1.M5").Single().Item1);
			Assert.That(m5, Is.Not.Null);
			Assert.That(m5.IsAsync, Is.False);
			Assert.That(m5.Description, Is.EqualTo("M5"));
			Assert.That(m5.Category, Is.EqualTo("My category"));
			Assert.That(m5.ExpectedAssertionCount, Is.Null);

			var m6 = Metadata.GetTestData(FindMethods("C1.M6").Single().Item1);
			Assert.That(m6, Is.Null);
		}

		[Test]
		public void AsyncTestAttributeWorks() {
			Prepare(
@"using System.Testing;

[TestFixture]
public class C1 {
	[AsyncTest]
	public void M1() {}

	[AsyncTest(""M2 description"")]
	public void M2() {}

	[AsyncTest(ExpectedAssertionCount = 2)]
	public void M3() {}

	[AsyncTest(""M4 description"", ExpectedAssertionCount = null)]
	public void M4() {}

	[AsyncTest(Category = ""My category"")]
	public void M5() {}

	public void M6() {}
}
");
			var m1 = Metadata.GetTestData(FindMethods("C1.M1").Single().Item1);
			Assert.That(m1, Is.Not.Null);
			Assert.That(m1.IsAsync, Is.True);
			Assert.That(m1.Description, Is.EqualTo("M1"));
			Assert.That(m1.Category, Is.Null);
			Assert.That(m1.ExpectedAssertionCount, Is.Null);

			var m2 = Metadata.GetTestData(FindMethods("C1.M2").Single().Item1);
			Assert.That(m2, Is.Not.Null);
			Assert.That(m2.IsAsync, Is.True);
			Assert.That(m2.Description, Is.EqualTo("M2 description"));
			Assert.That(m2.Category, Is.Null);
			Assert.That(m2.ExpectedAssertionCount, Is.Null);

			var m3 = Metadata.GetTestData(FindMethods("C1.M3").Single().Item1);
			Assert.That(m3, Is.Not.Null);
			Assert.That(m3.IsAsync, Is.True);
			Assert.That(m3.Description, Is.EqualTo("M3"));
			Assert.That(m3.Category, Is.Null);
			Assert.That(m3.ExpectedAssertionCount, Is.EqualTo(2));

			var m4 = Metadata.GetTestData(FindMethods("C1.M4").Single().Item1);
			Assert.That(m4, Is.Not.Null);
			Assert.That(m4.IsAsync, Is.True);
			Assert.That(m4.Description, Is.EqualTo("M4 description"));
			Assert.That(m4.Category, Is.Null);
			Assert.That(m4.ExpectedAssertionCount, Is.Null);

			var m5 = Metadata.GetTestData(FindMethods("C1.M5").Single().Item1);
			Assert.That(m5, Is.Not.Null);
			Assert.That(m5.IsAsync, Is.True);
			Assert.That(m5.Description, Is.EqualTo("M5"));
			Assert.That(m5.Category, Is.EqualTo("My category"));
			Assert.That(m5.ExpectedAssertionCount, Is.Null);

			var m6 = Metadata.GetTestData(FindMethods("C1.M6").Single().Item1);
			Assert.That(m6, Is.Null);
		}

		[Test]
		public void MethodWithTestOrAsyncTestAttributeMustBeAPublicNonGenericParameterInstanceMethodReturningVoid() {
			var defs = new[] { "private void M()", "public int M()", "public void M<T>()", "public void M(int x)", "public static void M()" };

			foreach (var def in defs) {
				foreach (var attr in new[] { "Test", "AsyncTest" }) {
					Prepare("using System.Testing; [TestFixture] public class C1 { [" + attr + "] " + def + " {} }", expectErrors: true);
					Assert.That(AllErrors, Has.Count.EqualTo(1));
					Assert.That(AllErrors[0].Code, Is.EqualTo(7020));
				}
			}
		}

		[Test]
		public void TestAttributeAndAsyncTestAttributeOnTheSameMethodIsAnError() {
			Prepare("using System.Testing; [TestFixture] public class C1 { [Test][AsyncTest] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestAttribute") && m.Contains("AsyncTestAttribute")));
		}

		[Test]
		public void TestOrAsyncTestAttributeCannotBeSpecifiedOnTypeThatIsNotATestFixture() {
			Prepare("using System.Testing; public class C1 { [Test] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestAttribute") && m.Contains("TestFixtureAttribute")));

			Prepare("using System.Testing; public class C1 { [AsyncTest] public void M() {} }", expectErrors: true);
			Assert.That(AllErrorTexts, Has.Count.EqualTo(1));
			Assert.That(AllErrorTexts.Any(m => m.Contains("TestAttribute") && m.Contains("TestFixtureAttribute")));
		}
	}
}
