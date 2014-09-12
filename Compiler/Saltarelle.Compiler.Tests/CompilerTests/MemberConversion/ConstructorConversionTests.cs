using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MemberConversion {
	[TestFixture]
	public class ConstructorConversionTests : CompilerTestBase {
		[Test]
		public void DefaultConstructorIsInsertedIfNoConstructorIsDefined() {
			Compile(new[] { "class C {}" });
			var cls = FindClass("C");
			Assert.That(cls.NamedConstructors, Is.Empty);
			Assert.That(cls.UnnamedConstructor, Is.Not.Null);
			Assert.That(cls.UnnamedConstructor.ParameterNames, Has.Count.EqualTo(0));
		}

		[Test]
		public void DefaultConstructorImplementedAsStaticMethodWorks() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.StaticMethod("X") };
			Compile(new[] { "class C { }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.X"), Is.Not.Null);
			Assert.That(FindNamedConstructor("C.X"), Is.Null);
		}

		[Test]
		public void DefaultConstructorIsNotInsertedIfOtherConstructorIsDefined() {
			var metadataImporter = new MockMetadataImporter() { GetConstructorSemantics = c => c.Parameters.Length == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("ctor$" + string.Join("$", c.Parameters.Select(p => p.Type.Name))) };
			Compile(new[] { "class C { C(int i) {} }" }, metadataImporter: metadataImporter);
			var cls = FindClass("C");
			Assert.That(cls.UnnamedConstructor, Is.Null);
			Assert.That(cls.NamedConstructors, Has.Count.EqualTo(1));
			Assert.That(cls.NamedConstructors[0].Name, Is.EqualTo("ctor$Int32"));
			Assert.That(cls.NamedConstructors[0].Definition, Is.Not.Null);
		}

		[Test]
		public void ConstructorsCanBeOverloadedWithDifferentImplementations() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ctor.ContainingType.SpecialType == SpecialType.System_Object ? ConstructorScriptSemantics.Unnamed(skipInInitializer: true) : (ctor.Parameters[0].Type.Name == "String" ? ConstructorScriptSemantics.Named("StringCtor") : ConstructorScriptSemantics.StaticMethod("IntCtor")) };
			Compile(new[] { "class C { C(int i) {} C(string s) {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").NamedConstructors, Has.Count.EqualTo(1));
			Assert.That(FindClass("C").StaticMethods, Has.Count.EqualTo(1));
			Assert.That(FindNamedConstructor("C.StringCtor"), Is.Not.Null);
			Assert.That(FindStaticMethod("C.IntCtor"), Is.Not.Null);
		}

		[Test]
		public void ConstructorImplementedAsStaticMethodGetsAddedToTheStaticMethodsCollectionAndNotTheConstructors() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.StaticMethod("X") };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindStaticMethod("C.X"), Is.Not.Null);
			Assert.That(FindNamedConstructor("C.X"), Is.Null);
		}

		[Test]
		public void ConstructorImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").UnnamedConstructor, Is.Null);
		}

		[Test]
		public void ConstructorImplementedAsInlineCodeDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.InlineCode("X") };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").UnnamedConstructor, Is.Null);
		}

		[Test]
		public void ConstructorImplementedAsJsonDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.Json(new ISymbol[0]) };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			Assert.That(FindClass("C").UnnamedConstructor, Is.Null);
		}

		[Test]
		public void StaticConstructorBodyGetsAddedLastInTheStaticInitStatements() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => { if (ctor.IsStatic) throw new InvalidOperationException(); else return ConstructorScriptSemantics.Unnamed(); } };
			Compile(new[] {
@"class C {
	static int x = 0;
	static C() {
		int z = 2;
	}
	static int y = 1;
}" }, metadataImporter: metadataImporter);

			var cctor = OutputFormatter.Format(FindClass("C").StaticInitStatements, allowIntermediates: true);
			Assert.That(cctor.Replace("\r\n", "\n"), Is.EqualTo(
@"$Init({sm_C}, '$x', 0);
$Init({sm_C}, '$y', 1);
var $z = 2;
".Replace("\r\n", "\n")));
		}

		[Test]
		public void StaticFieldsWithoutInitializersInGenericTypeAreInitializedToDefault() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => { if (ctor.IsStatic) throw new InvalidOperationException(); else return ConstructorScriptSemantics.Unnamed(); } };
			Compile(new[] {
@"class C<T> {
	static T x;
	static int y;
	static string z;
}" }, metadataImporter: metadataImporter);

			var cctor = OutputFormatter.Format(FindClass("C").StaticInitStatements, allowIntermediates: true);
			Assert.That(cctor.Replace("\r\n", "\n"), Is.EqualTo(
@"$Init(sm_$InstantiateGenericType({C}, $T), '$x', $Default($T));
$Init(sm_$InstantiateGenericType({C}, $T), '$y', $Default({def_Int32}));
$Init(sm_$InstantiateGenericType({C}, $T), '$z', null);
".Replace("\r\n", "\n")));
		}

		[Test]
		public void StaticFieldsWithInitializersInGenericTypeAreInitialized() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => { if (ctor.IsStatic) throw new InvalidOperationException(); else return ConstructorScriptSemantics.Unnamed(); } };
			Compile(new[] {
@"class C<T> {
	static T x = default(T);
	static int y = 42;
	static string z = ""X"";
}" }, metadataImporter: metadataImporter);

			var cctor = OutputFormatter.Format(FindClass("C").StaticInitStatements, allowIntermediates: true);
			Assert.That(cctor.Replace("\r\n", "\n"), Is.EqualTo(
@"$Init(sm_$InstantiateGenericType({C}, $T), '$x', $Default($T));
$Init(sm_$InstantiateGenericType({C}, $T), '$y', 42);
$Init(sm_$InstantiateGenericType({C}, $T), '$z', 'X');
".Replace("\r\n", "\n")));
		}
	}
}
