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
		private string Process(IList<JsStatement> stmts, IAssembly[] assemblies, IMetadataImporter metadata = null, INamer namer = null) {
			var compilation = new Mock<ICompilation>();
			compilation.SetupGet(_ => _.MainAssembly).Returns(assemblies[0]);
			compilation.SetupGet(_ => _.Assemblies).Returns(assemblies);
			var s = new AttributeStore(compilation.Object, new MockErrorReporter());
			var obj = new Linker(metadata ?? new MockMetadataImporter(), namer ?? new MockNamer(), s, compilation.Object);
			var processed = obj.Process(stmts);
			return OutputFormatter.Format(processed, allowIntermediates: false);
		}

		private void AssertCorrect(string actual, string expected) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void ImportingTypesFromGlobalNamespaceWorks() {
			var otherAsm = Common.CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", otherAsm)),
				JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", otherAsm)), "x"), JsExpression.Number(1)))
			}, new[] { Common.CreateMockAssembly(), otherAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
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
				new JsTypeReferenceExpression(type),
				JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", asm)), "x"), JsExpression.Number(1)))
			}, new[] { asm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
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
				JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(type), "x"), JsExpression.Number(1)))
			}, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("$" + t.FullName) });

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
	return $MyImportedType.x + 1;
})();
");
		}

		[Test]
		public void ImportingTypeFromOwnAssemblyButOtherModuleNameResultsInARequire() {
			var asm = Common.CreateMockAssembly(attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var type = Common.CreateMockTypeDefinition("GlobalType", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("some-module") });
			var actual = Process(new JsStatement[] {
				new JsTypeReferenceExpression(type),
				JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("some-module") })), "x"), JsExpression.Number(1)))
			}, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $somemodule = require('some-module');
$somemodule.$GlobalType;
return $somemodule.$Global.$NestedNamespace.$InnerNamespace.$Type.x + 1;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameInOwnAssemblyWithModuleNameUsesExports() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var actual = Process(new JsStatement[] {
				JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", asm)), "x"),
			}, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"(function() {
	'use strict';
	exports.x;
})();
");
		}

		[Test]
		public void UsingTypeWithNoScriptNameInModuleReturnsTheModuleVariable() {
			var asm = Common.CreateMockAssembly(attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var type = Common.CreateMockTypeDefinition("GlobalType", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("some-module") });
			var actual = Process(new JsStatement[] {
				JsStatement.Return(new JsTypeReferenceExpression(type)),
			}, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $somemodule = require('some-module');
return $somemodule;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameInOwnAssemblyButWithDifferentModuleNameResultsInARequire() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("main-module") });
			var actual = Process(new JsStatement[] {
				JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") })), "x"),
			}, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $mymodule = require('my-module');
$mymodule.x;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameResultsInGlobalAccess() {
			var otherAsm = Common.CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				JsExpression.MemberAccess(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("GlobalType", otherAsm)), "x"),
			}, new[] { Common.CreateMockAssembly(), otherAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
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
				JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "a"), JsExpression.Member(new JsTypeReferenceExpression(t2), "b")),
				JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t3), "c"), JsExpression.Member(new JsTypeReferenceExpression(t4), "d")),
				JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "e"), JsExpression.Member(new JsTypeReferenceExpression(t4), "f")),
			}, new[] { Common.CreateMockAssembly(), t1.ParentAssembly, t2.ParentAssembly, t3.ParentAssembly, t4.ParentAssembly });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $module1 = require('module1');
var $module2 = require('module2');
var $module3 = require('module3');
var $asm = {};
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
				JsExpression.Member(new JsTypeReferenceExpression(t1), "a"),
			}, new[] { Common.CreateMockAssembly(), t1.ParentAssembly }, metadata: md);

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $mymodule = require('mymodule');
var $asm = {};
$mymodule.a;
");
		}

		[Test]
		public void AsyncModuleWithoutReferencesWorks() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new AsyncModuleAttribute() });
			var type = Common.CreateMockTypeDefinition("GlobalType", asm);
			var actual = Process(new JsStatement[] {
				new JsTypeReferenceExpression(type),
				JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("Global.NestedNamespace.InnerNamespace.Type", asm)), "x"), JsExpression.Number(1)),
			}, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullName.Split('.').Select(x => "$" + x))) });

			AssertCorrect(actual,
@"define(['mscorlib'], function($_) {
	'use strict';
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
				JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "a"), JsExpression.Member(new JsTypeReferenceExpression(t2), "b")),
				JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t3), "c"), JsExpression.Member(new JsTypeReferenceExpression(t4), "d")),
				JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(t1), "e"), JsExpression.Member(new JsTypeReferenceExpression(t4), "f")),
			}, new[] { asm, t1.ParentAssembly, t2.ParentAssembly, t3.ParentAssembly, t4.ParentAssembly });

			AssertCorrect(actual,
@"define(['mscorlib', 'module1', 'module2', 'module3'], function($_, $module1, $module2, $module3) {
	'use strict';
	var exports = {};
	$module1.SomeNamespace.InnerNamespace.Type1.a + $module2.SomeNamespace.InnerNamespace.Type2.b;
	$module1.SomeNamespace.Type3.c + $module3.Type4.d;
	$module1.SomeNamespace.InnerNamespace.Type1.e + $module3.Type4.f;
	return exports;
});
");
		}

		[Test]
		public void AsyncModuleWithAdditionalDependenciesWorks() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { 
				() => new AsyncModuleAttribute(), 
				() => new AdditionalDependencyAttribute("my-additional-dep", "__unused"),
				() => new AdditionalDependencyAttribute("my-other-dep", "myDep2")});
			var actual = Process(new JsStatement[] {
				JsExpression.String("myModule")
			}, new[] { asm });

			AssertCorrect(actual,
@"define(['mscorlib', 'my-additional-dep', 'my-other-dep'], function($_, __unused, myDep2) {
	'use strict';
	var exports = {};
	'myModule';
	return exports;
});
");
		}

		[Test]
		public void ModuleWithAdditionalDepenciesWorks() {
			var asm = Common.CreateMockAssembly(new Expression<Func<Attribute>>[] {
				() => new AdditionalDependencyAttribute("my-additional-dep", "myDep"),
				() => new AdditionalDependencyAttribute("my-other-dep", "myDep2")});
			var actual = Process(new JsStatement[] {
				JsExpression.String("myModule")
			}, new[] { asm });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var myDep = require('my-additional-dep');
var myDep2 = require('my-other-dep');
var $asm = {};
'myModule';
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
				JsExpression.Member(new JsTypeReferenceExpression(t1), "a"),
				JsExpression.Member(new JsTypeReferenceExpression(t2), "b"),
				JsExpression.Member(new JsTypeReferenceExpression(t3), "c"),
				JsExpression.Member(new JsTypeReferenceExpression(t4), "d"),
				JsExpression.Member(new JsTypeReferenceExpression(t5), "e"),
				JsStatement.Var("mymodule", null),
			}, new[] { Common.CreateMockAssembly(), t1.ParentAssembly, t2.ParentAssembly, t3.ParentAssembly, t4.ParentAssembly, t5.ParentAssembly }, namer: new MockNamer(prefixWithDollar: false));

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var _2 = require('-');
var _ = require('+');
var mymodule2 = require('mymodule');
var mymodule4 = require('mymodule-');
var mymodule3 = require('mymodule+');
var $asm = {};
mymodule2.Type1.a;
mymodule3.Type2.b;
mymodule4.Type3.c;
_.Type4.d;
_2.Type5.e;
var mymodule;
");
		}

		[Test]
		public void ModuleNameAttributeCanBeSpecifiedOnType() {
			var c1 = Common.CreateMockTypeDefinition("C1", Common.CreateMockAssembly());
			var c2 = Common.CreateMockTypeDefinition("C2", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") });
			var c3 = Common.CreateMockTypeDefinition("C3", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("") });
			var c4 = Common.CreateMockTypeDefinition("C4", Common.CreateMockAssembly(), attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute(null) });

			var actual = Process(new JsStatement[] {
				JsExpression.Member(new JsTypeReferenceExpression(c1), "a"),
				JsExpression.Member(new JsTypeReferenceExpression(c2), "b"),
				JsExpression.Member(new JsTypeReferenceExpression(c3), "c"),
				JsExpression.Member(new JsTypeReferenceExpression(c4), "d"),
			}, new[] { Common.CreateMockAssembly(), c1.ParentAssembly, c2.ParentAssembly, c3.ParentAssembly, c4.ParentAssembly });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $mymodule = require('my-module');
var $asm = {};
C1.a;
$mymodule.C2.b;
C3.c;
C4.d;
");
		}

		[Test]
		public void ModuleNameAttributeIsInheritedToInnerTypesButCanBeOverridden() {
			var asm = Common.CreateMockAssembly();
			var c1 = Common.CreateMockTypeDefinition("C1", asm, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") });
			var c1d1 = Common.CreateMockTypeDefinition("C1+D1", asm, declaringType: c1);
			var c1d2 = Common.CreateMockTypeDefinition("C1+D2", asm, declaringType: c1, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("other-module") });
			var c1d3 = Common.CreateMockTypeDefinition("C1+D3", asm, declaringType: c1, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute(null) });
			var c1d4 = Common.CreateMockTypeDefinition("C1+D4", asm, declaringType: c1, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("") });

			var c2 = Common.CreateMockTypeDefinition("C2", asm);
			var c2d1 = Common.CreateMockTypeDefinition("C2+D1", asm, declaringType: c2);
			var c2d2 = Common.CreateMockTypeDefinition("C2+D2", asm, declaringType: c2, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("third-module") });
			var c2d3 = Common.CreateMockTypeDefinition("C2+D3", asm, declaringType: c2, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute(null) });
			var c2d4 = Common.CreateMockTypeDefinition("C2+D4", asm, declaringType: c2, attributes: new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("") });

			var actual = Process(new JsStatement[] {
				JsExpression.Member(new JsTypeReferenceExpression(c1), "a"),
				JsExpression.Member(new JsTypeReferenceExpression(c1d1), "b"),
				JsExpression.Member(new JsTypeReferenceExpression(c1d2), "c"),
				JsExpression.Member(new JsTypeReferenceExpression(c1d3), "d"),
				JsExpression.Member(new JsTypeReferenceExpression(c1d4), "e"),
				JsExpression.Member(new JsTypeReferenceExpression(c2), "f"),
				JsExpression.Member(new JsTypeReferenceExpression(c2d1), "g"),
				JsExpression.Member(new JsTypeReferenceExpression(c2d2), "h"),
				JsExpression.Member(new JsTypeReferenceExpression(c2d3), "i"),
				JsExpression.Member(new JsTypeReferenceExpression(c2d4), "j"),
			}, new[] { Common.CreateMockAssembly(), asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $mymodule = require('my-module');
var $othermodule = require('other-module');
var $thirdmodule = require('third-module');
var $asm = {};
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
				JsExpression.Member(new JsTypeReferenceExpression(c1), "a"),
				JsExpression.Member(new JsTypeReferenceExpression(c2), "b"),
			}, new[] { Common.CreateMockAssembly(), asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $mymodule = require('my-module');
var $asm = {};
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
				JsExpression.Member(new JsTypeReferenceExpression(c1), "a"),
				JsExpression.Member(new JsTypeReferenceExpression(c2), "b"),
				JsExpression.Member(new JsTypeReferenceExpression(c3), "c"),
			}, new[] { Common.CreateMockAssembly(), asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $othermodule = require('other-module');
var $asm = {};
$othermodule.C1.a;
C2.b;
C3.c;
");
		}

		[Test]
		public void ClassWithSameNameAsVariable1() {
			var someAsm = Common.CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
					JsStatement.Var("y", JsExpression.Number(0)),
					JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("x", someAsm)), "a"), JsExpression.Identifier("x")),
					JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("y", someAsm)), "a"), JsExpression.Identifier("y"))
				)),
				JsExpression.FunctionDefinition(new[] { "x" }, JsStatement.Block(
					JsStatement.Var("y", JsExpression.Number(0)),
					JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("x", someAsm)), "a"), JsExpression.Identifier("x")),
					JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("y", someAsm)), "a"), JsExpression.Identifier("y"))
				))
			}, new[] { Common.CreateMockAssembly(), someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
	function f(x1) {
		var y1 = 0;
		x.a + x1;
		y.a + y1;
	}
	(function(x1) {
		var y1 = 0;
		x.a + x1;
		y.a + y1;
	});
})();
");
		}

		[Test]
		public void ClassWithSameNameAsVariable2() {
			var someAsm = Common.CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
					JsStatement.Var("y", JsExpression.Number(0)),
					JsExpression.FunctionDefinition(new string[0],
						JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("x", someAsm)), "a"), JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("y", someAsm)), "a"))
					),
					JsExpression.FunctionDefinition(new string[0], JsStatement.Block(
						JsStatement.Var("z", JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Identifier("x"), JsExpression.Identifier("y")))
					))
				))
			}, new[] { Common.CreateMockAssembly(), someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
	function f(x1) {
		var y1 = 0;
		(function() {
			x.a + y.a;
		});
		(function() {
			var z = x1 + y1;
		});
	}
})();
");
		}

		[Test]
		public void RenamedVariableClashWithOtherVariable() {
			var someAsm = Common.CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
					JsExpression.FunctionDefinition(new string[0],
						JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("x", someAsm)), "a")
					),
					JsExpression.FunctionDefinition(new string[0], JsStatement.Block(
						JsStatement.Var("x1", null),
						JsExpression.Add(JsExpression.Identifier("x"), JsExpression.Identifier("x1"))
					))
				))
			}, new[] { Common.CreateMockAssembly(), someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
	function f(x2) {
		(function() {
			x.a;
		});
		(function() {
			var x1;
			x2 + x1;
		});
	}
})();
");
		}

		[Test]
		public void RenamedVariableClashWithImplicitGlobal() {
			var someAsm = Common.CreateMockAssembly();
			var actual = Process(new JsStatement[] {
				JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
					JsExpression.FunctionDefinition(new string[0],
						JsExpression.Member(new JsTypeReferenceExpression(Common.CreateMockTypeDefinition("x", someAsm)), "a")
					),
					JsExpression.FunctionDefinition(new string[0], JsStatement.Block(
						JsExpression.Add(JsExpression.Identifier("x"), JsExpression.Identifier("x1"))
					))
				))
			}, new[] { Common.CreateMockAssembly(), someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
	function f(x2) {
		(function() {
			x.a;
		});
		(function() {
			x2 + x1;
		});
	}
})();
");
		}

		[Test]
		public void CurrentAssemblyReferenceWorksInNonModule() {
			var actual = Process(new JsStatement[] {
				JsExpression.Member(Linker.CurrentAssemblyExpressionStatic, "a")
			}, new[] { Common.CreateMockAssembly() }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"(function() {
	'use strict';
	var $asm = {};
	$asm.a;
})();
");
		}

		[Test]
		public void CurrentAssemblyReferenceWorksInModule() {
			var actual = Process(new JsStatement[] {
				JsExpression.Member(Linker.CurrentAssemblyExpressionStatic, "a")
			}, new[] { Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new ModuleNameAttribute("my-module") }) }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"(function() {
	'use strict';
	exports.a;
})();
");
		}

		[Test]
		public void CurrentAssemblyReferenceWorksInAsyncModule() {
			var actual = Process(new JsStatement[] {
				JsExpression.Member(Linker.CurrentAssemblyExpressionStatic, "a")
			}, new[] { Common.CreateMockAssembly(new Expression<Func<Attribute>>[] { () => new AsyncModuleAttribute() }) }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"define(['mscorlib'], function(_) {
	'use strict';
	var exports = {};
	exports.a;
	return exports;
});
");
		}
	}
}
