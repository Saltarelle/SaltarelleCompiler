using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class ExceptionTests : RuntimeLibraryTestBase {
		[Test]
		public void ThrowingAndCatchingExceptionsWorks() {
			var result = ExecuteCSharp(@"
using System;
public class E1 : Exception {
	public E1(string message) : base(message) { }
}
public class E2 : E1 {
	public E2(string message) : base(message) { }
}

public class C {
	public static string M() {
		try {
			throw new E2(""The message"");
		}
		catch (E2 e) {
			return e.Message;
		}
		return null;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("The message"));
		}

		[Test]
		public void CanCatchExceptionAsBaseType() {
			var result = ExecuteCSharp(@"
using System;
public class E1 : Exception {
	public E1(string message) : base(message) { }
}
public class E2 : E1 {
	public E2(string message) : base(message) { }
}

public class C {
	public static string M() {
		try {
			throw new E1(""The message"");
		}
		catch (E2 e) {
			return null;
		}
		catch (E1 e) {
			return e.Message;
		}
		return null;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("The message"));
		}

		[Test]
		public void CanCatchStringAsException() {
			var result = ExecuteCSharp(@"
using System;
public class E1 : Exception {
	public E1(string message) : base(message) { }
}
public class E2 : E1 {
	public E2(string message) : base(message) { }
}

public class C {
	[System.Runtime.CompilerServices.InlineCode(""(function() {{ throw 'The message'; }})()"")]
	private static void ThrowIt();

	public static string M() {
		try {
			ThrowIt();
		}
		catch (E1 e) {
			return null;
		}
		catch (Exception e) {
			return e.Message;
		}
		return null;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("The message"));
		}

		[Test]
		public void CanCatchStringAsCatchAll() {
			var result = ExecuteCSharp(@"
using System;
public class E1 : Exception {
	public E1(string message) : base(message) { }
}
public class E2 : E1 {
	public E2(string message) : base(message) { }
}

public class C {
	[System.Runtime.CompilerServices.InlineCode(""(function() {{ throw 'The message'; }})()"")]
	private static void ThrowIt();

	public static string M() {
		try {
			ThrowIt();
		}
		catch (E1 e) {
			return null;
		}
		catch {
			return ""OK"";
		}
		return null;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo("OK"));
		}
	}
}
