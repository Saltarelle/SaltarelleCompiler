using System;
using System.Linq;
using FluentAssertions;
using ICSharpCode.NRefactory.TypeSystem;
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
			cls.NamedConstructors.Should().BeEmpty();
			cls.UnnamedConstructor.Should().NotBeNull();
			cls.UnnamedConstructor.ParameterNames.Should().HaveCount(0);
		}

		[Test]
		public void DefaultConstructorImplementedAsStaticMethodWorks() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.StaticMethod("X") };
			Compile(new[] { "class C { }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.X").Should().NotBeNull();
			FindNamedConstructor("C.X").Should().BeNull();
		}

		[Test]
		public void DefaultConstructorIsNotInsertedIfOtherConstructorIsDefined() {
			var metadataImporter = new MockMetadataImporter() { GetConstructorSemantics = c => c.Parameters.Count == 0 ? ConstructorScriptSemantics.Unnamed() : ConstructorScriptSemantics.Named("ctor$" + string.Join("$", c.Parameters.Select(p => p.Type.Name))) };
			Compile(new[] { "class C { C(int i) {} }" }, metadataImporter: metadataImporter);
			var cls = FindClass("C");
			cls.UnnamedConstructor.Should().BeNull();
			cls.NamedConstructors.Should().HaveCount(1);
			cls.NamedConstructors[0].Name.Should().Be("ctor$Int32");
			cls.NamedConstructors[0].Definition.Should().NotBeNull();
		}

		[Test]
		public void ConstructorsCanBeOverloadedWithDifferentImplementations() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ctor.DeclaringType.IsKnownType(KnownTypeCode.Object) ? ConstructorScriptSemantics.Unnamed(skipInInitializer: true) : (ctor.Parameters[0].Type.Name == "String" ? ConstructorScriptSemantics.Named("StringCtor") : ConstructorScriptSemantics.StaticMethod("IntCtor")) };
			Compile(new[] { "class C { C(int i) {} C(string s) {} }" }, metadataImporter: metadataImporter);
			FindClass("C").NamedConstructors.Should().HaveCount(1);
			FindClass("C").StaticMethods.Should().HaveCount(1);
			FindNamedConstructor("C.StringCtor").Should().NotBeNull();
			FindStaticMethod("C.IntCtor").Should().NotBeNull();
		}

		[Test]
		public void ConstructorImplementedAsStaticMethodGetsAddedToTheStaticMethodsCollectionAndNotTheConstructors() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.StaticMethod("X") };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			FindStaticMethod("C.X").Should().NotBeNull();
			FindNamedConstructor("C.X").Should().BeNull();
		}

		[Test]
		public void ConstructorImplementedAsNotUsableFromScriptDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.NotUsableFromScript() };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").UnnamedConstructor.Should().BeNull();
		}

		[Test]
		public void ConstructorImplementedAsInlineCodeDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.InlineCode("X") };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").UnnamedConstructor.Should().BeNull();
		}

		[Test]
		public void ConstructorImplementedAsJsonDoesNotAppearOnTheType() {
			var metadataImporter = new MockMetadataImporter { GetConstructorSemantics = ctor => ConstructorScriptSemantics.Json(new IMember[0]) };
			Compile(new[] { "class C { public C() {} }" }, metadataImporter: metadataImporter);
			FindClass("C").UnnamedConstructor.Should().BeNull();
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
			cctor.Replace("\r\n", "\n").Should().Be(
@"$Init({sm_C}, '$x', 0);
$Init({sm_C}, '$y', 1);
var $z = 2;
".Replace("\r\n", "\n"));
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
			cctor.Replace("\r\n", "\n").Should().Be(
@"$Init(sm_$InstantiateGenericType({C}, $T), '$x', $Default($T));
$Init(sm_$InstantiateGenericType({C}, $T), '$y', $Default({def_Int32}));
$Init(sm_$InstantiateGenericType({C}, $T), '$z', $Default({def_String}));
".Replace("\r\n", "\n"));
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
			cctor.Replace("\r\n", "\n").Should().Be(
@"$Init(sm_$InstantiateGenericType({C}, $T), '$x', $Default($T));
$Init(sm_$InstantiateGenericType({C}, $T), '$y', 42);
$Init(sm_$InstantiateGenericType({C}, $T), '$z', 'X');
".Replace("\r\n", "\n"));
		}
	}
}
