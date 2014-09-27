using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CoreLib.Plugin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;
using Saltarelle.Compiler.Roslyn;

namespace CoreLib.Tests.LinkerTests {
	[TestFixture]
	public class LinkerTests {
		private Compilation Compile(string source) {
			return CSharpCompilation.Create(Guid.NewGuid().ToString(), new[] { CSharpSyntaxTree.ParseText("using System; using System.Runtime.CompilerServices; " + source) }, new[] { Common.Mscorlib }, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		}

		private string Process(string source, Func<Compilation, IList<JsStatement>> stmts, IEnumerable<Compilation> references, IMetadataImporter metadata = null, INamer namer = null) {
			var compilation = CSharpCompilation.Create("Test", new[] { CSharpSyntaxTree.ParseText("using System; using System.Runtime.CompilerServices; " + source) }, new[] { Common.Mscorlib }.Concat((references ?? new Compilation[0]).Select(r => r.ToMetadataReference())), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var s = new AttributeStore(compilation, new MockErrorReporter(), new IAutomaticMetadataAttributeApplier[0]);
			var obj = new Linker(metadata ?? new MockMetadataImporter(), namer ?? new MockNamer(), s, compilation);
			var processed = obj.Process(stmts(compilation));
			return OutputFormatter.Format(processed, allowIntermediates: false);
		}

		private void AssertCorrect(string actual, string expected) {
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo(expected.Replace("\r\n", "\n")), "Expected:\n" + expected + "\n\nActual:\n" + actual);
		}

		[Test]
		public void ImportingTypesFromGlobalNamespaceWorks() {
			var otherAsm = Compile(@"class GlobalType {} namespace Global.NestedNamespace.InnerNamespace { class Type {} }");
			var actual = Process("",
			                     _ => new JsStatement[] {
			                             new JsTypeReferenceExpression(otherAsm.GetTypeByMetadataName("GlobalType")),
			                             JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(otherAsm.GetTypeByMetadataName("Global.NestedNamespace.InnerNamespace.Type")), "x"), JsExpression.Number(1)))
			                         }, new[] { otherAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullyQualifiedName().Split('.').Select(x => "$" + x))) });

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
			var actual = Process("class GlobalType {} namespace Global.NestedNamespace.InnerNamespace { class Type {} }",
			                     asm => new JsStatement[] {
			                         new JsTypeReferenceExpression(asm.GetTypeByMetadataName("GlobalType")),
			                         JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("Global.NestedNamespace.InnerNamespace.Type")), "x"), JsExpression.Number(1)))
			                     },
			                     null, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullyQualifiedName().Split('.').Select(x => "$" + x))) });

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
			var actual = Process("[Imported] class MyImportedType {}",
			                     asm => 
			                     new JsStatement[] {
			                         JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("MyImportedType")), "x"), JsExpression.Number(1)))
			                     }, null, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("$" + t.FullyQualifiedName()) });

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
			var actual = Process(@"[assembly: ModuleName(""main-module"")] [ModuleName(""some-module"")] class GlobalType {} namespace Global.NestedNamespace.InnerNamespace { [ModuleName(""some-module"")] class Type {} }",
			                     asm => new JsStatement[] {
			                         new JsTypeReferenceExpression(asm.GetTypeByMetadataName("GlobalType")),
			                         JsStatement.Return(JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("Global.NestedNamespace.InnerNamespace.Type")), "x"), JsExpression.Number(1)))
			                     }, null, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullyQualifiedName().Split('.').Select(x => "$" + x))) });

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
			var actual = Process(@"[assembly: ModuleName(""main-module"")] class GlobalType {}",
			                     asm => new JsStatement[] {
			                         JsExpression.MemberAccess(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("GlobalType")), "x"),
			                     }, null, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"(function() {
	'use strict';
	exports.x;
})();
");
		}

		[Test]
		public void UsingTypeWithNoScriptNameInModuleReturnsTheModuleVariable() {
			var actual = Process(@"[assembly: ModuleName(""main-module"")] [ModuleName(""some-module"")] class GlobalType {}",
			                     asm => new JsStatement[] {
			                         JsStatement.Return(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("GlobalType"))),
			                     }, null, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $somemodule = require('some-module');
return $somemodule;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameInOwnAssemblyButWithDifferentModuleNameResultsInARequire() {
			var actual = Process(@"[assembly: ModuleName(""main-module"")] [ModuleName(""my-module"")] class GlobalType {}",
			                     asm => new JsStatement[] {
			                         JsExpression.MemberAccess(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("GlobalType")), "x"),
			                     }, null, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

			AssertCorrect(actual,
@"'use strict';
require('mscorlib');
var $mymodule = require('my-module');
$mymodule.x;
");
		}

		[Test]
		public void AccessingMemberOnTypeWithEmptyScriptNameResultsInGlobalAccess() {
			var otherAsm = Compile(@"class GlobalType {}");
			var actual = Process("",
			                     asm => new JsStatement[] {
			                         JsExpression.MemberAccess(new JsTypeReferenceExpression(otherAsm.GetTypeByMetadataName("GlobalType")), "x"),
			                     }, new[] { otherAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

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
			var a1 = Compile(@"[assembly: ModuleName(""module1"")] namespace SomeNamespace.InnerNamespace { class Type1 {} }");
			var a2 = Compile(@"[assembly: ModuleName(""module2"")] namespace SomeNamespace.InnerNamespace { class Type2 {} }");
			var a3 = Compile(@"[assembly: ModuleName(""module1"")] namespace SomeNamespace { class Type3 {} }");
			var a4 = Compile(@"[assembly: ModuleName(""module3"")] class Type4 {}");

			var actual = Process("",
			                     asm => new JsStatement[] {
			                         JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(a1.GetTypeByMetadataName("SomeNamespace.InnerNamespace.Type1")), "a"), JsExpression.Member(new JsTypeReferenceExpression(a2.GetTypeByMetadataName("SomeNamespace.InnerNamespace.Type2")), "b")),
			                         JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(a3.GetTypeByMetadataName("SomeNamespace.Type3")), "c"), JsExpression.Member(new JsTypeReferenceExpression(a4.GetTypeByMetadataName("Type4")), "d")),
			                         JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(a1.GetTypeByMetadataName("SomeNamespace.InnerNamespace.Type1")), "e"), JsExpression.Member(new JsTypeReferenceExpression(a4.GetTypeByMetadataName("Type4")), "f")),
			                     }, new[] { a1, a2, a3, a4 });

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
			var a1 = Compile(@"[assembly: ModuleName(""mymodule"")] class Type1 {}");

			var actual = Process("",
			                     asm => new JsStatement[] {
			                         JsExpression.Member(new JsTypeReferenceExpression(a1.GetTypeByMetadataName("Type1")), "a"),
			                     }, new[] { a1 }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType("") });

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
			var actual = Process(@"[assembly: AsyncModule] class GlobalType {} namespace Global.NestedNamespace.InnerNamespace { class Type {} }",
			                     asm => new JsStatement[] {
			                         new JsTypeReferenceExpression(asm.GetTypeByMetadataName("GlobalType")),
			                         JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("Global.NestedNamespace.InnerNamespace.Type")), "x"), JsExpression.Number(1)),
			                     }, null, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(string.Join(".", t.FullyQualifiedName().Split('.').Select(x => "$" + x))) });

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
			var a1 = Compile(@"[assembly: ModuleName(""module1"")] namespace SomeNamespace.InnerNamespace { class Type1 {} }");
			var a2 = Compile(@"[assembly: ModuleName(""module2"")] namespace SomeNamespace.InnerNamespace { class Type2 {} }");
			var a3 = Compile(@"[assembly: ModuleName(""module1"")] namespace SomeNamespace { class Type3 {} }");
			var a4 = Compile(@"[assembly: ModuleName(""module3"")] class Type4 {} }");

			var actual = Process(@"[assembly: AsyncModule]",
			             asm => new JsStatement[] {
			                 JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(a1.GetTypeByMetadataName("SomeNamespace.InnerNamespace.Type1")), "a"), JsExpression.Member(new JsTypeReferenceExpression(a2.GetTypeByMetadataName("SomeNamespace.InnerNamespace.Type2")), "b")),
			                 JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(a3.GetTypeByMetadataName("SomeNamespace.Type3")), "c"), JsExpression.Member(new JsTypeReferenceExpression(a4.GetTypeByMetadataName("Type4")), "d")),
			                 JsExpression.Add(JsExpression.Member(new JsTypeReferenceExpression(a1.GetTypeByMetadataName("SomeNamespace.InnerNamespace.Type1")), "e"), JsExpression.Member(new JsTypeReferenceExpression(a4.GetTypeByMetadataName("Type4")), "f")),
			             }, new[] { a1, a2, a3, a4 });

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
			var actual = Process(@"[assembly: AsyncModule, AdditionalDependency(""my-additional-dep"", ""__unused""), AdditionalDependency(""my-other-dep"", ""myDep2"")]",
			                     asm => new JsStatement[] {
			                         JsExpression.String("myModule")
			                     }, null);

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
			var actual = Process(@"[assembly: AdditionalDependency(""my-additional-dep"", ""myDep""), AdditionalDependencyAttribute(""my-other-dep"", ""myDep2"")]",
			                     asm => new JsStatement[] {
			                         JsExpression.String("myModule")
			                     }, null);

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
			var a1 = Compile(@"[assembly: ModuleName(""mymodule"")] class Type1 {}");
			var a2 = Compile(@"[assembly: ModuleName(""mymodule+"")] class Type2 {}");
			var a3 = Compile(@"[assembly: ModuleName(""mymodule-"")] class Type3 {}");
			var a4 = Compile(@"[assembly: ModuleName(""+"")] class Type4 {}");
			var a5 = Compile(@"[assembly: ModuleName(""-"")] class Type5 {}");

			var actual = Process("",
			                     asm => new JsStatement[] {
			                         JsExpression.Member(new JsTypeReferenceExpression(a1.GetTypeByMetadataName("Type1")), "a"),
			                         JsExpression.Member(new JsTypeReferenceExpression(a2.GetTypeByMetadataName("Type2")), "b"),
			                         JsExpression.Member(new JsTypeReferenceExpression(a3.GetTypeByMetadataName("Type3")), "c"),
			                         JsExpression.Member(new JsTypeReferenceExpression(a4.GetTypeByMetadataName("Type4")), "d"),
			                         JsExpression.Member(new JsTypeReferenceExpression(a5.GetTypeByMetadataName("Type5")), "e"),
			                         JsStatement.Var("mymodule", null),
			                     }, new[] { a1, a2, a3, a4, a5 }, namer: new MockNamer(prefixWithDollar: false));

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
			var a1 = Compile(@"class C1 {}");
			var a2 = Compile(@"[ModuleName(""my-module"")] class C2 {}");
			var a3 = Compile(@"[ModuleName("""")] class C3 {}");
			var a4 = Compile(@"[ModuleName(null)] class C4 {}");

			var actual = Process("",
			                     asm => new JsStatement[] {
			                         JsExpression.Member(new JsTypeReferenceExpression(a1.GetTypeByMetadataName("C1")), "a"),
			                         JsExpression.Member(new JsTypeReferenceExpression(a2.GetTypeByMetadataName("C2")), "b"),
			                         JsExpression.Member(new JsTypeReferenceExpression(a3.GetTypeByMetadataName("C3")), "c"),
			                         JsExpression.Member(new JsTypeReferenceExpression(a4.GetTypeByMetadataName("C4")), "d"),
			                     }, new[] { a1, a2, a3, a4 });

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
			var asm = Compile(
@"[ModuleName(""my-module"")]
class C1 {
	class D1 {}
	[ModuleName(""other-module"")] class D2 {}
	[ModuleName(null)] class D3 {}
	[ModuleName("""")] class D4 {}
}
class C2 {
	class D1 {}
	[ModuleName(""third-module"")] class D2 {}
	[ModuleName(null)] class D3 {}
	[ModuleName("""")] class D4 {}
}");

			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C1")), "a"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C1+D1")), "b"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C1+D2")), "c"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C1+D3")), "d"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C1+D4")), "e"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C2")), "f"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C2+D1")), "g"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C2+D2")), "h"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C2+D3")), "i"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C2+D4")), "j"),
			                     }, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.FullyQualifiedName().Replace(".", "_")) });

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
			var asm = Compile(@"[assembly: ModuleName(""my-module"")] class C1 {} class C2 {}");

			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C1")), "a"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C2")), "b"),
			                     }, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

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
			var asm = Compile(@"[assembly: ModuleName(""my-module"")] [ModuleName(""other-module"")] class C1 {} [ModuleName("""")] class C2 {} [ModuleName(null)] class C3 {}");

			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C1")), "a"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C2")), "b"),
			                         JsExpression.Member(new JsTypeReferenceExpression(asm.GetTypeByMetadataName("C3")), "c"),
			                     }, new[] { asm }, metadata: new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name.Replace("+", "_")) });

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
			var someAsm = Compile("class x {} class y {}");
			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
			                             JsStatement.Var("y", JsExpression.Number(0)),
			                             JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("x")), "a"), JsExpression.Identifier("x")),
			                             JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("y")), "a"), JsExpression.Identifier("y"))
			                         )),
			                         JsExpression.FunctionDefinition(new[] { "x" }, JsStatement.Block(
			                             JsStatement.Var("y", JsExpression.Number(0)),
			                             JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("x")), "a"), JsExpression.Identifier("x")),
			                             JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("y")), "a"), JsExpression.Identifier("y"))
			                         ))
			                     }, new[] { someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

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
			var someAsm = Compile("class x {} class y {}");
			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
			                             JsStatement.Var("y", JsExpression.Number(0)),
			                             JsExpression.FunctionDefinition(new string[0],
			                                 JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("x")), "a"), JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("y")), "a"))
			                             ),
			                             JsExpression.FunctionDefinition(new string[0], JsStatement.Block(
			                                 JsStatement.Var("z", JsExpression.Binary(ExpressionNodeType.Add, JsExpression.Identifier("x"), JsExpression.Identifier("y")))
			                             ))
			                         ))
			                     }, new[] { someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

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
			var someAsm = Compile("class x {}");
			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
			                             JsExpression.FunctionDefinition(new string[0],
			                                 JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("x")), "a")
			                             ),
			                             JsExpression.FunctionDefinition(new string[0], JsStatement.Block(
			                                 JsStatement.Var("x1", null),
			                                 JsExpression.Add(JsExpression.Identifier("x"), JsExpression.Identifier("x1"))
			                             ))
			                         ))
			                     }, new[] { someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

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
			var someAsm = Compile("class x {}");
			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsStatement.Function("f", new[] { "x" }, JsStatement.Block(
			                             JsExpression.FunctionDefinition(new string[0],
			                                 JsExpression.Member(new JsTypeReferenceExpression(someAsm.GetTypeByMetadataName("x")), "a")
			                             ),
			                             JsExpression.FunctionDefinition(new string[0], JsStatement.Block(
			                                 JsExpression.Add(JsExpression.Identifier("x"), JsExpression.Identifier("x1"))
			                             ))
			                         ))
			                     }, new[] { someAsm }, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

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
			var actual = Process("",
			                     _ => new JsStatement[] {
			                         JsExpression.Member(Linker.CurrentAssemblyExpressionStatic, "a")
			                     }, null, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

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
			var actual = Process(@"[assembly: ModuleName(""my-module"")]",
			                     _ => new JsStatement[] {
			                         JsExpression.Member(Linker.CurrentAssemblyExpressionStatic, "a")
			                     }, null, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

			AssertCorrect(actual,
@"(function() {
	'use strict';
	exports.a;
})();
");
		}

		[Test]
		public void CurrentAssemblyReferenceWorksInAsyncModule() {
			var actual = Process(@"[assembly: AsyncModule]",
			                     _ => new JsStatement[] {
			                         JsExpression.Member(Linker.CurrentAssemblyExpressionStatic, "a")
			                     }, null, new MockMetadataImporter { GetTypeSemantics = t => TypeScriptSemantics.NormalType(t.Name) }, namer: new Namer());

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
