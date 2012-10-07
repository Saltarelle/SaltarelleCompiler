using System.Linq;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class InlineCodeMethodCompilerTests : MethodCompilerTestBase {
		private class DummyType : AbstractType {
			public override string Name { get { return "dummy"; } }
			public override bool? IsReferenceType { get { return true; } }
			public override TypeKind Kind { get { return TypeKind.Class; } }
			public override ITypeReference ToTypeReference() { throw new System.NotImplementedException(); }
		}

		[Test]
		public void TheTokenizerWorks() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("X{ab}{{y}z}}Y{{{c}}}T", new[] { "ab", "c" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] {
			                new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Text, "X"),
			                new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Parameter, index: 0),
			                new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Text, "{y}z}Y{"),
			                new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Parameter, index: 1),
			                new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Text, "}T"),
			            }));

			string msg = null;
			InlineCodeMethodCompiler.Tokenize("Something {abcd", new string[0], new string[0], s => msg = s);
			Assert.That(msg, Is.StringContaining("'}'"));

			Assert.That(InlineCodeMethodCompiler.Tokenize("X{}Y", new string[0], new string[0], s => Assert.Fail("Unexpected error " + s)), Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Text, "X{}Y") }));
		}

		[Test]
		public void TokenizerCanDetectTypeReferences() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{$System.Type}", new string[0], new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.TypeRef, "System.Type") }));
		}

		[Test]
		public void InvalidTypeNameIsReportedAsAnError() {
			string msg = null;
			InlineCodeMethodCompiler.Tokenize("{$Some[]-bad|type}", new string[0], new string[0], s => msg = s);
			Assert.That(msg, Is.StringContaining("Some[]-bad|type"));
		}

		[Test]
		public void TokenizerCanDetectThis() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{this}", new string[0], new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.This) }));
		}

		[Test]
		public void TokenizerCanDetectParameterNames() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{p1}{p2}", new[] { "p1", "p2" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Parameter, index: 0), new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Parameter, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectParameterNamePreceededByAtSign() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{p1}", new[] { "@p1" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.Parameter, index: 0) }));
		}

		[Test]
		public void TokenizerCanDetectLiteralStringParameterToUseAsIdentifier() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{@p1}{@p2}", new[] { "p1", "p2" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier, index: 0), new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.LiteralStringParameterToUseAsIdentifier, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectExpandedParamArrayParameter() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{*p1}{*p2}", new[] { "p1", "p2" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.ExpandedParamArrayParameter, index: 0), new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.ExpandedParamArrayParameter, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectTypeParameterNames() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{T1}{T2}", new string[0], new[] { "T1", "T2" }, s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.TypeParameter, index: 0), new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.TypeParameter, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectTypeParameterNamePreceededByAtSign() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{T1}", new string[0], new[] { "@T1" }, s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new InlineCodeMethodCompiler.InlineCodeToken(InlineCodeMethodCompiler.InlineCodeToken.TokenType.TypeParameter, index: 0) }));
		}

		[Test]
		public void UsingTextWithBracesInLiteralCodeWorks() {
			AssertCorrect(
@"public void F(int arg1, string arg2) {}
public void M() {
	// BEGIN
	F(45, ""test"");
	// END
}",
@"	{};
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{{ }}") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void UsingParametersFromLiteralCodeWorks() {
			AssertCorrect(
@"public void F(int arg1, string arg2) {}
public void M() {
	// BEGIN
	F(45, ""test"");
	// END
}",
@"	{ item1: 45, item2: 'test' };
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{{ item1: {arg1}, item2: {arg2} }}") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void UsingThisInInlineCodeForInstanceMethodWorks() {
			AssertCorrect(
@"class C1 { public void F() {} }
public void M() {
	var c = new C1();
	// BEGIN
	c.F();
	// END
}",
@"	[$c];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("[ {this} ]") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void UsingMethodTypeArgumentsFromLiteralCodeWorks() {
			AssertCorrect(
@"public void F<T1, T2>(T1 arg1, T2 arg2) {}
public void M() {
	// BEGIN
	F(45, ""test"");
	// END
}",
@"	{ item1: {sm_Int32}, item2: {sm_String} };
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{{ item1: {T1}, item2: {T2} }}") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void UsingTypeTypeArgumentsFromLiteralCodeWorks() {
			AssertCorrect(
@"class C1<T1> {
	public class C2<T2> {
		public void F() {
		}
	}
}

public void M() {
	var c = new C1<int>.C2<string>();
	// BEGIN
	c.F();
	// END
}",
@"	{ item1: {sm_Int32}, item2: {sm_String} };
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{{ item1: {T1}, item2: {T2} }}") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void UsingTypeReferenceFromLiteralCodeWorks() {
			AssertCorrect(
@"public void F() {}
public void M() {
	// BEGIN
	F();
	// END
}",
@"	[{sm_String}, {sm_Int32}];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("[ {$System.String}, {$System.Int32} ]") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void UsingLiteralStringParameterToUseAsIdentifierFromLiteralCodeWorks() {
			AssertCorrect(
@"public void F(string s) {}
public void M() {
	// BEGIN
	F(""X"");
	// END
}",
@"	invoke_X;
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("invoke_{@s}") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void InvokingMethodThatExpectsLiteralStringWithSomethingElseIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public void F(string myParameter) {}
	public void M() {
		string s = ""X"";
		// BEGIN
		F(s);
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("invoke_{@myParameter}") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.Contains("myParameter") && m.Contains("literal string")));
		}

		[Test]
		public void InvokingMethodWithExpandedParamArrayWorksForFunctionCalls() {
			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	'x' * 4();
	'x' * 4('y');
	'x' * 4('y', 'z');
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2}({*p3})") : MethodScriptSemantics.NormalMethod(m.Name) });

			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	'x' * 4(A);
	'x' * 4(A, 'y');
	'x' * 4(A, 'y', 'z');
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2}(A, {*p3})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void InvokingMethodWithExpandedParamArrayWorksForNewExpressions() {
			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	'x' * new 4();
	'x' * new 4('y');
	'x' * new 4('y', 'z');
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}* new {p2}({*p3})") : MethodScriptSemantics.NormalMethod(m.Name) });

			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	'x' * new 4(A);
	'x' * new 4(A, 'y');
	'x' * new 4(A, 'y', 'z');
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}* new {p2}(A, {*p3})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void InvokingMethodWithExpandedParamArrayWorksForArrayLiterals() {
			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	'x' * 4 + [];
	'x' * 4 + ['y'];
	'x' * 4 + ['y', 'z'];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2} + [{*p3}]") : MethodScriptSemantics.NormalMethod(m.Name) });

			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	'x' * 4 + [A];
	'x' * 4 + [A, 'y'];
	'x' * 4 + [A, 'y', 'z'];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2} + [A, {*p3}]") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void UsingExpandedParamArrayInOtherContextIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public void F(string p1, int p2, params string[] p3) {}
	public void M() {
		string[] args = null;
		// BEGIN
		F(""x"", 1, ""y"", ""z"");
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2} + {*p3}") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.Contains("can only be used")));
		}

		[Test]
		public void InlineCodeWithSyntaxErrorIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public void F(string p1, int p2, params string[] p3) {}
	public void M() {
		string[] args = null;
		// BEGIN
		F(""x"", 1, ""y"", ""z"");
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2}+") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.Contains("syntax error")));
		}

		[Test]
		public void InvokingMethodWithExpandedParamArrayInNonExpandedFormIsAnError() {
			var er = new MockErrorReporter(false);
			Compile(new[] {
@"class C {
	public void F(string p1, int p2, params string[] p3) {}
	public void M() {
		string[] args = null;
		// BEGIN
		F(""x"", 1, new[] { ""y"", ""z"" });
		// END
	}
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2}({*p3})") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.Contains("C.F") && m.Contains("expanded")));
		}

		[Test]
		public void ValidateReturnsNoErrosWhenCalledWithAValidString() {
			Compile("class C<T1> { public void F<T2>(string s, int a, params string[] p) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			Assert.That(InlineCodeMethodCompiler.ValidateLiteralCode(method, "{$System.Object}({T1}, {T2}, {@s}, {this}, {a}, {*p})", t => new DummyType()), Is.Empty);
		}

		[Test]
		public void ValidateReturnsAnErrorWhenThereIsAFormatStringError() {
			Compile("class C { public void F() {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{abc", t => new DummyType());
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("expected '}'")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenThisIsUsedForAStaticMethod() {
			Compile("class C { public static void F() {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{this}", t => new DummyType());
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("{this}") && e.Contains("static")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenAReferencedTypeCannotBeFound() {
			Compile("class C { public static void F() {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{$NonExisting.Type}()", t => SpecialType.UnknownType);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("NonExisting.Type")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenTheAtModifierIsUsedWithAnArgumentThatIsNotAString() {
			Compile("class C { public static void F(int myArg) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{@myArg}", t => new DummyType());
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("'@'") && e.Contains("myArg")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenTheStarModifierIsUsedWithAnArgumentThatIsNotAParamArray() {
			Compile("class C { public static void F(string[] myArg) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{*myArg}", t => new DummyType());
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("*") && e.Contains("myArg") && e.Contains("param array")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenThereIsASyntaxError() {
			Compile("class C { public static void F(string x, int y) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{x} + ", t => new DummyType());
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("syntax error")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenAnExpandedParamArrayIsUsedInAnInvalidContext() {
			Compile("class C { public static void F(string p1, int p2, params string[] p3) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{p1}*{p2} + {*p3}", t => new DummyType());
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("can only be used")));
		}

		[Test]
		public void InvokingInlineCodeMethodWithTypeParameterAsTypeArgumentWorks() {
			AssertCorrect(
@"public class C2<T1> {
	public void F<T2>() {}
}
public class C<T3> {
	public void M<T4>() {
		var c = new C2<T3>();
		// BEGIN
		c.F<T4>();
		// END
	}
}",
@"	sm_$T3._(sm_$T4);
", addSkeleton: false, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{T1}._({T2})") : MethodScriptSemantics.NormalMethod(m.Name) });
		}
	}
}
