using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.OOPEmulation;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class StructTests : OOPEmulatorTestBase {
		[Test]
		public void NonGenericStructWithAllDataWorks() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public struct MyStruct : Interface1, Interface2, Interface3 {
	public MyStruct(int b) { b = 0; }
	public MyStruct(string c) { c = null; }
	public void M1(int d) { d = 0; }
	public void M2(int e) { e = 0; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyStruct() {
		int h = 0;
		int i = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyStruct
var $MyStruct = global.MyStruct = {Script}.mkType($asm, 'MyStruct', function() {
}, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	},
	getHashCode: function() {
		return 0;
	},
	equals: function(o) {
		return {Script}.isInstanceOfType(o, $MyStruct);
	}
}, {
	$ctor1: function(b) {
		b = 0;
	},
	$ctor2: function(c) {
		c = null;
	},
	s1: function(f) {
		f = 0;
	},
	s2: function(g) {
		g = 0;
	}
});
-
{Script}.initStruct($MyStruct, [{Interface1}, {Interface2}, {Interface3}]);
$MyStruct.$ctor1.prototype = $MyStruct.$ctor2.prototype = $MyStruct.prototype;
", "MyStruct");
		}

		[Test]
		public void GenericStructWithIgnoreGenericArgumentsIsRegisteredLikeNonGenericStruct() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
[System.Runtime.CompilerServices.IncludeGenericArguments(false)]
public struct MyStruct<T> : Interface1, Interface2, Interface3 {
	public MyStruct(int b) { b = 0; }
	public MyStruct(string c) { c = null; }
	public void M1(int d) { d = 0; }
	public void M2(int e) { e = 0; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyStruct() {
		int h = 0;
		int i = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyStruct<T>
var $MyStruct = global.MyStruct = {Script}.mkType($asm, 'MyStruct', function() {
}, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	},
	getHashCode: function() {
		return 0;
	},
	equals: function(o) {
		return {Script}.isInstanceOfType(o, $MyStruct);
	}
}, {
	$ctor1: function(b) {
		b = 0;
	},
	$ctor2: function(c) {
		c = null;
	},
	s1: function(f) {
		f = 0;
	},
	s2: function(g) {
		g = 0;
	}
});
-
{Script}.initStruct($MyStruct, [{Interface1}, {Interface2}, {Interface3}]);
$MyStruct.$ctor1.prototype = $MyStruct.$ctor2.prototype = $MyStruct.prototype;
", "MyStruct<T>");
		}

		[Test]
		public void StructWithoutInstanceMethodsWorks() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public struct MyStruct : Interface1, Interface2, Interface3 {
	public MyStruct(int b) { b = 0; }
	public MyStruct(string c) { c = null; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyStruct() {
		int h = 0;
		int i = 0;
	}

}",
@"////////////////////////////////////////////////////////////////////////////////
// MyStruct
var $MyStruct = global.MyStruct = {Script}.mkType($asm, 'MyStruct', function() {
}, {
	getHashCode: function() {
		return 0;
	},
	equals: function(o) {
		return {Script}.isInstanceOfType(o, $MyStruct);
	}
}, {
	$ctor1: function(b) {
		b = 0;
	},
	$ctor2: function(c) {
		c = null;
	},
	s1: function(f) {
		f = 0;
	},
	s2: function(g) {
		g = 0;
	}
});
-
{Script}.initStruct($MyStruct, [{Interface1}, {Interface2}, {Interface3}]);
$MyStruct.$ctor1.prototype = $MyStruct.$ctor2.prototype = $MyStruct.prototype;
", "MyStruct");
		}

		[Test]
		public void StructWithNamespaceWorks() {
			AssertCorrectEmulation(
@"namespace SomeNamespace.InnerNamespace {
	public struct MyStruct {
	}
}",
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyStruct
var $SomeNamespace_InnerNamespace_MyStruct = global.SomeNamespace.InnerNamespace.MyStruct = {Script}.mkType($asm, 'SomeNamespace.InnerNamespace.MyStruct', function() {
}, {
	getHashCode: function() {
		return 0;
	},
	equals: function(o) {
		return {Script}.isInstanceOfType(o, $SomeNamespace_InnerNamespace_MyStruct);
	}
});
-
{Script}.initStruct($SomeNamespace_InnerNamespace_MyStruct);
", "SomeNamespace.InnerNamespace.MyStruct");
		}

		[Test]
		public void StructWithoutUnnamedConstructorWorks() {
			AssertCorrectEmulation(
@"using System.Runtime.CompilerServices;
public struct MyStruct {
	[ScriptName(""someName"")] public MyStruct(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {}
	public void M1() {}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyStruct
var $MyStruct = global.MyStruct = {Script}.mkType($asm, 'MyStruct', null, {
	m1: function() {
	},
	getHashCode: function() {
		return 0;
	},
	equals: function(o) {
		return {Script}.isInstanceOfType(o, $MyStruct);
	}
}, {
	someName: function() {
	},
	createInstance: function() {
		return new {MyStruct}.someName();
	}
});
-
{Script}.initStruct($MyStruct);
$MyStruct.someName.prototype = $MyStruct.prototype;
", "MyStruct");
		}

		[Test]
		public void GenericStructWorks() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2<T1, T2> {}
public interface Interface3 {}
[System.Runtime.CompilerServices.IncludeGenericArguments(true)]
public struct MyStruct<T1, T2> : Interface1, Interface2<T2, int>, Interface3 {
	public MyStruct(int b) { b = 0; }
	public MyStruct(string c) { c = null; }
	public void M1(int d) { d = 0; }
	public void M2(int e) { e = 0; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyStruct() {
		int h = 0;
		int i = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyStruct<T1, T2>
var $MyStruct$2 = global.MyStruct$2 = {Script}.mkType($asm, 'MyStruct$2', function(T1, T2) {
	var $type = {Script}.registerGenericStructInstance({MyStruct}, [T1, T2], function() {
	}, {
		m1: function(d) {
			d = 0;
		},
		m2: function(e) {
			e = 0;
		},
		getHashCode: function() {
			return 0;
		},
		equals: function(o) {
			return {Script}.isInstanceOfType(o, $type);
		}
	}, {
		$ctor1: function(b) {
			b = 0;
		},
		$ctor2: function(c) {
			c = null;
		},
		s1: function(f) {
			f = 0;
		},
		s2: function(g) {
			g = 0;
		}
	}, function() {
		return [{Interface1}, {Script}.makeGenericType({Interface2}, [T2, {Int32}]), {Interface3}];
	});
	$type.$ctor1.prototype = $type.$ctor2.prototype = $type.prototype;
	var h = 0;
	var i = 0;
	return $type;
});
{Script}.initGenericStruct($MyStruct$2, 2);
-
", "MyStruct<T1, T2>");
		}

		[Test]
		public void GeneratedGetHashCodeGeneratesHashCodeBasedOnAllInstanceFields() {
			var compilation = Compile(@"
using System;
using System.Runtime.CompilerServices;
public enum E1 {}
[NamedValues] public enum E2 {}
[Mutable] public struct S {
	public static int FS;
	public readonly int F1;
	public readonly int? F2;
	public readonly bool F3;
	public readonly bool? F4;
	public readonly E1 F5;
	public readonly E2 F6;
	public readonly E1? F7;
	public readonly E2? F8;
	public readonly object F9;
	public readonly DateTime F10;
	public readonly DateTime? F11;
	public int P1 { get; set; }
	[IntrinsicProperty] public int P2 { get; set; }
	public event System.Action E1;
	[NonScriptable] public readonly int F12;
	[NonScriptable] public int P3 { get; set; }
	[NonScriptable] public event System.Action E2;
}");
			var getHashCode = FindInstanceMember(compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")), "getHashCode");
			Assert.That(OutputFormatter.Format(getHashCode, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function() {
	var h = this.f1;
	h = h * 397 ^ (this.f2 || 0);
	h = h * 397 ^ (this.f3 ? 1 : 0);
	h = h * 397 ^ (this.f4 ? 1 : 0);
	h = h * 397 ^ this.f5;
	h = h * 397 ^ (this.f6 ? {Script}.getHashCode(this.f6) : 0);
	h = h * 397 ^ (this.f7 || 0);
	h = h * 397 ^ (this.f8 ? {Script}.getHashCode(this.f8) : 0);
	h = h * 397 ^ (this.f9 ? {Script}.getHashCode(this.f9) : 0);
	h = h * 397 ^ {Script}.getHashCode(this.f10);
	h = h * 397 ^ (this.f11 ? {Script}.getHashCode(this.f11) : 0);
	h = h * 397 ^ this.$2$P1Field;
	h = h * 397 ^ this.p2;
	h = h * 397 ^ (this.$2$E1Field ? {Script}.getHashCode(this.$2$E1Field) : 0);
	return h;
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedGetHashCodeWithOneField() {
			var compilation = Compile(@"
public struct S {
	public readonly double D;
}");
			var getHashCode = FindInstanceMember(compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")), "getHashCode");
			Assert.That(OutputFormatter.Format(getHashCode, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function() {
	return this.d | 0;
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedGetHashCodeWithNoFields() {
			var compilation = Compile(@"
public struct S {
}");
			var getHashCode = FindInstanceMember(compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")), "getHashCode");
			Assert.That(OutputFormatter.Format(getHashCode, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function() {
	return 0;
}".Replace("\r\n", "\n")));
		}

		private JsFunctionDefinitionExpression FindInstanceMember(TypeOOPEmulation emulation, string memberName) {
			JsExpression expr = ((JsVariableDeclarationStatement)emulation.Phases[0].Statements[1]).Declarations[0].Initializer;
			if (expr.NodeType == ExpressionNodeType.Assign)
				expr = ((JsBinaryExpression)expr).Right;
			var ie = (JsInvocationExpression)expr;
			var instanceMembers = (JsObjectLiteralExpression)ie.Arguments[3];
			return (JsFunctionDefinitionExpression)instanceMembers.Values.Single(v => v.Name == memberName).Value;
		}

		[Test]
		public void GeneratedEqualsCalculatesEqualityBasedOnAllInstanceFields() {
			var compilation = Compile(@"
using System;
using System.Runtime.CompilerServices;
public enum E1 {}
[NamedValues] public enum E2 {}
[Mutable] public struct S {
	public static int FS;
	public readonly int F1;
	public readonly int? F2;
	public readonly bool F3;
	public readonly bool? F4;
	public readonly E1 F5;
	public readonly E2 F6;
	public readonly E1? F7;
	public readonly E2? F8;
	public readonly object F9;
	public readonly DateTime F10;
	public readonly DateTime? F11;
	public int P1 { get; set; }
	[IntrinsicProperty] public int P2 { get; set; }
	public event System.Action E1;
	[NonScriptable] public readonly int F12;
	[NonScriptable] public int P3 { get; set; }
	[NonScriptable] public event System.Action E2;
}");
			var equals = FindInstanceMember(compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")), "equals");
			Assert.That(OutputFormatter.Format(equals, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function(o) {
	if (!{Script}.isInstanceOfType(o, $S)) {
		return false;
	}
	return this.f1 === o.f1 && {Script}.equals(this.f2, o.f2) && this.f3 === o.f3 && {Script}.equals(this.f4, o.f4) && this.f5 === o.f5 && {Script}.equals(this.f6, o.f6) && {Script}.equals(this.f7, o.f7) && {Script}.equals(this.f8, o.f8) && {Script}.equals(this.f9, o.f9) && {Script}.equals(this.f10, o.f10) && {Script}.equals(this.f11, o.f11) && this.$2$P1Field === o.$2$P1Field && this.p2 === o.p2 && {Script}.equals(this.$2$E1Field, o.$2$E1Field);
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedEqualsWithOneField() {
			var compilation = Compile(@"
public struct S {
	public readonly double D;
}");
			var equals = FindInstanceMember(compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")), "equals");
			Assert.That(OutputFormatter.Format(equals, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function(o) {
	if (!{Script}.isInstanceOfType(o, $S)) {
		return false;
	}
	return this.d === o.d;
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedEqualsWithNoFields() {
			var compilation = Compile(@"
public struct S {
}");
			var equals = FindInstanceMember(compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")), "equals");
			Assert.That(OutputFormatter.Format(equals, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function(o) {
	return {Script}.isInstanceOfType(o, $S);
}".Replace("\r\n", "\n")));
		}
	}
}
