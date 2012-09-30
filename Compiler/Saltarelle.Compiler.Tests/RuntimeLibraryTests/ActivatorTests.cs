using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests
{
	[TestFixture]
	public class ActivatorTests : RuntimeLibraryTestBase {
		[Test]
		public void ActivatorCreateInstanceWithNoArgumentsWorksForClassWithUnnamedDefaultConstructor() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public int I;
	public C() {
		I = 42;
	}

	public static T Create<T>() where T : new() {
		return new T();
	}

	public static int[] M() {
		var c1 = Activator.CreateInstance<C>();
		var c2 = (C)Activator.CreateInstance(typeof(C));
		var c3 = Create<C>();
		return new[] { c1.I, c2.I, c3.I };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo(42));
		}

		[Test]
		public void ActivatorCreateInstanceWithNoArgumentsWorksForClassWithNamedDefaultConstructor() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
public class C {
	public int I;
	[ScriptName(""named"")]
	public C() {
		I = 42;
	}

	[ScriptName("""")]
	public C(int i) {
		I = 1;
	}

	public static T Create<T>() where T : new() {
		return new T();
	}

	public static int[] M() {
		var c1 = Activator.CreateInstance<C>();
		var c2 = (C)Activator.CreateInstance(typeof(C));
		var c3 = Create<C>();
		return new[] { c1.I, c2.I, c3.I };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo(42));
		}

		[Test]
		public void ActivatorCreateInstanceWithNoArgumentsWorksForClassWithInlineCodeDefaultConstructor() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
public class C {
	public int i;
	[InlineCode(""{{ i: 42 }}"")]
	public C() {
		I = 42;
	}

	public static T Create<T>() where T : new() {
		return new T();
	}

	public static int[] M() {
		dynamic c1 = Activator.CreateInstance<C>();
		dynamic c2 = Activator.CreateInstance(typeof(C));
		dynamic c3 = Create<C>();
		return new int[] { c1.i, c2.i, c3.i };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo(42));
		}

		[Test]
		public void ActivatorCreateInstanceWithNoArgumentsWorksForClassWithStaticMethodDefaultConstructor() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
[Serializable]
class S {
	public int I;
	public S() {
		I = 42;
	}
}

public class C {
	public static T Create<T>() where T : new() {
		return new T();
	}

	public static int[] M() {
		var s1 = Activator.CreateInstance<S>();
		var s2 = (S)Activator.CreateInstance(typeof(S));
		var s3 = (S)Activator.CreateInstance(typeof(S));
		return new[] { s1.I, s2.I, s3.I };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo(42));
		}

		[Test]
		public void ActivatorCreateInstanceWithNoArgumentsWorksForClassWithJsonDefaultConstructor() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
[Serializable]
class S {
	[ObjectLiteral]
	public S() {
	}
}

public class C {
	public static T Create<T>() where T : new() {
		return new T();
	}

	public static bool[] M() {
		var s1 = Activator.CreateInstance<S>();
		var s2 = (S)Activator.CreateInstance(typeof(S));
		var s3 = Create<S>();
		return new bool[] { ((dynamic)s1).constructor == typeof(object), ((dynamic)s2).constructor == typeof(object), ((dynamic)s3).constructor == typeof(object) };
	}
}", "C.M");
			Assert.That(result, Has.All.True);
		}

		[Test]
		public void ActivatorCreateInstanceWithNoArgumentsWorksForGenericClassWithNamedDefaultConstructor() {
			var result = ExecuteCSharp(@"
using System;
using System.Runtime.CompilerServices;
public class C2<T> {
	public int I;
	[ScriptName(""named"")]
	public C2() {
		I = 42;
	}

	[ScriptName("""")]
	public C2(T t) {
		I = 1;
	}
}

public class C {
	public static T Create<T>() where T : new() {
		return new T();
	}

	public static int[] M() {
		var c1 = Activator.CreateInstance<C2<int>>();
		var c2 = (C2<int>)Activator.CreateInstance(typeof(C2<int>));
		var c3 = Create<C2<int>>();
		return new[] { c1.I, c2.I, c3.I };
	}
}", "C.M");
			Assert.That(result, Has.All.EqualTo(42));
		}
	}
}
