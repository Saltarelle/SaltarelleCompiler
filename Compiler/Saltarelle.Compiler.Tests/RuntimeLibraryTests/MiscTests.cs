using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class MiscTests : RuntimeLibraryTestBase {
		[Test]
		public void CoalesceWorks() {
			var result = ExecuteCSharp(@"
public class C {
	public static object[] M() {
		int? v1 = null, v2 = 1, v3 = 0, v4 = 2;
		string s1 = null, s2 = ""x"";
		return new object[] { v1 ?? v1, v1 ?? v2, v3 ?? v4, s1 ?? s1, s1 ?? s2 };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new object [] { null, 1, 0, null, "x" }));
		}

		[Test]
		public void ScriptBooleanWorks() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public static bool[] M() {
		return new[] { Script.Boolean(""x""), Script.Boolean("""") };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, false }));
		}

		[Test]
		public void ScriptEvalWorks() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public static object M() {
		return Script.Eval(""1 + 2"");
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(3));
		}

		[Test]
		public void ArgumentsMembersWork() {
			var result = ExecuteCSharp(@"
using System;
public class C {
	public static object[] Other(int a, string b, int c) {
		return new[] { Arguments.Length, Arguments.GetArgument(1), Arguments.ToArray() };
	}

	public static object M() {
		return Other(1, ""x"", 2);
	}
}", "C.M");
			var l = (IList)result;
			Assert.That(l, Has.Count.EqualTo(3));
			Assert.That(l[0], Is.EqualTo(3));
			Assert.That(l[1], Is.EqualTo("x"));
			Assert.That(l[2], Is.EqualTo(new object[] { 1, "x", 2 }));
		}

		[Test]
		public void StringBuilderWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Text;
public class C {
	public static string M() {
		var arr = new[] { 1, 2 };
		var sb = new StringBuilder();
		sb.Append(true)
		  .Append('X')
		  .Append(4)
		  .Append(1.5)
		  .Append(arr)
		  .Append(""Text"")
		  .AppendLine()
		  .AppendLine(true)
		  .AppendLine('X')
		  .AppendLine(4)
		  .AppendLine(1.5)
		  .AppendLine(arr)
		  .AppendLine(""Text"")
		  .AppendLine();

		  return sb.ToString();
	}
}", "C.M");
			Assert.That(((string)result).Replace("\r\n", "\n"), Is.EqualTo(
@"trueX41.51,2Text
true
X
4
1.5
1,2
Text

".Replace("\r\n", "\n")));
		}

		[Test]
		public void QueryExpressionWorks() {
			var result = ExecuteCSharp(@"
using System.Linq;
public class C {
	public static void M() {
		string[] args = new[] { ""4"", ""5"", ""7"" };
		// BEGIN
		return (from a in args let b = int.Parse(a) let c = b + 1 select a + b.ToString() + c.ToString()).ToArray();
		// END
	}
}", "C.M", includeLinq: true);
			Assert.That(result, Is.EqualTo(new[] { "445", "556", "778" }));
		}

		[Test]
		public void UpcastCharToObjectIsAnError() {
			var result = Compile(@"
public class C {
	public static void M() {
		string s = ""X"" + 'c';
	}
}", expectErrors: true);
			Assert.That(result.Item4.AllMessages.Select(m => m.Code), Is.EqualTo(new[] { 7700 }));
		}
	}
}
