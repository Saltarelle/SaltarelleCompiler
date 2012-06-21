using FluentAssertions;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.Compiler.MemberConversionTests {
    [TestFixture]
    public class FieldConversionTests : CompilerTestBase {
        [Test]
        public void InstanceFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldSemantics = f => FieldScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public int SomeField; }" }, namingConvention: namingConvention);
            FindInstanceFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void StaticFieldsAreCorrectlyImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldSemantics = f => FieldScriptSemantics.Field("$SomeProp") };
            Compile(new[] { "class C { public static int SomeField; }" }, namingConvention: namingConvention);
            FindStaticFieldInitializer("C.$SomeProp").Should().NotBeNull();
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void FieldsThatAreNotUsableFromScriptAreNotImported() {
            var namingConvention = new MockNamingConventionResolver { GetFieldSemantics = f => FieldScriptSemantics.NotUsableFromScript() };
            Compile(new[] { "class C { public int SomeField; }" }, namingConvention: namingConvention);
            FindClass("C").UnnamedConstructor.Body.Statements.Should().BeEmpty();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

        [Test]
        public void ImportingMultipleFieldsInTheSameDeclarationWorks() {
            var namingConvention = new MockNamingConventionResolver { GetFieldSemantics = f => FieldScriptSemantics.Field("$" + f.Name) };
            Compile(new[] { "class C { public int Field1, Field2; }" }, namingConvention: namingConvention);
            FindInstanceFieldInitializer("C.$Field1").Should().NotBeNull();
            FindInstanceFieldInitializer("C.$Field2").Should().NotBeNull();
            FindClass("C").StaticInitStatements.Should().BeEmpty();
            FindClass("C").InstanceMethods.Should().BeEmpty();
            FindClass("C").StaticMethods.Should().BeEmpty();
        }

		[Test]
		public void DefaultValuesForFieldsAreCorrect() {
			Compile(new[] {
@"class X {}

class C<T1, T2> where T1 : class {
	public static bool f1;
	public static byte f2;
	public static sbyte f3;
	public static char f4;
	public static short f5;
	public static ushort f6;
	public static int f7;
	public static uint f8;
	public static long f9;
	public static ulong f10;
	public static decimal f11;
	public static float f12;
	public static double f13;
	public static string f14;
	public static object f15;
	public static X f16;
	public static C<int, double> f17;
	public static int? f18;
	public static T1 f19;
	public static T2 f20;
}" });

			Assert.That(FindStaticFieldInitializer("C.$f1"),  Is.EqualTo("false"));
			Assert.That(FindStaticFieldInitializer("C.$f2"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f3"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f4"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f5"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f6"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f7"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f8"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f9"),  Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f10"), Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f11"), Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f12"), Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f13"), Is.EqualTo("0"));
			Assert.That(FindStaticFieldInitializer("C.$f14"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$f15"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$f16"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$f17"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$f18"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$f19"), Is.EqualTo("null"));
			Assert.That(FindStaticFieldInitializer("C.$f20"), Is.EqualTo("$Default($T2)"));
		}
    }
}
