using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class JsDictionaryTests : RuntimeLibraryTestBase {
		[Test]
		public void NonGenericJsDictionaryWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;
public class C {
	static void Print<T>(ICollection<T> c) {
		var l = new List<T>();
		foreach (var e in c) {
			l.Add(e);
		}
		l.Sort();
		s += l.Join("","");
		s += ""\n"";
	}
	static void Print(object o) {
		s += o + ""\n"";
	}

	static string s;
	public static string M() {
		s = """";

		var d = new JsDictionary();
		d[""a""] = ""va"";
		d[""b""] = ""vb"";
		d[""c""] = ""vc"";
		Print(d.Count);
		Print(d[""b""]);
		Print(d.Keys);
		Print(d.ContainsKey(""b""));
		Print(d.ContainsKey(""vb""));
		var items = new List<string>();
		foreach (var e in d) {
			items.Add(e.Key + "" = "" + e.Value);
		}
		Print(items);
		d.Remove(""b"");
		Print(d.Keys);
		var d2 = JsDictionary.GetDictionary(new { x = ""value1"", y = ""value2"" });
		Print(d2.Keys);

		return s;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(
@"3
vb
a,b,c
true
false
a = va,b = vb,c = vc
a,c
x,y
".Replace("\r\n", "\n")));
		}

		[Test]
		public void GenericJsDictionaryWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;
public class C {
	static void Print<T>(ICollection<T> c) {
		var l = new List<T>();
		foreach (var e in c) {
			l.Add(e);
		}
		l.Sort();
		s += l.Join("","");
		s += ""\n"";
	}
	static void Print(object o) {
		s += o + ""\n"";
	}

	static string s;
	public static string M() {
		s = """";

		var d = new JsDictionary<string, string>();
		d[""a""] = ""va"";
		d[""b""] = ""vb"";
		d[""c""] = ""vc"";
		Print(d.Count);
		Print(d[""b""]);
		Print(d.Keys);
		Print(d.ContainsKey(""b""));
		Print(d.ContainsKey(""vb""));
		var items = new List<string>();
		foreach (var e in d) {
			items.Add(e.Key + "" = "" + e.Value);
		}
		Print(items);
		d.Remove(""b"");
		Print(d.Keys);
		var d2 = JsDictionary<string, string>.GetDictionary(new { x = ""value1"", y = ""value2"" });
		Print(d2.Keys);

		return s;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(
@"3
vb
a,b,c
true
false
a = va,b = vb,c = vc
a,c
x,y
".Replace("\r\n", "\n")));
		}
	}
}
