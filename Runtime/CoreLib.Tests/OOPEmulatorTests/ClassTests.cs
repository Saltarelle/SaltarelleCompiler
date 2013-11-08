using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.ScriptSemantics;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class ClassTests : OOPEmulatorTestBase {
		[Test]
		public void NonGenericClassWithAllDataWorks() {
			AssertCorrect(
@"public class TheBaseClass {}
public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public class MyClass : TheBaseClass, Interface1, Interface2, Interface3 {
	public MyClass() { int a = 0; }
	public MyClass(int b) { b = 0; }
	public MyClass(string c) { c = null; }
	public void M1(int d) { d = 0; }
	public void M2(int e) { e = 0; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyClass() {
		int h = 0;
		int i = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
	{TheBaseClass}.call(this);
	var a = 0;
};
$MyClass.__typeName = 'MyClass';
$MyClass.$ctor1 = function(b) {
	{TheBaseClass}.call(this);
	b = 0;
};
$MyClass.$ctor2 = function(c) {
	{TheBaseClass}.call(this);
	c = null;
};
$MyClass.s1 = function(f) {
	f = 0;
};
$MyClass.s2 = function(g) {
	g = 0;
};
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	}
}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
$MyClass.$ctor1.prototype = $MyClass.$ctor2.prototype = $MyClass.prototype;
var h = 0;
var i = 0;
", new[] { "MyClass" });
		}

		[Test]
		public void GenericClassWithIgnoreGenericArgumentsIsRegisteredLikeNonGenericClass() {
			AssertCorrect(
@"public class TheBaseClass {}
public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
[System.Runtime.CompilerServices.IncludeGenericArguments(false)]
public class MyClass<T> : TheBaseClass, Interface1, Interface2, Interface3 {
	public MyClass() { int a = 0; }
	public MyClass(int b) { b = 0; }
	public MyClass(string c) { c = null; }
	public void M1(int d) { d = 0; }
	public void M2(int e) { e = 0; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyClass() {
		int h = 0;
		int i = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
	{TheBaseClass}.call(this);
	var a = 0;
};
$MyClass.__typeName = 'MyClass';
$MyClass.$ctor1 = function(b) {
	{TheBaseClass}.call(this);
	b = 0;
};
$MyClass.$ctor2 = function(c) {
	{TheBaseClass}.call(this);
	c = null;
};
$MyClass.s1 = function(f) {
	f = 0;
};
$MyClass.s2 = function(g) {
	g = 0;
};
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	}
}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
$MyClass.$ctor1.prototype = $MyClass.$ctor2.prototype = $MyClass.prototype;
var h = 0;
var i = 0;
", new[] { "MyClass" });
		}

		[Test]
		public void ClassWithoutInstanceMethodsOmitsMembersVariable() {
			AssertCorrect(
@"public class TheBaseClass {}
public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public class MyClass : TheBaseClass, Interface1, Interface2, Interface3 {
	public MyClass() { int a = 0; }
	public MyClass(int b) { b = 0; }
	public MyClass(string c) { c = null; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyClass() {
		int h = 0;
		int i = 0;
	}

}",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
	{TheBaseClass}.call(this);
	var a = 0;
};
$MyClass.__typeName = 'MyClass';
$MyClass.$ctor1 = function(b) {
	{TheBaseClass}.call(this);
	b = 0;
};
$MyClass.$ctor2 = function(c) {
	{TheBaseClass}.call(this);
	c = null;
};
$MyClass.s1 = function(f) {
	f = 0;
};
$MyClass.s2 = function(g) {
	g = 0;
};
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
$MyClass.$ctor1.prototype = $MyClass.$ctor2.prototype = $MyClass.prototype;
var h = 0;
var i = 0;
", new[] { "MyClass" });
		}

		[Test]
		public void InheritingBothBaseTypeAndInterfacesWorks() {
			AssertCorrect(
@"public class TheBaseClass {}
public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public class MyClass : TheBaseClass, Interface1, Interface2, Interface3 {
	public MyClass(int x) {
		x = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	{TheBaseClass}.call(this);
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
", new[] { "MyClass" });
		}

		[Test]
		public void InheritingOnlyInterfacesPassesNullForTheBaseClassInRegisterClass() {
			AssertCorrect(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public class MyClass : Interface1, Interface2, Interface3 {
	public MyClass(int x) {
		x = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {}, null, [{Interface1}, {Interface2}, {Interface3}]);
", new[] { "MyClass" });
		}

		[Test]
		public void InheritingOnlyBaseClassWorks() {
			AssertCorrect(
@"public class TheBaseClass {}
public class MyClass : TheBaseClass {
	public MyClass(int x) {
		x = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	{TheBaseClass}.call(this);
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {}, {TheBaseClass});
", new[] { "MyClass" });
		}

		[Test]
		public void ClassWithoutBothBaseClassAndInterfacesOnlyPassTheNameAndMembersToRegisterClass() {
			AssertCorrect(
@"public class MyClass {
	public MyClass(int x) { x = 0; }
}",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {});
", new[] { "MyClass" });
		}

		[Test]
		public void ClassWithNamespaceWorks() {
			AssertCorrect(
@"namespace SomeNamespace.InnerNamespace {
	public class MyClass {
		public MyClass(int x) { x = 0; }
	}
}",
@"global.SomeNamespace = global.SomeNamespace || {};
global.SomeNamespace.InnerNamespace = global.SomeNamespace.InnerNamespace || {};
{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(x) {
	x = 0;
};
$SomeNamespace_InnerNamespace_MyClass.__typeName = 'SomeNamespace.InnerNamespace.MyClass';
global.SomeNamespace.InnerNamespace.MyClass = $SomeNamespace_InnerNamespace_MyClass;
{Script}.initClass($SomeNamespace_InnerNamespace_MyClass, $asm, {});
");
		}

		[Test]
		public void InterfaceWorks() {
			AssertCorrect(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public interface IMyInterface : Interface1, Interface2, Interface3 {
	public void M1();
	public void M2();
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// IMyInterface
var $IMyInterface = function() {
};
$IMyInterface.__typeName = 'IMyInterface';
global.IMyInterface = $IMyInterface;
{Script}.initInterface($IMyInterface, $asm, { m1: null, m2: null }, [{Interface1}, {Interface2}, {Interface3}]);
", new[] { "IMyInterface" });
		}

		[Test]
		public void ClassWithoutUnnamedConstructorWorks() {
			AssertCorrect(
@"using System.Runtime.CompilerServices;
public class MyClass {
	[ScriptName(""someName"")] public MyClass(int x) {}
	public void M1() {}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
$MyClass.someName = function(x) {
};
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {
	m1: function() {
	}
});
$MyClass.someName.prototype = $MyClass.prototype;
", new[] { "MyClass" });
		}

		[Test]
		public void GenericClassWorks() {
			AssertCorrect(
@"public class TheBaseClass<T> {}
public interface Interface1 {}
public interface Interface2<T1, T2> {}
public interface Interface3 {}
[System.Runtime.CompilerServices.IncludeGenericArguments(true)]
public class MyClass<T1, T2> : TheBaseClass<T1>, Interface1, Interface2<T2, int>, Interface3 {
	public MyClass() { int a = 0; }
	public MyClass(int b) { b = 0; }
	public MyClass(string c) { c = null; }
	public void M1(int d) { d = 0; }
	public void M2(int e) { e = 0; }
	public static void S1(int f) { f = 0; }
	public static void S2(int g) { g = 0; }
	static MyClass() {
		int h = 0;
		int i = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass$2 = function(T1, T2) {
	var $type = function() {
		{Script}.makeGenericType({TheBaseClass}, [T1]).call(this);
		var a = 0;
	};
	$type.$ctor1 = function(b) {
		{Script}.makeGenericType({TheBaseClass}, [T1]).call(this);
		b = 0;
	};
	$type.$ctor2 = function(c) {
		{Script}.makeGenericType({TheBaseClass}, [T1]).call(this);
		c = null;
	};
	$type.s1 = function(f) {
		f = 0;
	};
	$type.s2 = function(g) {
		g = 0;
	};
	{Script}.registerGenericClassInstance($type, {MyClass}, [T1, T2], {
		m1: function(d) {
			d = 0;
		},
		m2: function(e) {
			e = 0;
		}
	}, function() {
		return {Script}.makeGenericType({TheBaseClass}, [T1]);
	}, function() {
		return [{Interface1}, {Script}.makeGenericType({Interface2}, [T2, {Int32}]), {Interface3}];
	});
	$type.$ctor1.prototype = $type.$ctor2.prototype = $type.prototype;
	var h = 0;
	var i = 0;
	return $type;
};
$MyClass$2.__typeName = 'MyClass$2';
{Script}.initGenericClass($MyClass$2, $asm, 2);
global.MyClass$2 = $MyClass$2;
", new[] { "MyClass" });
		}

		[Test]
		public void GenericInterfaceWorks() {
			AssertCorrect(
@"public interface Interface1 {}
public interface Interface2<T1, T2> {}
public interface Interface3 {}
public interface IMyInterface<T1, T2> : Interface1, Interface2<T2, int>, Interface3 {
	void M1(int x);
	void M2(int y);
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// IMyInterface
var $IMyInterface$2 = function(T1, T2) {
	var $type = function() {
	};
	{Script}.registerGenericInterfaceInstance($type, {IMyInterface}, [T1, T2], { m1: null, m2: null }, function() {
		return [{Interface1}, {Script}.makeGenericType({Interface2}, [T2, {Int32}]), {Interface3}];
	});
	return $type;
};
$IMyInterface$2.__typeName = 'IMyInterface$2';
{Script}.initGenericInterface($IMyInterface$2, $asm, 2);
global.IMyInterface$2 = $IMyInterface$2;
", new[] { "IMyInterface" });
		}

		[Test]
		public void GenericInstanceMethodWorks() {
			AssertCorrect(
@"public class MyClass {
	public void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {
	m1: function(T1, T2) {
		return function(a) {
			var x = 0;
		};
	}
});
", new[] { "MyClass" });
		}

		[Test]
		public void GenericInstanceMethodWithIgnoreGenericArgumentsIsTreatedLikeNonGenericMethod() {
			AssertCorrect(
@"public class MyClass {
	[System.Runtime.CompilerServices.IncludeGenericArguments(false)]
	public void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {
	m1: function(a) {
		var x = 0;
	}
});
", new[] { "MyClass" });
		}

		[Test]
		public void GenericStaticMethodWorks() {
			AssertCorrect(
@"public class MyClass {
	public static void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
$MyClass.m1 = function(T1, T2) {
	return function(a) {
		var x = 0;
	};
};
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {});
", new[] { "MyClass" });
		}

		[Test]
		public void GenericStaticMethodWithIgnoreGenericArgumentsIsTreatedLikeNonGenericMethod() {
			AssertCorrect(
@"public class MyClass {
	[System.Runtime.CompilerServices.IncludeGenericArguments(false)]
	public static void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
$MyClass.m1 = function(a) {
	var x = 0;
};
global.MyClass = $MyClass;
{Script}.initClass($MyClass, $asm, {});
", new[] { "MyClass" });
		}

		[Test]
		public void GlobalMethodsAttributeCausesGlobalMethodsToBeGenerated() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.GlobalMethods]
public static class MyClass {
	public static void S1(int a) { a = 0; }
	public static void S2(int b) { b = 0; }
	static MyClass() {
		int c = 0;
		int d = 0;
	}
}",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
global.s1 = function(a) {
	a = 0;
};
global.s2 = function(b) {
	b = 0;
};
var c = 0;
var d = 0;
", new[] { "MyClass" });
		}

		[Test]
		public void GlobalMethodsAttributeWithModuleNameCausesModuleGlobalMethodsToBeGeneratedOnTheExportsObject() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.GlobalMethods]
[System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public static class MyClass {
	public static void S1(int a) { a = 0; }
	public static void S2(int b) { b = 0; }
	static MyClass() {
		int c = 0;
		int d = 0;
	}
}",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
exports.s1 = function(a) {
	a = 0;
};
exports.s2 = function(b) {
	b = 0;
};
var c = 0;
var d = 0;
", new[] { "MyClass" });
		}

		[Test]
		public void ResourcesAttributeCausesAResourcesClassToBeGenerated() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.Resources]
public static class MyClass {
	public const string Field1 = ""the value"";
	public const int Field2 = 42;
	public const object Field3 = null;
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = { field1: 'the value', field2: 42, field3: null };
global.MyClass = $MyClass;
", new[] { "MyClass" });
		}

		[Test]
		public void MixinAttributeWorks() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.Mixin(""$.fn"")]
public static class MyClass {
	public static int Method1(int x) { x = 0; }
	public static int Method2(int y) { y = 0; }
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
$.fn.method1 = function(x) {
	x = 0;
};
$.fn.method2 = function(y) {
	y = 0;
};
", new[] { "MyClass" });
		}

		[Test]
		public void InternalTypesAreNotExported() {
			AssertCorrect(
@"internal class Outer {
	public class Inner {
	}
}
internal class GenericClass<T1> {}
internal interface Interface {}
internal interface GenericInterface<T1> {}
[System.Runtime.CompilerServices.Resources] internal static class ResourceClass {
	public const string Field1 = ""the value"";
	public const int Field2 = 42;
	public const object Field3 = null;
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// GenericClass
var $$GenericClass$1 = function(T1) {
	var $type = function() {
	};
	{Script}.registerGenericClassInstance($type, {GenericClass}, [T1], {}, function() {
		return null;
	}, function() {
		return [];
	});
	return $type;
};
$$GenericClass$1.__typeName = '$GenericClass$1';
{Script}.initGenericClass($$GenericClass$1, $asm, 1);
////////////////////////////////////////////////////////////////////////////////
// GenericInterface
var $$GenericInterface$1 = function(T1) {
	var $type = function() {
	};
	{Script}.registerGenericInterfaceInstance($type, {GenericInterface}, [T1], {}, function() {
		return [];
	});
	return $type;
};
$$GenericInterface$1.__typeName = '$GenericInterface$1';
{Script}.initGenericInterface($$GenericInterface$1, $asm, 1);
////////////////////////////////////////////////////////////////////////////////
// Interface
var $$Interface = function() {
};
$$Interface.__typeName = '$Interface';
////////////////////////////////////////////////////////////////////////////////
// Outer
var $$Outer = function() {
};
$$Outer.__typeName = '$Outer';
////////////////////////////////////////////////////////////////////////////////
// Outer.Inner
var $$Outer$Inner = function() {
};
$$Outer$Inner.__typeName = '$Outer$Inner';
////////////////////////////////////////////////////////////////////////////////
// ResourceClass
var $$ResourceClass = { $field1: 'the value', $field2: 42, $field3: null };
{Script}.initInterface($$Interface, $asm, {});
{Script}.initClass($$Outer, $asm, {});
{Script}.initClass($$Outer$Inner, $asm, {});
");
		}

		[Test]
		public void AbstractMethodsWork() {
			AssertCorrect(
@"public class C { abstract void M(); }
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// C
var $C = function() {
};
$C.__typeName = 'C';
global.C = $C;
{Script}.initClass($C, $asm, { $m: null });
");
		}

		[Test]
		public void ClassesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrect(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public class GenericClass<T1> {}
public class NormalClass {}
public interface Interface {}
public interface GenericInterface<T1> {}
[System.Runtime.CompilerServices.Resources] public static class ResourceClass {
	public const string Field1 = ""the value"";
	public const int Field2 = 42;
	public const object Field3 = null;
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// GenericClass
var $GenericClass$1 = function(T1) {
	var $type = function() {
	};
	{Script}.registerGenericClassInstance($type, {GenericClass}, [T1], {}, function() {
		return null;
	}, function() {
		return [];
	});
	return $type;
};
$GenericClass$1.__typeName = 'GenericClass$1';
{Script}.initGenericClass($GenericClass$1, $asm, 1);
exports.GenericClass$1 = $GenericClass$1;
////////////////////////////////////////////////////////////////////////////////
// GenericInterface
var $GenericInterface$1 = function(T1) {
	var $type = function() {
	};
	{Script}.registerGenericInterfaceInstance($type, {GenericInterface}, [T1], {}, function() {
		return [];
	});
	return $type;
};
$GenericInterface$1.__typeName = 'GenericInterface$1';
{Script}.initGenericInterface($GenericInterface$1, $asm, 1);
exports.GenericInterface$1 = $GenericInterface$1;
////////////////////////////////////////////////////////////////////////////////
// Interface
var $Interface = function() {
};
$Interface.__typeName = 'Interface';
exports.Interface = $Interface;
////////////////////////////////////////////////////////////////////////////////
// NormalClass
var $NormalClass = function() {
};
$NormalClass.__typeName = 'NormalClass';
exports.NormalClass = $NormalClass;
////////////////////////////////////////////////////////////////////////////////
// ResourceClass
var $ResourceClass = { field1: 'the value', field2: 42, field3: null };
exports.ResourceClass = $ResourceClass;
{Script}.initInterface($Interface, $asm, {});
{Script}.initClass($NormalClass, $asm, {});
");
		}

		[Test]
		public void SerializableClassAppearsAsBaseClass() {
			AssertCorrect(@"
using System;
[Serializable] public class B {}
[Serializable] public class D : B {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
$D.createInstance = function() {
	return {D}.$ctor();
};
$D.$ctor = function() {
	var $this = {B}.$ctor();
	return $this;
};
global.D = $D;
{Script}.initClass($D, $asm, {}, {B});
", new[] { "D" });
		}

		[Test]
		public void SerializableInterfaceAppearsInInheritanceList() {
			AssertCorrect(@"
using System;
[Serializable] public interface I1 {}
[Serializable] public interface I2 : I1 {}
[Serializable] public class C : I1 {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// C
var $C = function() {
};
$C.__typeName = 'C';
$C.createInstance = function() {
	return {C}.$ctor();
};
$C.$ctor = function() {
	var $this = {};
	return $this;
};
global.C = $C;
////////////////////////////////////////////////////////////////////////////////
// I1
var $I1 = function() {
};
$I1.__typeName = 'I1';
global.I1 = $I1;
////////////////////////////////////////////////////////////////////////////////
// I2
var $I2 = function() {
};
$I2.__typeName = 'I2';
global.I2 = $I2;
{Script}.initInterface($I1, $asm, {});
{Script}.initClass($C, $asm, {}, null, [{I1}]);
{Script}.initInterface($I2, $asm, {}, [{I1}]);
");
		}

		[Test]
		public void ImportedClassThatDoesNotObeyTheTypeSystemAppearsAsBaseClass() {
			AssertCorrect(@"
using System.Runtime.CompilerServices;
[Imported] public class B {}
public class D : B {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
	{B}.call(this);
};
$D.__typeName = 'D';
global.D = $D;
{Script}.initClass($D, $asm, {}, {B});
", new [] { "D", "I3" });
		}

		[Test]
		public void ImportedInterfaceThatDoesNotObeyTheTypeSystemDoesNotAppearAsABaseInterface() {
			AssertCorrect(@"
using System.Runtime.CompilerServices;
[Imported] public interface I1 {}
public interface I2 {}
public interface I3 : I1, I2 {}
public class D : I1, I2 {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
global.D = $D;
////////////////////////////////////////////////////////////////////////////////
// I3
var $I3 = function() {
};
$I3.__typeName = 'I3';
global.I3 = $I3;
{Script}.initClass($D, $asm, {}, null, [{I2}]);
{Script}.initInterface($I3, $asm, {}, [{I2}]);
", new [] { "D", "I3" });
		}

		[Test]
		public void ImportedInterfaceThatDoesObeyTheTypeSystemDoesAppearsAsABaseInterface() {
			AssertCorrect(@"
using System.Runtime.CompilerServices;
[Imported(ObeysTypeSystem = true)] public interface I1 {}
public interface I2 {}
public interface I3 : I1, I2 {}
public class D : I1, I2 {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
global.D = $D;
////////////////////////////////////////////////////////////////////////////////
// I3
var $I3 = function() {
};
$I3.__typeName = 'I3';
global.I3 = $I3;
{Script}.initClass($D, $asm, {}, null, [{I1}, {I2}]);
{Script}.initInterface($I3, $asm, {}, [{I1}, {I2}]);
", new[] { "D", "I3" });
		}

		[Test]
		public void ImportedTypeThatDoesNotObeyTheTypeSystemIsReplacedWithObjectForGenericArgumentsInInheritanceList() {
			AssertCorrect(@"
using System.Runtime.CompilerServices;
[Imported] public class C {}
public interface I<T1, T2> {}
public class B<T1, T2> {}
public class D : B<C, int>, I<string, C> {}
public interface I2 : I<C, int> {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
	{Script}.makeGenericType({B}, [{Object}, {Int32}]).call(this);
};
$D.__typeName = 'D';
global.D = $D;
////////////////////////////////////////////////////////////////////////////////
// I2
var $I2 = function() {
};
$I2.__typeName = 'I2';
global.I2 = $I2;
{Script}.initClass($D, $asm, {}, {Script}.makeGenericType({B}, [{Object}, {Int32}]), [{Script}.makeGenericType({I}, [{String}, {Object}])]);
{Script}.initInterface($I2, $asm, {}, [{Script}.makeGenericType({I}, [{Object}, {Int32}])]);
", new[] { "D", "I2" });
		}

		[Test]
		public void ImportedTypeThatDoesObeyTheTypeSystemIsUsedInInheritanceList() {
			AssertCorrect(@"
using System.Runtime.CompilerServices;
[Imported(ObeysTypeSystem=true)] public class C {}
public interface I<T1, T2> {}
public class B<T1, T2> {}
public class D : B<C, int>, I<string, C> {}
public interface I2 : I<C, int> {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
	{Script}.makeGenericType({B}, [{C}, {Int32}]).call(this);
};
$D.__typeName = 'D';
global.D = $D;
////////////////////////////////////////////////////////////////////////////////
// I2
var $I2 = function() {
};
$I2.__typeName = 'I2';
global.I2 = $I2;
{Script}.initClass($D, $asm, {}, {Script}.makeGenericType({B}, [{C}, {Int32}]), [{Script}.makeGenericType({I}, [{String}, {C}])]);
{Script}.initInterface($I2, $asm, {}, [{Script}.makeGenericType({I}, [{C}, {Int32}])]);
", new[] { "D", "I2" });
		}

		[Test]
		public void UsingUnavailableTypeArgumentInInheritanceListIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
using System.Runtime.CompilerServices;
public interface I<T> {}
[IncludeGenericArguments(false)]
public class D1<T> : I<T> {}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type D1")));
		}

		[Test]
		public void ReferenceToGenericClassIsReplacedWithClassVariableForReferenceToSameClass() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.IncludeGenericArguments(true)]
public class OtherClass<T1, T2> {
	public static void F() {}
}
[System.Runtime.CompilerServices.IncludeGenericArguments(true)]
public class MyClass<T1, T2> {
	public static void F() {}
	public MyClass() {
		F();
		MyClass<T1, T2>.F();
		MyClass<int, string>.F();
		MyClass<T2, T1>.F();
		OtherClass<T1, T2>.F();
	}
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass$2 = function(T1, T2) {
	var $type = function() {
		$type.f();
		$type.f();
		{Script}.makeGenericType({MyClass}, [{Int32}, {String}]).f();
		{Script}.makeGenericType({MyClass}, [T2, T1]).f();
		{Script}.makeGenericType({OtherClass}, [T1, T2]).f();
	};
	$type.f = function() {
	};
	{Script}.registerGenericClassInstance($type, {MyClass}, [T1, T2], {}, function() {
		return null;
	}, function() {
		return [];
	});
	return $type;
};
$MyClass$2.__typeName = 'MyClass$2';
{Script}.initGenericClass($MyClass$2, $asm, 2);
global.MyClass$2 = $MyClass$2;
", new[] { "MyClass" });
		}

		[Test]
		public void InheritanceFromImportedSerializableClassIsNotRecordedInInheritanceList() {
			AssertCorrect(
@"[System.Runtime.CompilerServices.Imported, System.Serializable] public class B {}
public class D : B {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
global.D = $D;
{Script}.initClass($D, $asm, {});
", new[] { "D" });
		}

		[Test]
		public void InheritanceFromImportedSerializableInterfaceIsNotRecordedInInheritanceList() {
			AssertCorrect(
@"using System;
using System.Runtime.CompilerServices;
[Imported, Serializable] public interface I1 {}
[Serializable] public interface I2 {}
[Serializable] public interface I3 : I1, I2 {}
public class C : I1, I2 {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// C
var $C = function() {
};
$C.__typeName = 'C';
global.C = $C;
////////////////////////////////////////////////////////////////////////////////
// I3
var $I3 = function() {
};
$I3.__typeName = 'I3';
global.I3 = $I3;
{Script}.initClass($C, $asm, {}, null, [{I2}]);
{Script}.initInterface($I3, $asm, {}, [{I2}]);
", new[] { "C", "I3" });
		}

		[Test]
		public void TypeCheckCodeForSerializableTypesWorks() {
			AssertCorrect(
@"using System;
using System.Runtime.CompilerServices;
[Serializable] public class C {}
[Serializable(TypeCheckCode = ""{this}.X"")] public class D : B {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
$D.createInstance = function() {
	return {D}.$ctor();
};
$D.$ctor = function() {
	var $this = {};
	return $this;
};
$D.isInstanceOfType = function(obj) {
	return obj.X;
};
global.D = $D;
{Script}.initClass($D, $asm, {});
", new[] { "D" });
		}

		[Test]
		public void TypeCheckCodeForGenericSerializableTypesWorks() {
			AssertCorrect(
@"using System;
using System.Runtime.CompilerServices;
[Serializable] public class C {}
[Serializable(TypeCheckCode = ""{this}.X == {T}"")] public class D<T> : B {}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// D
var $D$1 = function(T) {
	var $type = function() {
	};
	$type.createInstance = function() {
		return $type.$ctor();
	};
	$type.$ctor = function() {
		var $this = {};
		return $this;
	};
	$type.isInstanceOfType = function(obj) {
		return obj.X == T;
	};
	{Script}.registerGenericClassInstance($type, {D}, [T], {}, function() {
		return null;
	}, function() {
		return [];
	});
	return $type;
};
$D$1.__typeName = 'D$1';
{Script}.initGenericClass($D$1, $asm, 1);
global.D$1 = $D$1;
", new[] { "D" });
		}

		[Test]
		public void UsingUnavailableTypeParameterInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
[System.Serializable(TypeCheckCode = ""{this} == {T}""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type C1")));
		}

		[Test]
		public void ReferencingNonExistentTypeInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
[System.Serializable(TypeCheckCode = ""{this} == {$Some.Nonexistent.Type}""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7157 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("Some.Nonexistent.Type")));
		}

		[Test]
		public void SyntaxErrorInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			Process(@"
[System.Serializable(TypeCheckCode = ""{{this} == 1""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7157 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("syntax error")));
		}

		[Test]
		public void VarianceWorks() {
			AssertCorrect(
@"public interface IMyInterface<T1, out T2, in T3> {
	void M1(int x);
	void M2(int y);
}
",
@"{Script}.initAssembly($asm, 'x');
////////////////////////////////////////////////////////////////////////////////
// IMyInterface
var $IMyInterface$3 = function(T1, T2, T3) {
	var $type = function() {
	};
	{Script}.registerGenericInterfaceInstance($type, {IMyInterface}, [T1, T2, T3], { m1: null, m2: null }, function() {
		return [];
	});
	{Script}.setMetadata($type, { variance: [0, 1, 2] });
	return $type;
};
$IMyInterface$3.__typeName = 'IMyInterface$3';
{Script}.initGenericInterface($IMyInterface$3, $asm, 3);
global.IMyInterface$3 = $IMyInterface$3;
{Script}.setMetadata($IMyInterface$3, { variance: [0, 1, 2] });
", new[] { "IMyInterface" });
		}
	}
}
