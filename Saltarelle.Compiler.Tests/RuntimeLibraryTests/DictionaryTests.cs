using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class DictionaryTests : RuntimeLibraryTestBase {
		[Test]
		public void DictionaryWorks() {
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
	static bool DoesItThrow(Action a) {
		try {
			a();
			return false;
		}
		catch {
			return true;
		}
	}

	static string s;
	public static string M() {
		s = """";

		var d = new Dictionary<string, int>();
		d[""a""] = 10;
		d[""b""] = 11;
		d[""c""] = 12;
		Print(d.Count);
		Print(d[""b""]);
		Print(((IDictionary<string, int>)d)[""b""]);
		((IDictionary<string, int>)d)[""b""] = 21;
		Print(((IDictionary<string, int>)d)[""b""]);

		Print(d.Keys);
		Print(d.Values);
		Print(d.ContainsKey(""b""));
		Print(d.ContainsKey(""b2""));
		var items = new List<string>();
		foreach (var e in d) {
			items.Add(e.Key + "" = "" + e.Value);
		}
		Print(items);
		d.Remove(""b"");
		Print(d.Keys);
		d.Add(""x"", 20);
		Print(d.Keys);
		Print(DoesItThrow(() => d.Add(""x"", 100));
		int i;
		Print(d.TryGetValue(""x"", out i));
		Print(i);
		Print(d.TryGetValue(""y"", out i));
		Print(i);
		d.Clear();
		Print(d.Count);

		return s;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(
@"3
11
11
21
a,b,c
10,12,21
true
false
a = 10,b = 21,c = 12
a,c
a,c,x
true
true
20
false
0
0
".Replace("\r\n", "\n")));
		}

		[Test]
		public void AllConstructorsWork() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections;
using System.Collections.Generic;
public class C {
	static void Print<TKey, TValue>(Dictionary<TKey, TValue> d) {
		var l = new List<string>();
		foreach (var e in d) {
			l.Add(e.Key + "" = "" + e.Value);
		}
		l.Sort();
		s += l.Join("","");
		s += ""\n"";
	}

	static string s;
	public static string M() {
		s = """";

		var d1 = new Dictionary<string, int>();
		s += d1.Count + ""\n"";

		var dx = new Dictionary<string, int>() { { ""x"", 10 }, { ""y"", 20 } };

		var d2 = new Dictionary<string, int>(dx);
		Print(d2);

		var d3 = new Dictionary<string, int>((JsDictionary<string, int>)(object)new { a = 1, b = 2 });
		Print(d3);

		return s;
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(
@"0
x = 10,y = 20
a = 1,b = 2
".Replace("\r\n", "\n")));
		}
	}
}
