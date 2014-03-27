using System;
using System.Linq;
using NUnit.Framework;
using Saltarelle.Compiler;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.JSModel.Expressions;
using Saltarelle.Compiler.JSModel.Statements;
using Saltarelle.Compiler.JSModel.TypeSystem;
using Saltarelle.Compiler.Tests;

namespace CoreLib.Tests.OOPEmulatorTests {
	[TestFixture]
	public class ClassTests : OOPEmulatorTestBase {
		[Test]
		public void NonGenericClassWithAllDataWorks() {
			AssertCorrectEmulation(
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
@"////////////////////////////////////////////////////////////////////////////////
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
-
{Script}.initClass($MyClass, $asm, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	}
}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
$MyClass.$ctor1.prototype = $MyClass.$ctor2.prototype = $MyClass.prototype;
", "MyClass");
		}

		[Test]
		public void GenericClassWithIgnoreGenericArgumentsIsRegisteredLikeNonGenericClass() {
			AssertCorrectEmulation(
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
@"////////////////////////////////////////////////////////////////////////////////
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
-
{Script}.initClass($MyClass, $asm, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	}
}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
$MyClass.$ctor1.prototype = $MyClass.$ctor2.prototype = $MyClass.prototype;
", "MyClass");
		}

		[Test]
		public void ClassWithoutInstanceMethodsOmitsMembersVariable() {
			AssertCorrectEmulation(
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
@"////////////////////////////////////////////////////////////////////////////////
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
-
{Script}.initClass($MyClass, $asm, {}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
$MyClass.$ctor1.prototype = $MyClass.$ctor2.prototype = $MyClass.prototype;
", "MyClass");
		}

		[Test]
		public void InheritingBothBaseTypeAndInterfacesWorks() {
			AssertCorrectEmulation(
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
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	{TheBaseClass}.call(this);
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {}, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
", "MyClass");
		}

		[Test]
		public void InheritingOnlyInterfacesPassesNullForTheBaseClassInRegisterClass() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public class MyClass : Interface1, Interface2, Interface3 {
	public MyClass(int x) {
		x = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {}, null, [{Interface1}, {Interface2}, {Interface3}]);
", "MyClass");
		}

		[Test]
		public void InheritingOnlyBaseClassWorks() {
			AssertCorrectEmulation(
@"public class TheBaseClass {}
public class MyClass : TheBaseClass {
	public MyClass(int x) {
		x = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	{TheBaseClass}.call(this);
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {}, {TheBaseClass});
", "MyClass");
		}

		[Test]
		public void ClassWithoutBothBaseClassAndInterfacesOnlyPassTheNameAndMembersToRegisterClass() {
			AssertCorrectEmulation(
@"public class MyClass {
	public MyClass(int x) { x = 0; }
}",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function(x) {
	x = 0;
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {});
", "MyClass");
		}

		[Test]
		public void ClassWithNamespaceWorks() {
			AssertCorrectEmulation(
@"namespace SomeNamespace.InnerNamespace {
	public class MyClass {
		public MyClass(int x) { x = 0; }
	}
}",
@"////////////////////////////////////////////////////////////////////////////////
// SomeNamespace.InnerNamespace.MyClass
var $SomeNamespace_InnerNamespace_MyClass = function(x) {
	x = 0;
};
$SomeNamespace_InnerNamespace_MyClass.__typeName = 'SomeNamespace.InnerNamespace.MyClass';
global.SomeNamespace.InnerNamespace.MyClass = $SomeNamespace_InnerNamespace_MyClass;
-
{Script}.initClass($SomeNamespace_InnerNamespace_MyClass, $asm, {});
", "SomeNamespace.InnerNamespace.MyClass");
		}

		[Test]
		public void InterfaceWorks() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public interface IMyInterface : Interface1, Interface2, Interface3 {
	public void M1();
	public void M2();
}
",
@"////////////////////////////////////////////////////////////////////////////////
// IMyInterface
var $IMyInterface = function() {
};
$IMyInterface.__typeName = 'IMyInterface';
global.IMyInterface = $IMyInterface;
-
{Script}.initInterface($IMyInterface, $asm, { m1: null, m2: null }, [{Interface1}, {Interface2}, {Interface3}]);
", "IMyInterface");
		}

		[Test]
		public void ClassWithoutUnnamedConstructorWorks() {
			AssertCorrectEmulation(
@"using System.Runtime.CompilerServices;
public class MyClass {
	[ScriptName(""someName"")] public MyClass(int x) {}
	public void M1() {}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
$MyClass.someName = function(x) {
};
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {
	m1: function() {
	}
});
$MyClass.someName.prototype = $MyClass.prototype;
", "MyClass");
		}

		[Test]
		public void GenericClassWorks() {
			AssertCorrectEmulation(
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
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "MyClass");
		}

		[Test]
		public void GenericInterfaceWorks() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2<T1, T2> {}
public interface Interface3 {}
public interface IMyInterface<T1, T2> : Interface1, Interface2<T2, int>, Interface3 {
	void M1(int x);
	void M2(int y);
}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "IMyInterface");
		}

		[Test]
		public void GenericInstanceMethodWorks() {
			AssertCorrectEmulation(
@"public class MyClass {
	public void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {
	m1: function(T1, T2) {
		return function(a) {
			var x = 0;
		};
	}
});
", "MyClass");
		}

		[Test]
		public void GenericInstanceMethodWithIgnoreGenericArgumentsIsTreatedLikeNonGenericMethod() {
			AssertCorrectEmulation(
@"public class MyClass {
	[System.Runtime.CompilerServices.IncludeGenericArguments(false)]
	public void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {
	m1: function(a) {
		var x = 0;
	}
});
", "MyClass");
		}

		[Test]
		public void GenericStaticMethodWorks() {
			AssertCorrectEmulation(
@"public class MyClass {
	public static void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
{Script}.initClass($MyClass, $asm, {});
", "MyClass");
		}

		[Test]
		public void GenericStaticMethodWithIgnoreGenericArgumentsIsTreatedLikeNonGenericMethod() {
			AssertCorrectEmulation(
@"public class MyClass {
	[System.Runtime.CompilerServices.IncludeGenericArguments(false)]
	public static void M1<T1, T2>(T1 a) {
		int x = 0;
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = function() {
};
$MyClass.__typeName = 'MyClass';
$MyClass.m1 = function(a) {
	var x = 0;
};
global.MyClass = $MyClass;
-
{Script}.initClass($MyClass, $asm, {});
", "MyClass");
		}

		[Test]
		public void GlobalMethodsAttributeCausesGlobalMethodsToBeGenerated() {
			AssertCorrectEmulation(
@"[System.Runtime.CompilerServices.GlobalMethods]
public static class MyClass {
	public static void S1(int a) { a = 0; }
	public static void S2(int b) { b = 0; }
	static MyClass() {
		int c = 0;
		int d = 0;
	}
}",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
global.s1 = function(a) {
	a = 0;
};
global.s2 = function(b) {
	b = 0;
};
-
", "MyClass");
		}

		[Test]
		public void GlobalMethodsAttributeWithModuleNameCausesModuleGlobalMethodsToBeGeneratedOnTheExportsObject() {
			AssertCorrectEmulation(
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
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
exports.s1 = function(a) {
	a = 0;
};
exports.s2 = function(b) {
	b = 0;
};
-
", "MyClass");
		}

		[Test]
		public void ResourcesAttributeCausesAResourcesClassToBeGenerated() {
			AssertCorrectEmulation(
@"[System.Runtime.CompilerServices.Resources]
public static class MyClass {
	public const string Field1 = ""the value"";
	public const int Field2 = 42;
	public const object Field3 = null;
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = { field1: 'the value', field2: 42, field3: null };
global.MyClass = $MyClass;
-
", "MyClass");
		}

		[Test]
		public void MixinAttributeWorks() {
			AssertCorrectEmulation(
@"[System.Runtime.CompilerServices.Mixin(""$.fn"")]
public static class MyClass {
	public static int Method1(int x) { x = 0; }
	public static int Method2(int y) { y = 0; }
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
$.fn.method1 = function(x) {
	x = 0;
};
$.fn.method2 = function(y) {
	y = 0;
};
-
", "MyClass");
		}

		[Test]
		public void InternalTypesAreNotExported() {
			string program =
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
}";
			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// Outer
var $$Outer = function() {
};
$$Outer.__typeName = '$Outer';
-
{Script}.initClass($$Outer, $asm, {});
", "Outer");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// Outer.Inner
var $$Outer$Inner = function() {
};
$$Outer$Inner.__typeName = '$Outer$Inner';
-
{Script}.initClass($$Outer$Inner, $asm, {});
", "Outer.Inner");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "GenericClass");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// Interface
var $$Interface = function() {
};
$$Interface.__typeName = '$Interface';
-
{Script}.initInterface($$Interface, $asm, {});
", "Interface");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "GenericInterface");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// ResourceClass
var $$ResourceClass = { $field1: 'the value', $field2: 42, $field3: null };
-
", "ResourceClass");
		}

		[Test]
		public void AbstractMethodsWork() {
			AssertCorrectEmulation(
@"public class C { abstract void M(); }
",
@"////////////////////////////////////////////////////////////////////////////////
// C
var $C = function() {
};
$C.__typeName = 'C';
global.C = $C;
-
{Script}.initClass($C, $asm, { $m: null });
", "C");
		}

		[Test]
		public void GenericClassesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public class GenericClass<T1> {}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "GenericClass");
		}

		[Test]
		public void NonGenericClassesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public class NormalClass {}
",
@"////////////////////////////////////////////////////////////////////////////////
// NormalClass
var $NormalClass = function() {
};
$NormalClass.__typeName = 'NormalClass';
exports.NormalClass = $NormalClass;
-
{Script}.initClass($NormalClass, $asm, {});
", "NormalClass");
		}

		[Test]
		public void ResourceClassesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
[System.Runtime.CompilerServices.Resources] public static class ResourceClass {
	public const string Field1 = ""the value"";
	public const int Field2 = 42;
	public const object Field3 = null;
}",
@"////////////////////////////////////////////////////////////////////////////////
// ResourceClass
var $ResourceClass = { field1: 'the value', field2: 42, field3: null };
exports.ResourceClass = $ResourceClass;
-
", "ResourceClass");
		}

		[Test]
		public void GenericInterfacesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public interface GenericInterface<T1> {}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "GenericInterface");
		}

		[Test]
		public void NonGenericInterfacesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public interface Interface {}
",
@"////////////////////////////////////////////////////////////////////////////////
// Interface
var $Interface = function() {
};
$Interface.__typeName = 'Interface';
exports.Interface = $Interface;
-
{Script}.initInterface($Interface, $asm, {});
", "Interface");
		}

		[Test]
		public void SerializableClassAppearsAsBaseClass() {
			AssertCorrectEmulation(@"
using System;
[Serializable] public class B {}
[Serializable] public class D : B {}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
{Script}.initClass($D, $asm, {}, {B});
", "D");
		}

		[Test]
		public void SerializableInterfaceAppearsInInheritanceList() {
			AssertCorrectEmulation(@"
using System;
[Serializable] public interface I1 {}
[Serializable] public interface I2 : I1 {}
[Serializable] public class C : I1 {}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
{Script}.initClass($C, $asm, {}, null, [{I1}]);
", "C");
		}

		[Test]
		public void ImportedClassThatDoesNotObeyTheTypeSystemAppearsAsBaseClass() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported] public class B {}
public class D : B {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
	{B}.call(this);
};
$D.__typeName = 'D';
global.D = $D;
-
{Script}.initClass($D, $asm, {}, {B});
", "D");
		}

		[Test]
		public void ImportedInterfaceThatDoesNotObeyTheTypeSystemDoesNotAppearAsABaseInterfaceOfInterface() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported] public interface I1 {}
public interface I2 {}
public interface I3 : I1, I2 {}
",
@"////////////////////////////////////////////////////////////////////////////////
// I3
var $I3 = function() {
};
$I3.__typeName = 'I3';
global.I3 = $I3;
-
{Script}.initInterface($I3, $asm, {}, [{I2}]);
", "I3");
		}

		[Test]
		public void ImportedInterfaceThatDoesNotObeyTheTypeSystemDoesNotAppearAsABaseInterfaceOfClass() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported] public interface I1 {}
public interface I2 {}
public class D : I1, I2 {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
global.D = $D;
-
{Script}.initClass($D, $asm, {}, null, [{I2}]);
", "D");
		}

		[Test]
		public void ImportedInterfaceThatDoesObeyTheTypeSystemDoesAppearsAsABaseInterfaceOfAnInterface() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported(ObeysTypeSystem = true)] public interface I1 {}
public interface I2 {}
public interface I3 : I1, I2 {}
",
@"////////////////////////////////////////////////////////////////////////////////
// I3
var $I3 = function() {
};
$I3.__typeName = 'I3';
global.I3 = $I3;
-
{Script}.initInterface($I3, $asm, {}, [{I1}, {I2}]);
", "I3");
		}

		[Test]
		public void ImportedInterfaceThatDoesObeyTheTypeSystemDoesAppearsAsABaseInterfaceOfAClass() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported(ObeysTypeSystem = true)] public interface I1 {}
public interface I2 {}
public interface I3 : I1, I2 {}
public class D : I1, I2 {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
global.D = $D;
-
{Script}.initClass($D, $asm, {}, null, [{I1}, {I2}]);
", "D");
		}

		[Test]
		public void ImportedTypeThatDoesNotObeyTheTypeSystemIsReplacedWithObjectForGenericArgumentsInInheritanceListOfInterfaces() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported] public class C {}
public interface I<T1, T2> {}
public interface I2 : I<C, int> {}
",
@"////////////////////////////////////////////////////////////////////////////////
// I2
var $I2 = function() {
};
$I2.__typeName = 'I2';
global.I2 = $I2;
-
{Script}.initInterface($I2, $asm, {}, [{Script}.makeGenericType({I}, [{Object}, {Int32}])]);
", "I2");
		}

		[Test]
		public void ImportedTypeThatDoesNotObeyTheTypeSystemIsReplacedWithObjectForGenericArgumentsInInheritanceListOfClasses() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported] public class C {}
public interface I<T1, T2> {}
public class B<T1, T2> {}
public class D : B<C, int>, I<string, C> {}
public interface I2 : I<C, int> {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
	{Script}.makeGenericType({B}, [{Object}, {Int32}]).call(this);
};
$D.__typeName = 'D';
global.D = $D;
-
{Script}.initClass($D, $asm, {}, {Script}.makeGenericType({B}, [{Object}, {Int32}]), [{Script}.makeGenericType({I}, [{String}, {Object}])]);
", "D");
		}

		[Test]
		public void ImportedTypeThatDoesObeyTheTypeSystemIsUsedInInheritanceListOfInterface() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported(ObeysTypeSystem=true)] public class C {}
public interface I<T1, T2> {}
public interface I2 : I<C, int> {}
",
@"////////////////////////////////////////////////////////////////////////////////
// I2
var $I2 = function() {
};
$I2.__typeName = 'I2';
global.I2 = $I2;
-
{Script}.initInterface($I2, $asm, {}, [{Script}.makeGenericType({I}, [{C}, {Int32}])]);
", "I2");
		}

		[Test]
		public void ImportedTypeThatDoesObeyTheTypeSystemIsUsedInInheritanceListOfClass() {
			AssertCorrectEmulation(@"
using System.Runtime.CompilerServices;
[Imported(ObeysTypeSystem=true)] public class C {}
public interface I<T1, T2> {}
public class B<T1, T2> {}
public class D : B<C, int>, I<string, C> {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
	{Script}.makeGenericType({B}, [{C}, {Int32}]).call(this);
};
$D.__typeName = 'D';
global.D = $D;
-
{Script}.initClass($D, $asm, {}, {Script}.makeGenericType({B}, [{C}, {Int32}]), [{Script}.makeGenericType({I}, [{String}, {C}])]);
", "D");
		}

		[Test]
		public void UsingUnavailableTypeArgumentInInheritanceListIsAnError() {
			var er = new MockErrorReporter();
			EmulateType(@"
using System.Runtime.CompilerServices;
public interface I<T> {}
[IncludeGenericArguments(false)]
public class D1<T> : I<T> {}
", "D1", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type D1")));
		}

		[Test]
		public void ReferenceToGenericClassIsReplacedWithClassVariableForReferenceToSameClass() {
			AssertCorrectEmulation(
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
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "MyClass");
		}

		[Test]
		public void InheritanceFromImportedSerializableClassIsNotRecordedInInheritanceList() {
			AssertCorrectEmulation(
@"[System.Runtime.CompilerServices.Imported, System.Serializable] public class B {}
public class D : B {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = function() {
};
$D.__typeName = 'D';
global.D = $D;
-
{Script}.initClass($D, $asm, {});
", "D");
		}

		[Test]
		public void InheritanceFromImportedSerializableInterfaceIsNotRecordedInInheritanceListOfInterface() {
			AssertCorrectEmulation(
@"using System;
using System.Runtime.CompilerServices;
[Imported, Serializable] public interface I1 {}
[Serializable] public interface I2 {}
[Serializable] public interface I3 : I1, I2 {}
",
@"////////////////////////////////////////////////////////////////////////////////
// I3
var $I3 = function() {
};
$I3.__typeName = 'I3';
global.I3 = $I3;
-
{Script}.initInterface($I3, $asm, {}, [{I2}]);
", "I3");
		}

		[Test]
		public void InheritanceFromImportedSerializableInterfaceIsNotRecordedInInheritanceListOfClass() {
			AssertCorrectEmulation(
@"using System;
using System.Runtime.CompilerServices;
[Imported, Serializable] public interface I1 {}
[Serializable] public interface I2 {}
public class C : I1, I2 {}
",
@"////////////////////////////////////////////////////////////////////////////////
// C
var $C = function() {
};
$C.__typeName = 'C';
global.C = $C;
-
{Script}.initClass($C, $asm, {}, null, [{I2}]);
", "C");
		}

		[Test]
		public void TypeCheckCodeForSerializableTypesWorks() {
			AssertCorrectEmulation(
@"using System;
using System.Runtime.CompilerServices;
[Serializable] public class C {}
[Serializable(TypeCheckCode = ""{this}.X"")] public class D : B {}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
{Script}.initClass($D, $asm, {});
", "D");
		}

		[Test]
		public void TypeCheckCodeForGenericSerializableTypesWorks() {
			AssertCorrectEmulation(
@"using System;
using System.Runtime.CompilerServices;
[Serializable] public class C {}
[Serializable(TypeCheckCode = ""{this}.X == {T}"")] public class D<T> : B {}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
", "D");
		}

		[Test]
		public void UsingUnavailableTypeParameterInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			EmulateType(@"
[System.Serializable(TypeCheckCode = ""{this} == {T}""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", "C1", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type C1")));
		}

		[Test]
		public void ReferencingNonExistentTypeInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			EmulateType(@"
[System.Serializable(TypeCheckCode = ""{this} == {$Some.Nonexistent.Type}""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", "C1", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7157 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("Some.Nonexistent.Type")));
		}

		[Test]
		public void SyntaxErrorInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			EmulateType(@"
[System.Serializable(TypeCheckCode = ""{{this} == 1""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", "C1", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == MessageSeverity.Error && m.Code == 7157 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("syntax error")));
		}

		[Test]
		public void VarianceWorks() {
			AssertCorrectEmulation(
@"public interface IMyInterface<T1, out T2, in T3> {
	void M1(int x);
	void M2(int y);
}
",
@"////////////////////////////////////////////////////////////////////////////////
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
-
-
{Script}.setMetadata($IMyInterface$3, { variance: [0, 1, 2] });
", "IMyInterface");
		}

		[Test]
		public void TheFirstPhaseDoesNotHaveAnyDependencies() {
			var actual = EmulateType(@"
class MyAttribute : System.Attribute {}
class B1 {}
class B2<T> : B {}
interface I1 {}
interface I2<T> : I1 {}
[My] class C : B2<int>, I2<string> {}
", "C");

			Assert.That(actual.Phases[0].DependentOnTypes, Is.Empty);
		}

		[Test]
		public void TheSecondPhaseHasAllBaseTypesAsDependencies() {
			var actual = EmulateType(@"
class MyAttribute : System.Attribute {}
class B1 {}
class B2<T> : B1 {}
interface I1 {}
interface I2<T> : I1 {}
[My] class C : B2<int>, I2<string> {}
", "C");

			Assert.That(actual.Phases[1].DependentOnTypes.Select(t => t.Name), Is.EquivalentTo(new[] { "B1", "B2", "I1", "I2", "Object" }));
		}

		[Test]
		public void TheThirdPhaseDoesNotHaveAnyDependencies() {
			var actual = EmulateType(@"
class MyAttribute : Attribute {}
class B1 {}
class B2<T> : B {}
interface I1 {}
interface I2<T> : I1 {}
[My] class C : B2<int>, I2<string> {}
", "C");

			Assert.That(actual.Phases[2].DependentOnTypes, Is.Empty);
		}

		[Test]
		public void TheTypesStaticInitStatementsAreReturnedAsTheStaticInitStatementsForNormalTypes() {
			var compilation = Compile(@"class C { static C() { int x = 0; int y = 1; } }");
			var statements = compilation.Item2.GetStaticInitStatements((JsClass)compilation.Item3.Single());
			var actual = OutputFormatter.Format(statements, allowIntermediates: true);
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo("var x = 0;\nvar y = 1;\n"));
		}

		[Test]
		public void NothingIsReturnedAsTheStaticInitStatementsForResourceTypes() {
			var compilation = Compile(@"[System.Runtime.CompilerServices.Resources] static class C { const int x = 0; const int y = 0; }");
			var statements = compilation.Item2.GetStaticInitStatements((JsClass)compilation.Item3.Single());
			Assert.That(statements, Is.Empty);
		}

		[Test]
		public void NothingIsReturnedAsTheStaticInitStatementsForGenericTypes() {
			var compilation = Compile(@"class C<T> { static C() { int x = 0; int y = 1; } }");
			var statements = compilation.Item2.GetStaticInitStatements((JsClass)compilation.Item3.Single());
			Assert.That(statements, Is.Empty);
		}

		[Test]
		public void GeneratedGetHashCodeGeneratesHashCodeBasedOnAllInstanceFields() {
			var compilation = Compile(@"
using System;
using System.Runtime.CompilerServices;
public enum E1 {}
[NamedValues] public enum E2 {}
public struct S {
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
	public [NonScriptable] readonly int F12;
}");
			var initClass = compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")).Phases[1].Statements[0];
			var getHashCode = ((JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)initClass).Expression).Arguments[2]).Values.Single(v => v.Name == "getHashCode");
			Assert.That(OutputFormatter.Format(getHashCode.Value, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
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
	return h;
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedGetHashCodeWithOneField() {
			var compilation = Compile(@"
public struct S {
	public readonly double D;
}");
			var initClass = compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")).Phases[1].Statements[0];
			var getHashCode = ((JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)initClass).Expression).Arguments[2]).Values.Single(v => v.Name == "getHashCode");
			Assert.That(OutputFormatter.Format(getHashCode.Value, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function() {
	return this.d | 0;
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedGetHashCodeWithNoFields() {
			var compilation = Compile(@"
public struct S {
}");
			var initClass = compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")).Phases[1].Statements[0];
			var getHashCode = ((JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)initClass).Expression).Arguments[2]).Values.Single(v => v.Name == "getHashCode");
			Assert.That(OutputFormatter.Format(getHashCode.Value, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function() {
	return 0;
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedEqualsCalculatesEqualityBasedOnAllInstanceFields() {
			var compilation = Compile(@"
using System;
using System.Runtime.CompilerServices;
public enum E1 {}
[NamedValues] public enum E2 {}
public struct S {
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
	public [NonScriptable] readonly int F12;
}");
			var initClass = compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")).Phases[1].Statements[0];
			var equals = ((JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)initClass).Expression).Arguments[2]).Values.Single(v => v.Name == "equals");
			Assert.That(OutputFormatter.Format(equals.Value, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function(o) {
	if (!{Script}.isInstanceOfType(o, $S)) {
		return false;
	}
	return this.f1 === o.f1 && {Script}.equals(this.f2, o.f2) && this.f3 === o.f3 && {Script}.equals(this.f4, o.f4) && this.f5 === o.f5 && {Script}.equals(this.f6, o.f6) && {Script}.equals(this.f7, o.f7) && {Script}.equals(this.f8, o.f8) && {Script}.equals(this.f9, o.f9) && {Script}.equals(this.f10, o.f10) && {Script}.equals(this.f11, o.f11);
}".Replace("\r\n", "\n")));
		}

		[Test]
		public void GeneratedEqualsWithOneField() {
			var compilation = Compile(@"
public struct S {
	public readonly double D;
}");
			var initClass = compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")).Phases[1].Statements[0];
			var equals = ((JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)initClass).Expression).Arguments[2]).Values.Single(v => v.Name == "equals");
			Assert.That(OutputFormatter.Format(equals.Value, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
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
			var initClass = compilation.Item2.EmulateType((JsClass)compilation.Item3.Single(t => t.CSharpTypeDefinition.Name == "S")).Phases[1].Statements[0];
			var equals = ((JsObjectLiteralExpression)((JsInvocationExpression)((JsExpressionStatement)initClass).Expression).Arguments[2]).Values.Single(v => v.Name == "equals");
			Assert.That(OutputFormatter.Format(equals.Value, allowIntermediates: true).Replace("\r\n", "\n"), Is.EqualTo(
@"function(o) {
	return {Script}.isInstanceOfType(o, $S);
}".Replace("\r\n", "\n")));
		}
	}
}
