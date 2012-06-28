using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.ScriptSharpMetadataImporter {
	[TestFixture]
	public class ConstructorTests : ScriptSharpMetadataImporterTestBase {
		[Test]
		public void SingleConstructorIsUnnamed() {
			Prepare(@"public class C { public C() {} }");

			var impl = FindConstructor("C", 0);
			Assert.That(impl.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(impl.GenerateCode, Is.True);
		}

		[Test]
		public void DefaultConstructorIsUnnamed() {
			Prepare(@"public class C {}");

			var impl = FindConstructor("C", 0);
			Assert.That(impl.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(impl.GenerateCode, Is.True);
		}

		[Test]
		public void AdditionalConstructorsAreNamed() {
			Prepare(@"public class C { public C() {} public C(int i) {} public C(int i, int j) {} }");

			var c1 = FindConstructor("C", 0);
			Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c1.GenerateCode, Is.True);

			var c2 = FindConstructor("C", 1);
			Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c2.Name, Is.EqualTo("$ctor1"));
			Assert.That(c2.GenerateCode, Is.True);

			var c3 = FindConstructor("C", 2);
			Assert.That(c3.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c3.Name, Is.EqualTo("$ctor2"));
			Assert.That(c3.GenerateCode, Is.True);
		}

		[Test]
		public void ScriptNameAttributeCanSpecifyTheNameOfAConstructor() {
			Prepare(@"using System.Runtime.CompilerServices; public class C { [ScriptName(""Renamed"")] public C() {} public C(int i) {} }");

			var c1 = FindConstructor("C", 0);
			Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c1.Name, Is.EqualTo("Renamed"));
			Assert.That(c1.GenerateCode, Is.True);

			var c2 = FindConstructor("C", 1);
			Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c2.GenerateCode, Is.True);
		}

		[Test]
		public void BlankScriptNameAttributeForAConstructorDesignatesTheDefaultConstructor() {
			Prepare(@"using System.Runtime.CompilerServices; public class C { public C() {} [ScriptName("""")] public C(int i) {} }");

			var c1 = FindConstructor("C", 0);
			Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c1.Name, Is.EqualTo("$ctor1"));
			Assert.That(c1.GenerateCode, Is.True);

			var c2 = FindConstructor("C", 1);
			Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c2.GenerateCode, Is.True);
		}

		[Test]
		public void AlternateSignatureWorksWhenTheMainMemberIsAnUnnamedConstructor() {
			Prepare(@"using System.Runtime.CompilerServices; public class C { [AlternateSignature] public C() {} [AlternateSignature] public C(int i) {} public C(int i, int j) {} }");

			var c1 = FindConstructor("C", 0);
			Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c1.GenerateCode, Is.False);

			var c2 = FindConstructor("C", 1);
			Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c2.GenerateCode, Is.False);

			var c3 = FindConstructor("C", 2);
			Assert.That(c3.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c3.GenerateCode, Is.True);
		}

		[Test]
		public void AlternateSignatureWorksWhenTheMainMemberIsANamedConstructor() {
			Prepare(@"using System.Runtime.CompilerServices; public class C { [AlternateSignature] public C() {} [AlternateSignature] public C(int i) {} [ScriptName(""Renamed"")] public C(int i, int j) {} }");

			var c1 = FindConstructor("C", 0);
			Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c1.Name, Is.EqualTo("Renamed"));
			Assert.That(c1.GenerateCode, Is.False);

			var c2 = FindConstructor("C", 1);
			Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c2.Name, Is.EqualTo("Renamed"));
			Assert.That(c2.GenerateCode, Is.False);

			var c3 = FindConstructor("C", 2);
			Assert.That(c3.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c3.Name, Is.EqualTo("Renamed"));
			Assert.That(c3.GenerateCode, Is.True);
		}

		[Test]
		public void InlineCodeAttributeCanBeSpecifiedForConstructor() {
			Prepare(@"using System.Runtime.CompilerServices; public class C { [InlineCode(""$X$"")] public C() {} public C(int i) {} }");

			var c1 = FindConstructor("C", 0);
			Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.InlineCode));
			Assert.That(c1.LiteralCode, Is.EqualTo("$X$"));
			Assert.That(c1.GenerateCode, Is.False);

			var c2 = FindConstructor("C", 1);
			Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c2.GenerateCode, Is.True);
		}

		[Test]
		public void InlineCodeCanUseArgumentsAndTypeArguments() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1<T1> {
	class C2<T2> {
		[InlineCode(""Some.[].Strange{{ }}'thing' {T1} {T2} {x} {y}"")]
		public C(int x, string y) {}
	}
}");

			var c = FindConstructor("C1`1+C2`1", 2);
			Assert.That(c.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.InlineCode));
			Assert.That(c.LiteralCode, Is.EqualTo("Some.[].Strange{{ }}'thing' {T1} {T2} {x} {y}"));
		}

		[Test]
		public void InlineCodeAttributeWithUnknownArgumentsIsAnError() {
			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""{this}"")] public static void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.SomeMethod") && AllErrors[0].Contains("inline code") && AllErrors[0].Contains("{this}"));

			Prepare(@"using System.Runtime.CompilerServices; class C1 { [InlineCode(""{x}"")] public void SomeMethod() {} }", expectErrors: true);
			Assert.That(AllErrors, Has.Count.EqualTo(1));
			Assert.That(AllErrors[0].Contains("C1.SomeMethod") && AllErrors[0].Contains("inline code") && AllErrors[0].Contains("{x}"));
		}

		[Test]
		public void NonScriptableAttributeCausesConstructorToNotBeUsableFromScript() {
			Prepare(@"using System.Runtime.CompilerServices; public class C { [NonScriptable] public C() {} }");

			var impl = FindConstructor("C", 0);
			Assert.That(impl.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void NonPublicNamedConstructorNamessAreMinimized() {
			Prepare(@"class C { public C() {} public C(int i) {} public C(int i, int j) {} }");

			var c1 = FindConstructor("C", 0);
			Assert.That(c1.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
			Assert.That(c1.GenerateCode, Is.True);

			var c2 = FindConstructor("C", 1);
			Assert.That(c2.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c2.Name, Is.EqualTo("$0"));
			Assert.That(c2.GenerateCode, Is.True);

			var c3 = FindConstructor("C", 2);
			Assert.That(c3.Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.NamedConstructor));
			Assert.That(c3.Name, Is.EqualTo("$1"));
			Assert.That(c3.GenerateCode, Is.True);
		}

		[Test]
		public void DelegateTypeConstructorCannotBeUsed() {
			Prepare("public delegate void Del();");
			var del = AllTypes["Del"];

			Assert.That(del.GetConstructors().Select(c => Metadata.GetConstructorSemantics(c).Type), Has.All.EqualTo(ConstructorScriptSemantics.ImplType.NotUsableFromScript));
		}

		[Test]
		public void ExpandParamsAttributeCausesConstructorToUseExpandParamsOption() {
			Prepare(
@"using System.Runtime.CompilerServices;

class C1 {
	public C1(int a, int b, params int[] c) {}
	[ExpandParams]
	public C1(int a, params int[] c) {}
}");

			Assert.That(FindConstructor("C1", 3).ExpandParams, Is.False);
			Assert.That(FindConstructor("C1", 2).ExpandParams, Is.True);
		}

		[Test]
		public void ExpandParamsAttributeCanOnlyBeAppliedToConstructorWithParamArray() {
			Prepare(
@"using System.Runtime.CompilerServices;
class C1 {
	[ExpandParams]
	public C1(int a, int b, int[] c) {}
}", expectErrors: true);
			Assert.That(AllErrors.Count, Is.EqualTo(1));
			Assert.That(AllErrors.Any(m => m.Contains("C1") && m.Contains("constructor") && m.Contains("params") && m.Contains("ExpandParamsAttribute")));
		}

		[Test]
		public void StaticConstructorDoesNotCauseOtherConstructorToBeNamed() {
			Prepare(
@"public class C1 {
	static C1() {
	}
}");
		
			Assert.That(FindConstructor("C1", 0).Type, Is.EqualTo(ConstructorScriptSemantics.ImplType.UnnamedConstructor));
		}
	}
}
