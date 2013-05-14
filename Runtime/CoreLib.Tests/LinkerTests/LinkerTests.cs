using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CoreLib.Plugin;
using ICSharpCode.NRefactory.TypeSystem;
using Moq;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.LinkerTests {
	[TestFixture]
	public class LinkerTests {
		private string Process(IList<JsStatement> stmts, IAssembly mainAssembly, IMetadataImporter metadata = null, INamer namer = null) {
			var compilation = new Mock<ICompilation>();
			compilation.SetupGet(_ => _.MainAssembly).Returns(mainAssembly);
			var obj = new Linker(metadata ?? new MockMetadataImporter(), namer ?? new MockNamer(), compilation.Object);
			var processed = obj.Process(stmts);
			return string.Join("", processed.Select(s => OutputFormatter.Format(s, allowIntermediates: false)));
		}

		private void AssertCorrect(string actual, string expected) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void ImportingTypesFromGlobalNamespaceWorks() {
			var otherAsm = Common.CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", otherAsm))),
				new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", otherAsm)), "x"), JsExpression.Number(1)))
			}, Common.CreateMockAssembly(), new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"(function() {
	$GlobalType;
	return $Global.$NestedNamespace.$InnerNamespace.$Type.x + 1;
})();
");
		}

		[Test]
		public void ImportingTypeFromOwnAssemblyUsesTheTypeVariable() {
			var asm = Common.CreateMockAssembly();
			var type = Common.CreateMockTypeDefinition("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(type)),
				new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", asm)), "x"), JsExpression.Number(1)))
			}, asm, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"(function() {
	$$GlobalType;
	return $$Global_$NestedNamespace_$InnerNamespace_$Type.x + 1;
})();
");
		}

		[Test]
		public void ImportingImportedTypeFromOwnAssemblyWorks() {
			var asm = Common.CreateMockAssembly();
			var type = Common.CreateMockTypeDefinition("MyImportedType", asm, attributes: new Expression<Func<Attribute>>[] { () => new ImportedAttribute() });
			var actual = Process(new JsStatement[] {
				new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(type), "x"), JsExpression.Number(1)))
			}, asm, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("$" + t.FullName) });

			AssertCorrect(actual,
@"(function() {
	return $MyImportedType.x + 1;
})();
");
		}

		[Test]
		public void ImportingTypeFromOwnAssemblyButOtherModuleNameResultsInARequire() {
			var asm = Common.CreateMockAssembly(attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var type = Common.CreateMockTypeDefinition("GlobalType", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("some-module") });
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(type)),
				new JsReturnStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("some-module") })), "x"), JsExpression.Number(1)))
			}, asm, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"require('mscorlib');
var $somemodule = require('some-module');
$somemodule.$GlobalType;
return $somemodule.$Global.$NestedNamespace.$InnerNamespace.$Type.x + 1;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameInOwnAssemblyWithModuleNameUsesExports() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", asm)), "x")),
			}, asm, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"(function() {
	exports.x;
})();
");
		}

		[Test]
		public void UsingTypeWithNoScriptNameInModuleReturnsTheModuleVariable() {
			var asm = Common.CreateMockAssembly(attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var type = Common.CreateMockTypeDefinition("GlobalType", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("some-module") });
			var actual = Process(new JsStatement[] {
				new JsReturnStatement(new JsTypeReferenceExpression(type)), 
			}, asm, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"require('mscorlib');
var $somemodule = require('some-module');
return $somemodule;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameInOwnAssemblyButWithDifferentModuleNameResultsInARequire() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") })), "x")),
			}, asm, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('my-module');
$mymodule.x;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameResultsInGlobalAccess() {
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", Common.CreateMockAssembly())), "x")),
			}, Common.CreateMockAssembly(), new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"(function() {
	x;
})();
");
		}

		[Test]
		public void ImportingTypesFromModulesWorks() {
			var t1 = Common.CreateMockTypeDefinition("SomeNamespace.InnerNamespace.Type1", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module1") }));
			var t2 = Common.CreateMockTypeDefinition("SomeNamespace.InnerNamespace.Type2", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module2") }));
			var t3 = Common.CreateMockTypeDefinition("SomeNamespace.Type3", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module1") }));
			var t4 = Common.CreateMockTypeDefinition("Type4", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module3") }));

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "a"), JsExpression.Member(new JsTypeReferenceExpression(t2), "b"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t3), "c"), JsExpression.Member(new JsTypeReferenceExpression(t4), "d"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "e"), JsExpression.Member(new JsTypeReferenceExpression(t4), "f"))),
			}, Common.CreateMockAssembly());

			AssertCorrect(actual,
@"require('mscorlib');
var $module1 = require('module1');
var $module2 = require('module2');
var $module3 = require('module3');
$module1.SomeNamespace.InnerNamespace.Type1.a + $module2.SomeNamespace.InnerNamespace.Type2.b;
$module1.SomeNamespace.Type3.c + $module3.Type4.d;
$module1.SomeNamespace.InnerNamespace.Type1.e + $module3.Type4.f;
");
		}

		[Test]
		public void ImportingGlobalMethodsFromModulesWorks() {
			var t1 = Common.CreateMockTypeDefinition("Type1", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("mymodule") }));
			var md = new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") };

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t1), "a")),
			}, Common.CreateMockAssembly(), metadata: md);

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('mymodule');
$mymodule.a;
");
		}

		[Test]
		public void AsyncModuleWithoutReferencesWorks() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new AsyncModuleAttribute() });
			var type = Common.CreateMockTypeDefinition("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(new JsTypeReferenceExpression(type)),
				new JsExpressionStatement(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", asm)), "x"), JsExpression.Number(1))),
			}, asm, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"define(['mscorlib'], function($_) {
	var exports = {};
	$$GlobalType;
	$$Global_$NestedNamespace_$InnerNamespace_$Type.x + 1;
	return exports;
});
");
		}

		[Test]
		public void AsyncModuleWithReferencesWorks() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new AsyncModuleAttribute() });
			var t1 = Common.CreateMockTypeDefinition("SomeNamespace.InnerNamespace.Type1", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module1") }));
			var t2 = Common.CreateMockTypeDefinition("SomeNamespace.InnerNamespace.Type2", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module2") }));
			var t3 = Common.CreateMockTypeDefinition("SomeNamespace.Type3", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module1") }));
			var t4 = Common.CreateMockTypeDefinition("Type4", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("module3") }));

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "a"), JsExpression.Member(new JsTypeReferenceExpression(t2), "b"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t3), "c"), JsExpression.Member(new JsTypeReferenceExpression(t4), "d"))),
				new JsExpressionStatement(JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "e"), JsExpression.Member(new JsTypeReferenceExpression(t4), "f"))),
			}, asm);

			AssertCorrect(actual,
@"define(['mscorlib', 'module1', 'module2', 'module3'], function($_, $module1, $module2, $module3) {
	var exports = {};
	$module1.SomeNamespace.InnerNamespace.Type1.a + $module2.SomeNamespace.InnerNamespace.Type2.b;
	$module1.SomeNamespace.Type3.c + $module3.Type4.d;
	$module1.SomeNamespace.InnerNamespace.Type1.e + $module3.Type4.f;
	return exports;
});
");
		}

		[Test]
		public void AsyncModuleWithAdditionalDependenciesWorks()
		{
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { 
				() => new AsyncModuleAttribute(), 
				() => new AdditionalDependencyAttribute("my-additional-dep", "__unused"),
				() => new AdditionalDependencyAttribute("my-other-dep", "myDep2")});
			var type = Common.CreateMockTypeDefinition("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsReturnStatement(JsExpression.String("myModule"))
			}, asm);

			AssertCorrect(actual,
@"define(['mscorlib', 'my-additional-dep', 'my-other-dep'], function($_, __unused, myDep2) {
	var exports = {};
	return 'myModule';
	return exports;
});
");
		}

		[Test]
		public void ModuleWithAdditionalDepenciesWorks() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] {
				() => new AdditionalDependencyAttribute("my-additional-dep", "myDep"),
				() => new AdditionalDependencyAttribute("my-other-dep", "myDep2")});
			var type = Common.CreateMockTypeDefinition("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsReturnStatement(JsExpression.String("myModule"))
			}, asm);

			AssertCorrect(actual,
@"require('mscorlib');
var myDep = require('my-additional-dep');
var myDep2 = require('my-other-dep');
return 'myModule';
");
		}

		[Test]
		public void GeneratedModuleAliasesAreValidAndDoNotClashWithEachOtherOrUsedSymbols() {
			var t1 = Common.CreateMockTypeDefinition("Type1", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("mymodule") }));
			var t2 = Common.CreateMockTypeDefinition("Type2", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("mymodule+") }));
			var t3 = Common.CreateMockTypeDefinition("Type3", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("mymodule-") }));
			var t4 = Common.CreateMockTypeDefinition("Type4", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("+") }));
			var t5 = Common.CreateMockTypeDefinition("Type5", Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("-") }));

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t1), "a")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t2), "b")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t3), "c")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t4), "d")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(t5), "e")),
				new JsVariableDeclarationStatement("mymodule", null),
			}, Common.CreateMockAssembly(), namer: new MockNamer(prefixWithDollar: false));

			AssertCorrect(actual,
@"require('mscorlib');
var _2 = require('-');
var _ = require('+');
var mymodule2 = require('mymodule');
var mymodule4 = require('mymodule-');
var mymodule3 = require('mymodule+');
mymodule2.Type1.a;
mymodule3.Type2.b;
mymodule4.Type3.c;
_.Type4.d;
_2.Type5.e;
var mymodule;
");
		}

		[Test]
		public void UsedSymbolsGathererWorks() {
			var program = JavaScriptParser.Parser.ParseProgram(@"
function a(b, c) {
	function d(e) {
		var f = g, h = i + j;
		for (var k in l) {
		}
		for (m in a) {}
		var n = function o(p, q) {
			r;
		}
	}
}
s + (t * -u);
try {
}
catch (v) {
}
for (w = 0, x; w < 1; w++) {
}
for (var y = 0, z; w < 1; w++) {
}
");
			var actual = Linker.UsedSymbolsGatherer.Analyze(program);
			Assert.That(actual, Is.EquivalentTo(new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" }));
		}

		[Test]
		public void ModuleNameAttributeCanBeSpecifiedOnType() {
			var c1 = Common.CreateMockTypeDefinition("C1", Common.CreateMockAssembly());
			var c2 = Common.CreateMockTypeDefinition("C2", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") });
			var c3 = Common.CreateMockTypeDefinition("C3", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("") });
			var c4 = Common.CreateMockTypeDefinition("C4", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute(null) });

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1), "a")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2), "b")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c3), "c")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c4), "d")),
			}, Common.CreateMockAssembly());

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('my-module');
C1.a;
$mymodule.C2.b;
C3.c;
C4.d;
");
		}

		[Test]
		public void ModuleNameAttributeIsInheritedToInnerTypesButCanBeOverridden() {
			var c1 = Common.CreateMockTypeDefinition("C1", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") });
			var c1d1 = Common.CreateMockTypeDefinition("C1+D1", Common.CreateMockAssembly(), declaringType: c1);
			var c1d2 = Common.CreateMockTypeDefinition("C1+D2", Common.CreateMockAssembly(), declaringType: c1, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("other-module") });
			var c1d3 = Common.CreateMockTypeDefinition("C1+D3", Common.CreateMockAssembly(), declaringType: c1, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute(null) });
			var c1d4 = Common.CreateMockTypeDefinition("C1+D4", Common.CreateMockAssembly(), declaringType: c1, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("") });

			var c2 = Common.CreateMockTypeDefinition("C2", Common.CreateMockAssembly());
			var c2d1 = Common.CreateMockTypeDefinition("C2+D1", Common.CreateMockAssembly(), declaringType: c2);
			var c2d2 = Common.CreateMockTypeDefinition("C2+D2", Common.CreateMockAssembly(), declaringType: c2, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("third-module") });
			var c2d3 = Common.CreateMockTypeDefinition("C2+D3", Common.CreateMockAssembly(), declaringType: c2, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute(null) });
			var c2d4 = Common.CreateMockTypeDefinition("C2+D4", Common.CreateMockAssembly(), declaringType: c2, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("") });

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1), "a")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1d1), "b")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1d2), "c")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1d3), "d")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1d4), "e")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2), "f")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2d1), "g")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2d2), "h")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2d3), "i")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2d4), "j")),
			}, Common.CreateMockAssembly(), metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('my-module');
var $othermodule = require('other-module');
var $thirdmodule = require('third-module');
$mymodule.C1.a;
C1_D1.b;
$othermodule.C1_D2.c;
C1_D3.d;
C1_D4.e;
C2.f;
C2_D1.g;
$thirdmodule.C2_D2.h;
C2_D3.i;
C2_D4.j;
");
		}

		[Test]
		public void ModuleNameOnAssemblyWorks() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") });
			var c1 = Common.CreateMockTypeDefinition("C1", asm);
			var c2 = Common.CreateMockTypeDefinition("C2", asm);

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1), "a")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2), "b")),
			}, Common.CreateMockAssembly(), metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

			AssertCorrect(actual,
@"require('mscorlib');
var $mymodule = require('my-module');
$mymodule.C1.a;
$mymodule.C2.b;
");
		}

		[Test]
		public void ModuleNameOnAssemblyCanBeOverriddenOnType() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") });
			var c1 = Common.CreateMockTypeDefinition("C1", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("other-module") });
			var c2 = Common.CreateMockTypeDefinition("C2", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("") });
			var c3 = Common.CreateMockTypeDefinition("C3", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute(null) });

			var actual = Process(new JsStatement[] {
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c1), "a")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c2), "b")),
				new JsExpressionStatement(JsExpression.Member(new JsTypeReferenceExpression(c3), "c")),
			}, Common.CreateMockAssembly(), metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

			AssertCorrect(actual,
@"require('mscorlib');
var $othermodule = require('other-module');
$othermodule.C1.a;
C2.b;
C3.c;
");
		}
	}
}
