using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using Mono.CSharp;
using NUnit.Framework;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.ScriptSemantics;
using Is = NUnit.Framework.Is;
using Token = Saltarelle.Compiler.Compiler.InlineCodeMethodCompiler.InlineCodeToken;
using TokenType = Saltarelle.Compiler.Compiler.InlineCodeMethodCompiler.InlineCodeToken.TokenType;

namespace Saltarelle.Compiler.Tests.Compiler.MethodCompilationTests {
	[TestFixture]
	public class InlineCodeMethodCompilerTests : MethodCompilerTestBase {
		[Test]
		public void TheTokenizerWorks() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("X{ab}{{y}z}}Y{{{c}}}T", new[] { "ab", "c" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] {
			                new Token(TokenType.Text, "X"),
			                new Token(TokenType.Parameter, index: 0),
			                new Token(TokenType.Text, "{y}z}Y{"),
			                new Token(TokenType.Parameter, index: 1),
			                new Token(TokenType.Text, "}T"),
			            }));

			string msg = null;
			InlineCodeMethodCompiler.Tokenize("Something {abcd", new string[0], new string[0], s => msg = s);
			Assert.That(msg, Is.StringContaining("'}'"));

			Assert.That(InlineCodeMethodCompiler.Tokenize("X{}Y", new string[0], new string[0], s => Assert.Fail("Unexpected error " + s)), Is.EqualTo(new[] { new Token(TokenType.Text, "X{}Y") }));
		}

		[Test]
		public void TokenizerCanDetectTypeReferences() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{$System.Type}", new string[0], new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.TypeRef, "System.Type") }));
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
			            Is.EqualTo(new[] { new Token(TokenType.This) }));
		}

		[Test]
		public void TokenizerCanDetectParameterNames() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{p1}{p2}", new[] { "p1", "p2" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.Parameter, index: 0), new Token(TokenType.Parameter, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectParameterNamePreceededByAtSign() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{p1}", new[] { "@p1" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.Parameter, index: 0) }));
		}

		[Test]
		public void TokenizerCanDetectLiteralStringParameterToUseAsIdentifier() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{@p1}{@p2}", new[] { "p1", "p2" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.LiteralStringParameterToUseAsIdentifier, index: 0), new Token(TokenType.LiteralStringParameterToUseAsIdentifier, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectExpandedParamArrayParameter() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{*p1}{*p2}", new[] { "p1", "p2" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.ExpandedParamArrayParameter, index: 0), new Token(TokenType.ExpandedParamArrayParameter, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectExpandedParamArrayWithCommaBeforeParameter() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{,p1}{,p2}", new[] { "p1", "p2" }, new string[0], s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.ExpandedParamArrayParameterWithCommaBefore, index: 0), new Token(TokenType.ExpandedParamArrayParameterWithCommaBefore, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectTypeParameterNames() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{T1}{T2}", new string[0], new[] { "T1", "T2" }, s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.TypeParameter, index: 0), new Token(TokenType.TypeParameter, index: 1) }));
		}

		[Test]
		public void TokenizerCanDetectTypeParameterNamePreceededByAtSign() {
			Assert.That(InlineCodeMethodCompiler.Tokenize("{T1}", new string[0], new[] { "@T1" }, s => Assert.Fail("Unexpected error " + s)),
			            Is.EqualTo(new[] { new Token(TokenType.TypeParameter, index: 0) }));
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
@"	{ {} };
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{{ {} }}") : MethodScriptSemantics.NormalMethod(m.Name) });
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
@"	[ item1: 45, item2: 'test' ];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("[ item1: {arg1}, item2: {arg2} ]") : MethodScriptSemantics.NormalMethod(m.Name) });
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
@"	[ $c ];
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
@"	[ item1: {Int32}, item2: {String} ];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("[ item1: {T1}, item2: {T2} ]") : MethodScriptSemantics.NormalMethod(m.Name) });
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
@"	[ item1: {Int32}, item2: {String} ];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("[ item1: {T1}, item2: {T2} ]") : MethodScriptSemantics.NormalMethod(m.Name) });
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
@"	[ {String}, {Int32} ];
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
		public void InvokingMethodWithExpandedParamArrayWorks() {
			var er = new MockErrorReporter(false);
			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	'x'*4();
	'x'*4('y');
	'x'*4('y', 'z');
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("{p1}*{p2}({*p3})") : MethodScriptSemantics.NormalMethod(m.Name) });
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
		public void InvokingMethodWithExpandedParamArrayWithCommaBeforeWorks() {
			var er = new MockErrorReporter(false);
			AssertCorrect(
@"public void F(string p1, int p2, params string[] p3) {}
public void M() {
	// BEGIN
	F(""x"", 4);
	F(""x"", 4, ""y"");
	F(""x"", 4, ""y"", ""z"");
	// END
}",
@"	[ 'x', 4 ];
	[ 'x', 4, 'y' ];
	[ 'x', 4, 'y', 'z' ];
", metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("[ {p1}, {p2}{,p3} ]") : MethodScriptSemantics.NormalMethod(m.Name) });
		}

		[Test]
		public void InvokingMethodWithExpandedParamWithCommaBeforeArrayInNonExpandedFormIsAnErro() {
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
}" }, metadataImporter: new MockMetadataImporter { GetMethodSemantics = m => m.Name == "F" ? MethodScriptSemantics.InlineCode("[ {p1}, {p2} {,p3} ]") : MethodScriptSemantics.NormalMethod(m.Name) }, errorReporter: er);

			Assert.That(er.AllMessagesText.Count, Is.EqualTo(1));
			Assert.That(er.AllMessagesText.Any(m => m.Contains("C.F") && m.Contains("expanded")));
		}

		[Test]
		public void ValidateReturnsNoErrosWhenCalledWithAValidString() {
			Compile("class C<T1> { public void F<T2>(string s, int a, params string[] p) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			Assert.That(InlineCodeMethodCompiler.ValidateLiteralCode(method, "{$System.Object}({T1}, {T2}, @s, {this}, {a}, {*p}", t => true), Is.Empty);
		}

		[Test]
		public void ValidateReturnsErrorWhenThereIsASyntaxError() {
			Compile("class C { public void F() {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{abc", t => true);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("expected '}'")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenThisIsUsedForAStaticMethod() {
			Compile("class C { public static void F() {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{this}", t => true);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("{this}") && e.Contains("static")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenAReferencedTypeCannotBeFound() {
			Compile("class C { public static void F() {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{$NonExisting.Type}()", t => false);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("NonExisting.Type")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenTheAtModifierIsUsedWithAnArgumentThatIsNotAString() {
			Compile("class C { public static void F(int myArg) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{@myArg}", t => true);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("'@'") && e.Contains("myArg")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenTheStarModifierIsUsedWithAnArgumentThatIsNotAParamArray() {
			Compile("class C { public static void F(string[] myArg) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{*myArg}", t => true);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("'*'") && e.Contains("myArg")));
		}

		[Test]
		public void ValidateReturnsAnErrorWhenTheCommaModifierIsUsedWithAnArgumentThatIsNotAParamArray() {
			Compile("class C { public static void F(string[] myArg) {} }");
			var method = FindClass("C").CSharpTypeDefinition.Methods.Single(m => m.Name == "F");

			var result = InlineCodeMethodCompiler.ValidateLiteralCode(method, "{,myArg}", t => true);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result.Any(e => e.Contains("','") && e.Contains("myArg")));
		}
	}
}
