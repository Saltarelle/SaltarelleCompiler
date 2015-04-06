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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
	{TheBaseClass}.call(this);
	var a = 0;
}, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	}
}, {
	$ctor1: function(b) {
		{TheBaseClass}.call(this);
		b = 0;
	},
	$ctor2: function(c) {
		{TheBaseClass}.call(this);
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
{Script}.initClass($MyClass, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
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
// MyClass<T>
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
	{TheBaseClass}.call(this);
	var a = 0;
}, {
	m1: function(d) {
		d = 0;
	},
	m2: function(e) {
		e = 0;
	}
}, {
	$ctor1: function(b) {
		{TheBaseClass}.call(this);
		b = 0;
	},
	$ctor2: function(c) {
		{TheBaseClass}.call(this);
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
{Script}.initClass($MyClass, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
$MyClass.$ctor1.prototype = $MyClass.$ctor2.prototype = $MyClass.prototype;
", "MyClass<T>");
		}

		[Test]
		public void ClassWithoutInstanceMethodsWorks() {
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
	{TheBaseClass}.call(this);
	var a = 0;
}, null, {
	$ctor1: function(b) {
		{TheBaseClass}.call(this);
		b = 0;
	},
	$ctor2: function(c) {
		{TheBaseClass}.call(this);
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
{Script}.initClass($MyClass, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function(x) {
	{TheBaseClass}.call(this);
	x = 0;
});
-
{Script}.initClass($MyClass, {TheBaseClass}, [{Interface1}, {Interface2}, {Interface3}]);
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function(x) {
	x = 0;
});
-
{Script}.initClass($MyClass, null, [{Interface1}, {Interface2}, {Interface3}]);
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function(x) {
	{TheBaseClass}.call(this);
	x = 0;
});
-
{Script}.initClass($MyClass, {TheBaseClass});
", "MyClass");
		}

		[Test]
		public void ClassWithoutBothBaseClassAndInterfacesOnlyPassTheNameAndMembersToInitClass() {
			AssertCorrectEmulation(
@"public class MyClass {
	public MyClass(int x) { x = 0; }
}",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function(x) {
	x = 0;
});
-
{Script}.initClass($MyClass);
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
var $SomeNamespace_InnerNamespace_MyClass = global.SomeNamespace.InnerNamespace.MyClass = {Script}.mkType($asm, 'SomeNamespace.InnerNamespace.MyClass', function(x) {
	x = 0;
});
-
{Script}.initClass($SomeNamespace_InnerNamespace_MyClass);
", "SomeNamespace.InnerNamespace.MyClass");
		}

		[Test]
		public void InterfaceWorks() {
			AssertCorrectEmulation(
@"public interface Interface1 {}
public interface Interface2 {}
public interface Interface3 {}
public interface IMyInterface : Interface1, Interface2, Interface3 {
	void M1();
	void M2();
}
",
@"////////////////////////////////////////////////////////////////////////////////
// IMyInterface
var $IMyInterface = global.IMyInterface = {Script}.mkType($asm, 'IMyInterface');
-
{Script}.initInterface($IMyInterface, [{Interface1}, {Interface2}, {Interface3}]);
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', null, {
	m1: function() {
	}
}, {
	someName: function(x) {
	}
});
-
{Script}.initClass($MyClass);
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
// MyClass<T1, T2>
var $MyClass$2 = global.MyClass$2 = {Script}.mkType($asm, 'MyClass$2', function(T1, T2) {
	var $type = {Script}.registerGenericClassInstance({MyClass}, [T1, T2], function() {
		{Script}.makeGenericType({TheBaseClass}, [T1]).call(this);
		var a = 0;
	}, {
		m1: function(d) {
			d = 0;
		},
		m2: function(e) {
			e = 0;
		}
	}, {
		$ctor1: function(b) {
			{Script}.makeGenericType({TheBaseClass}, [T1]).call(this);
			b = 0;
		},
		$ctor2: function(c) {
			{Script}.makeGenericType({TheBaseClass}, [T1]).call(this);
			c = null;
		},
		s1: function(f) {
			f = 0;
		},
		s2: function(g) {
			g = 0;
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
});
{Script}.initGenericClass($MyClass$2, 2);
-
", "MyClass<T1, T2>");
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
// IMyInterface<T1, T2>
var $IMyInterface$2 = global.IMyInterface$2 = {Script}.mkType($asm, 'IMyInterface$2', function(T1, T2) {
	var $type = {Script}.registerGenericInterfaceInstance({IMyInterface}, [T1, T2], function() {
		return [{Interface1}, {Script}.makeGenericType({Interface2}, [T2, {Int32}]), {Interface3}];
	});
	return $type;
});
{Script}.initGenericInterface($IMyInterface$2, 2);
-
", "IMyInterface<T1, T2>");
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
}, {
	m1: function(T1, T2) {
		return function(a) {
			var x = 0;
		};
	}
});
-
{Script}.initClass($MyClass);
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
}, {
	m1: function(a) {
		var x = 0;
	}
});
-
{Script}.initClass($MyClass);
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
}, null, {
	m1: function(T1, T2) {
		return function(a) {
			var x = 0;
		};
	}
});
-
{Script}.initClass($MyClass);
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
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
}, null, {
	m1: function(a) {
		var x = 0;
	}
});
-
{Script}.initClass($MyClass);
", "MyClass");
		}

		[Test]
		public void PropertiesWithGeneratedAccessorsWork() {
			AssertCorrectEmulation(
@"public class MyClass {
	private int f1, f2, f3;
	[System.Runtime.CompilerServices.IntrinsicProperty]
	public int P1 {
		get { return f1; } set { f1 = value; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public int P2 {
		get { return f2; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public int P3 {
		set { f3 = value; }
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
	this.$f1 = 0;
	this.$f2 = 0;
	this.$f3 = 0;
}, {
	get p1() {
		return this.$f1;
	},
	set p1(value) {
		this.$f1 = value;
	},
	get p2() {
		return this.$f2;
	},
	set p3(value) {
		this.$f3 = value;
	}
});
-
{Script}.initClass($MyClass);
", "MyClass");
		}

		[Test]
		public void StaticPropertiesWithGeneratedAccessorsWork() {
			AssertCorrectEmulation(
@"public class MyClass {
	private static int f1, f2, f3;
	[System.Runtime.CompilerServices.IntrinsicProperty]
	public static int P1 {
		get { return f1; } set { f1 = value; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public static int P2 {
		get { return f2; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public static int P3 {
		set { f3 = value; }
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass
var $MyClass = global.MyClass = {Script}.mkType($asm, 'MyClass', function() {
}, null, {
	get p1() {
		return {MyClass}.$f1;
	},
	set p1(value) {
		{MyClass}.$f1 = value;
	},
	get p2() {
		return {MyClass}.$f2;
	},
	set p3(value) {
		{MyClass}.$f3 = value;
	}
});
-
{Script}.initClass($MyClass);
", "MyClass");
		}

		[Test]
		public void PropertiesWithGeneratedAccessorsWorkForGenericClasses() {
			AssertCorrectEmulation(
@"public class MyClass<T> {
	private int f1, f2, f3;
	[System.Runtime.CompilerServices.IntrinsicProperty]
	public int P1 {
		get { return f1; } set { f1 = value; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public int P2 {
		get { return f2; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public int P3 {
		set { f3 = value; }
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass<T>
var $MyClass$1 = global.MyClass$1 = {Script}.mkType($asm, 'MyClass$1', function(T) {
	var $type = {Script}.registerGenericClassInstance({MyClass}, [T], function() {
		this.$f1 = 0;
		this.$f2 = 0;
		this.$f3 = 0;
	}, {
		get p1() {
			return this.$f1;
		},
		set p1(value) {
			this.$f1 = value;
		},
		get p2() {
			return this.$f2;
		},
		set p3(value) {
			this.$f3 = value;
		}
	});
	return $type;
});
{Script}.initGenericClass($MyClass$1, 1);
-
", "MyClass<T>");
		}

		[Test]
		public void StaticPropertiesWithGeneratedAccessorsWorkForGenericClasses() {
			AssertCorrectEmulation(
@"public class MyClass<T> {
	private static int f1, f2, f3;
	[System.Runtime.CompilerServices.IntrinsicProperty]
	public static int P1 {
		get { return f1; } set { f1 = value; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public static int P2 {
		get { return f2; }
	}

	[System.Runtime.CompilerServices.IntrinsicProperty]
	public static int P3 {
		set { f3 = value; }
	}
}
",
@"////////////////////////////////////////////////////////////////////////////////
// MyClass<T>
var $MyClass$1 = global.MyClass$1 = {Script}.mkType($asm, 'MyClass$1', function(T) {
	var $type = {Script}.registerGenericClassInstance({MyClass}, [T], function() {
	}, null, {
		get p1() {
			return $type.$f1;
		},
		set p1(value) {
			$type.$f1 = value;
		},
		get p2() {
			return $type.$f2;
		},
		set p3(value) {
			$type.$f3 = value;
		}
	});
	$type.$f1 = 0;
	$type.$f2 = 0;
	$type.$f3 = 0;
	return $type;
});
{Script}.initGenericClass($MyClass$1, 1);
-
", "MyClass<T>");
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
var $MyClass = global.MyClass = { field1: 'the value', field2: 42, field3: null };
-
", "MyClass");
		}

		[Test]
		public void MixinAttributeWorks() {
			AssertCorrectEmulation(
@"[System.Runtime.CompilerServices.Mixin(""$.fn"")]
public static class MyClass {
	public static void Method1(int x) { x = 0; }
	public static void Method2(int y) { y = 0; }
}",
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
var $$Outer = {Script}.mkType($asm, '$Outer', function() {
});
-
{Script}.initClass($$Outer);
", "Outer");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// Outer.Inner
var $$Outer$Inner = {Script}.mkType($asm, '$Outer$Inner', function() {
});
-
{Script}.initClass($$Outer$Inner);
", "Outer.Inner");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// GenericClass<T1>
var $$GenericClass$1 = {Script}.mkType($asm, '$GenericClass$1', function(T1) {
	var $type = {Script}.registerGenericClassInstance({GenericClass}, [T1], function() {
	});
	return $type;
});
{Script}.initGenericClass($$GenericClass$1, 1);
-
", "GenericClass<T1>");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// Interface
var $$Interface = {Script}.mkType($asm, '$Interface');
-
{Script}.initInterface($$Interface);
", "Interface");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// GenericInterface<T1>
var $$GenericInterface$1 = {Script}.mkType($asm, '$GenericInterface$1', function(T1) {
	var $type = {Script}.registerGenericInterfaceInstance({GenericInterface}, [T1]);
	return $type;
});
{Script}.initGenericInterface($$GenericInterface$1, 1);
-
", "GenericInterface<T1>");

			AssertCorrectEmulation(program,
@"////////////////////////////////////////////////////////////////////////////////
// ResourceClass
var $$ResourceClass = { $field1: 'the value', $field2: 42, $field3: null };
-
", "ResourceClass");
		}

		[Test]
		public void AbstractMethodsAreNotExported() {
			AssertCorrectEmulation(
@"public abstract class C { internal abstract void M(); }
",
@"////////////////////////////////////////////////////////////////////////////////
// C
var $C = global.C = {Script}.mkType($asm, 'C', function() {
});
-
{Script}.initClass($C);
", "C");
		}

		[Test]
		public void GenericClassesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public class GenericClass<T1> {}
",
@"////////////////////////////////////////////////////////////////////////////////
// GenericClass<T1>
var $GenericClass$1 = exports.GenericClass$1 = {Script}.mkType($asm, 'GenericClass$1', function(T1) {
	var $type = {Script}.registerGenericClassInstance({GenericClass}, [T1], function() {
	});
	return $type;
});
{Script}.initGenericClass($GenericClass$1, 1);
-
", "GenericClass<T1>");
		}

		[Test]
		public void NonGenericClassesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public class NormalClass {}
",
@"////////////////////////////////////////////////////////////////////////////////
// NormalClass
var $NormalClass = exports.NormalClass = {Script}.mkType($asm, 'NormalClass', function() {
});
-
{Script}.initClass($NormalClass);
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
var $ResourceClass = exports.ResourceClass = { field1: 'the value', field2: 42, field3: null };
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
// GenericInterface<T1>
var $GenericInterface$1 = exports.GenericInterface$1 = {Script}.mkType($asm, 'GenericInterface$1', function(T1) {
	var $type = {Script}.registerGenericInterfaceInstance({GenericInterface}, [T1]);
	return $type;
});
{Script}.initGenericInterface($GenericInterface$1, 1);
-
", "GenericInterface<T1>");
		}

		[Test]
		public void NonGenericInterfacesWithModuleNamesGetExportedToTheExportsObject() {
			AssertCorrectEmulation(
@"[assembly: System.Runtime.CompilerServices.ModuleName(""mymodule"")]
public interface Interface {}
",
@"////////////////////////////////////////////////////////////////////////////////
// Interface
var $Interface = exports.Interface = {Script}.mkType($asm, 'Interface');
-
{Script}.initInterface($Interface);
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
var $D = global.D = {Script}.mkType($asm, 'D', null, null, {
	createInstance: function() {
		return {D}.$ctor();
	},
	$ctor: function() {
		var $this = {B}.$ctor();
		return $this;
	}
});
-
{Script}.initClass($D, {B});
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
var $C = global.C = {Script}.mkType($asm, 'C', null, null, {
	createInstance: function() {
		return {C}.$ctor();
	},
	$ctor: function() {
		var $this = {};
		return $this;
	}
});
-
{Script}.initClass($C, null, [{I1}]);
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
var $D = global.D = {Script}.mkType($asm, 'D', function() {
	{B}.call(this);
});
-
{Script}.initClass($D, {B});
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
var $I3 = global.I3 = {Script}.mkType($asm, 'I3');
-
{Script}.initInterface($I3, [{I2}]);
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
var $D = global.D = {Script}.mkType($asm, 'D', function() {
});
-
{Script}.initClass($D, null, [{I2}]);
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
var $I3 = global.I3 = {Script}.mkType($asm, 'I3');
-
{Script}.initInterface($I3, [{I1}, {I2}]);
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
var $D = global.D = {Script}.mkType($asm, 'D', function() {
});
-
{Script}.initClass($D, null, [{I1}, {I2}]);
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
var $I2 = global.I2 = {Script}.mkType($asm, 'I2');
-
{Script}.initInterface($I2, [{Script}.makeGenericType({I}, [{Object}, {Int32}])]);
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
var $D = global.D = {Script}.mkType($asm, 'D', function() {
	{Script}.makeGenericType({B}, [{Object}, {Int32}]).call(this);
});
-
{Script}.initClass($D, {Script}.makeGenericType({B}, [{Object}, {Int32}]), [{Script}.makeGenericType({I}, [{String}, {Object}])]);
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
var $I2 = global.I2 = {Script}.mkType($asm, 'I2');
-
{Script}.initInterface($I2, [{Script}.makeGenericType({I}, [{C}, {Int32}])]);
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
var $D = global.D = {Script}.mkType($asm, 'D', function() {
	{Script}.makeGenericType({B}, [{C}, {Int32}]).call(this);
});
-
{Script}.initClass($D, {Script}.makeGenericType({B}, [{C}, {Int32}]), [{Script}.makeGenericType({I}, [{String}, {C}])]);
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
", "D1<T>", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type D1")));
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
// MyClass<T1, T2>
var $MyClass$2 = global.MyClass$2 = {Script}.mkType($asm, 'MyClass$2', function(T1, T2) {
	var $type = {Script}.registerGenericClassInstance({MyClass}, [T1, T2], function() {
		$type.f();
		$type.f();
		{Script}.makeGenericType({MyClass}, [{Int32}, {String}]).f();
		{Script}.makeGenericType({MyClass}, [T2, T1]).f();
		{Script}.makeGenericType({OtherClass}, [T1, T2]).f();
	}, null, {
		f: function() {
		}
	});
	return $type;
});
{Script}.initGenericClass($MyClass$2, 2);
-
", "MyClass<T1, T2>");
		}

		[Test]
		public void InheritanceFromImportedSerializableClassIsNotRecordedInInheritanceList() {
			AssertCorrectEmulation(
@"[System.Runtime.CompilerServices.Imported, System.Serializable] public class B {}
public class D : B {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = global.D = {Script}.mkType($asm, 'D', function() {
});
-
{Script}.initClass($D);
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
var $I3 = global.I3 = {Script}.mkType($asm, 'I3');
-
{Script}.initInterface($I3, [{I2}]);
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
var $C = global.C = {Script}.mkType($asm, 'C', function() {
});
-
{Script}.initClass($C, null, [{I2}]);
", "C");
		}

		[Test]
		public void TypeCheckCodeForSerializableTypesWorks() {
			AssertCorrectEmulation(
@"using System;
using System.Runtime.CompilerServices;
[Serializable] public class C {}
[Serializable(TypeCheckCode = ""{this}.X"")] public class D : C {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D
var $D = global.D = {Script}.mkType($asm, 'D', null, null, {
	createInstance: function() {
		return {D}.$ctor();
	},
	$ctor: function() {
		var $this = {C}.$ctor();
		return $this;
	},
	isInstanceOfType: function(obj) {
		return obj.X;
	}
});
-
{Script}.initClass($D, {C});
", "D");
		}

		[Test]
		public void TypeCheckCodeForGenericSerializableTypesWorks() {
			AssertCorrectEmulation(
@"using System;
using System.Runtime.CompilerServices;
[Serializable] public class C {}
[Serializable(TypeCheckCode = ""{this}.X == {T}"")] public class D<T> : C {}
",
@"////////////////////////////////////////////////////////////////////////////////
// D<T>
var $D$1 = global.D$1 = {Script}.mkType($asm, 'D$1', function(T) {
	var $type = {Script}.registerGenericClassInstance({D}, [T], null, null, {
		createInstance: function() {
			return $type.$ctor();
		},
		$ctor: function() {
			var $this = {C}.$ctor();
			return $this;
		},
		isInstanceOfType: function(obj) {
			return obj.X == T;
		}
	}, function() {
		return {C};
	});
	return $type;
});
{Script}.initGenericClass($D$1, 1);
-
", "D<T>");
		}

		[Test]
		public void UsingUnavailableTypeParameterInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			EmulateType(@"
[System.Serializable(TypeCheckCode = ""{this} == {T}""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", "C1<T>", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7536 && m.FormattedMessage.Contains("IncludeGenericArguments") && m.FormattedMessage.Contains("type C1")));
		}

		[Test]
		public void ReferencingNonExistentTypeInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			EmulateType(@"
[System.Serializable(TypeCheckCode = ""{this} == {$Some.Nonexistent.Type}""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", "C1<T>", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7157 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("Some.Nonexistent.Type")));
		}

		[Test]
		public void SyntaxErrorInSerializableTypeCheckCodeIsAnError() {
			var er = new MockErrorReporter();
			EmulateType(@"
[System.Serializable(TypeCheckCode = ""{{this} == 1""), System.Runtime.CompilerServices.IncludeGenericArguments(false)] public class C1<T> {}
", "C1<T>", errorReporter: er);
			Assert.That(er.AllMessages.Count, Is.EqualTo(1));
			Assert.That(er.AllMessages.Any(m => m.Severity == DiagnosticSeverity.Error && m.Code == 7157 && m.FormattedMessage.Contains("C1") && m.FormattedMessage.Contains("syntax error")));
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
// IMyInterface<T1, T2, T3>
var $IMyInterface$3 = global.IMyInterface$3 = {Script}.mkType($asm, 'IMyInterface$3', function(T1, T2, T3) {
	var $type = {Script}.registerGenericInterfaceInstance({IMyInterface}, [T1, T2, T3]);
	{Script}.setMetadata($type, { variance: [0, 1, 2] });
	return $type;
});
{Script}.initGenericInterface($IMyInterface$3, 3);
-
-
{Script}.setMetadata($IMyInterface$3, { variance: [0, 1, 2] });
", "IMyInterface<T1, T2, T3>");
		}

		[Test]
		public void TheFirstPhaseDoesNotHaveAnyDependencies() {
			var actual = EmulateType(@"
class MyAttribute : System.Attribute {}
class B1 {}
class B2<T> : B1 {}
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
class MyAttribute : System.Attribute {}
class B1 {}
class B2<T> : B1 {}
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
			Assert.That(actual.Replace("\r\n", "\n"), Is.EqualTo("(function() {\n\tvar x = 0;\n\tvar y = 1;\n})();\n"));
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
	}
}
