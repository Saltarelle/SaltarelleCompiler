using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.RuntimeLibraryTests {
	[TestFixture]
	public class ArrayTests : RuntimeLibraryTestBase {
		[Test]
		public void CollectionInterfacesAreCorrectlyAssignable() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections.Generic;
public class C {
	public static bool[] M() {
		return new[] { typeof(IEnumerable<int>).IsAssignableFrom(typeof(ICollection<int>)), typeof(IEnumerable<int>).IsAssignableFrom(typeof(IList<int>)), typeof(ICollection<int>).IsAssignableFrom(typeof(IList<int>)) };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, true, true }));
		}

		[Test]
		public void ArrayCanBeAssignedToTheCollectionInterfaces() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections.Generic;

public class MyList : IList<int> {
	public int this[int index] { get { return 0; } set {} }
	public int IndexOf(int item) { return 0; }
	public void Insert(int index, int item) {}
	public void RemoveAt(int index) {}
	public int Count { get { return 0; } }
	public void Add(int item) {}
	public void Clear() {}
	public bool Contains(int item) {return false; }
	public bool Remove(int item) { return false; }
	public IEnumerator<int> GetEnumerator() { return null; }
}

public class C {
	public static bool[] M() {
		var obj = new object();
		var arr = new[] { 1, 2, 3 };
		var l = new MyList();
		return new[] { arr is IEnumerable<int>,
		               arr is ICollection<int>,
		               arr is IList<int>,
		               l is IEnumerable<int>,
		               l is ICollection<int>,
		               l is IList<int>,
		               obj is IEnumerable<int>,
		               obj is ICollection<int>,
		               obj is IList<int>,
		               typeof(IEnumerable<int>).IsAssignableFrom(typeof(int[])),
		               typeof(ICollection<int>).IsAssignableFrom(typeof(int[])),
		               typeof(IList<int>).IsAssignableFrom(typeof(int[])),
		               typeof(IEnumerable<int>).IsAssignableFrom(typeof(MyList)),
		               typeof(ICollection<int>).IsAssignableFrom(typeof(MyList)),
		               typeof(IList<int>).IsAssignableFrom(typeof(MyList)),
		               typeof(IEnumerable<int>).IsAssignableFrom(typeof(object)),
		               typeof(ICollection<int>).IsAssignableFrom(typeof(object)),
		               typeof(IList<int>).IsAssignableFrom(typeof(object)),
		             };
	}
}", "C.M");
			Assert.That(result, Is.EqualTo(new[] { true, true, true, true, true, true, false, false, false, true, true, true, true, true, true, false, false, false }));
		}

		[Test]
		public void ArrayCastToIListWorks() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections.Generic;

public class C {
	static string s;
	public static void Print(object o) {
		C.s += o + ""\n"";
	}
	public static void Print(int[] arr) {
		for (int i = 0; i < arr.Length; i++) {
			s += (i == 0 ? """" : "","") + arr[i];
		}
		s += ""\n""
	}

	public static void M() {
		s = """";
		var arr = new[] { 1, 2, 3 };
		var l = (IList<int>)arr;

		Print(l[1]);
		l[1] = 4;
		Print(l[1]);
		Print(l.IndexOf(4));
		l.Insert(1, 5);
		Print(arr);
		l.RemoveAt(2);
		Print(arr);
		l.Add(6);
		Print(arr);
		Print(l.Contains(6));
		l.Remove(5);
		Print(arr);
		foreach (var item in l) {
			Print(""|"" + item);
		}
		l.Clear();
		Print(arr.Length);
		return s;
	}
}
", "C.M");
			Assert.That(result, Is.EqualTo(
@"2
4
1
1,5,4,3
1,5,3
1,5,3,6
true
1,3,6
|1
|3
|6
0
".Replace("\r\n", "\n")));
		}

		[Test]
		public void ArrayMethodsWork() {
			var result = ExecuteCSharp(@"
using System;

public class C {
	static string s;
	public static void Print(object o) {
		C.s += o + ""\n"";
	}

	public static void M() {
		s = """";
		var arr = new[] { 1, 2, 3 };

		Print(arr[1]);
		arr[1] = 4;
		Print(arr[1]);
		Print(arr.Length);
		foreach (var item in arr) {
			Print(""|"" + item);
		}
		Print(arr.Aggregate(0, (o, v, i, a) => (int)o + (int)v));
		Print(arr.Aggregate(0, (o, v) => (int)o + (int)v));

		var arr2 = arr.Clone();
		Print(arr2 == arr);
		Print(arr2);
		Print(arr.Concat(new[] { 5, 6 }));
		Print(arr.Contains(4));
		Print(arr.Every((v, i, a) => (int)v >= 0));
		Print(arr.Every(v => (int)v >= 2));
		Print(arr.Extract(1));
		Print(arr.Extract(0, 2));
		Print(arr.Filter((v, i, a) => (int)v != 4));
		Print(arr.Filter(v => (int)v != 4));
		arr.ForEach((v, i, a) => Print(""|"" + v));
		arr.ForEach(Print);
		Print(arr.IndexOf(4));
		Print(arr.IndexOf(4, 3));
		Print(arr.Join());
		Print(arr.Join(""|""));
		Print(arr.Map((v, i, a) => (int)v + 1));
		Print(arr.Map(v => (int)v + 1));
		arr.Reverse();
		Print(arr);
		Print(arr.Some((v, i, a) => (int)v == 4));
		Print(arr.Some(v => (int)v == 10));
		arr.Sort();
		Print(arr);
		arr.Sort((a, b) => (int)b - (int)a);
		Print(arr);
		Print(arr.Concat(56, 57, 58));

		return s;
	}
}
", "C.M");
			Assert.That(result, Is.EqualTo(
@"2
4
3
|1
|4
|3
8
8
false
1,4,3
1,4,3,5,6
true
true
false
4,3
1,4
1,3
1,3
|1
|4
|3
1
4
3
1
-1
1,4,3
1|4|3
2,5,4
2,5,4
3,4,1
true
false
1,3,4
4,3,1
4,3,1,56,57,58
".Replace("\r\n", "\n")));
		}

		[Test]
		public void ListMethodsWork() {
			var result = ExecuteCSharp(@"
using System;
using System.Collections.Generic;

public class C {
	static string s;
	public static void Print(object o) {
		C.s += o + ""\n"";
	}

	public static void M() {
		s = """";
		var l = new List<int>(1, 2, 3);
		var i = (IList<int>)l;

		Print(l[1]);
		l[1] = 5;
		Print(l[1]);
		Print(l.Count);
		foreach (var item in l) {
			Print(""|"" + item);
		}
		Print(i[1]);
		i[1] = 4;
		Print(i[1]);
		Print(i.Count);
		Print(l.Aggregate(0, (o, v, i, a) => o + v));
		Print(l.Aggregate(0, (o, v) => o + v));
		var l2 = l.Clone();
		Print(l2 == l);
		Print(l2);
		Print(l.Concat(new[] { 5, 6 }));
		Print(l.Contains(4));
		Print(l.Every((v, i, a) => (int)v >= 0));
		Print(l.Every(v => (int)v >= 2));
		Print(l.Extract(1));
		Print(l.Extract(0, 2));
		Print(l.Filter((v, i, a) => (int)v != 4));
		Print(l.Filter(v => (int)v != 4));
		l.ForEach((v, i, a) => Print(""|"" + v));
		l.ForEach(Print);
		Print(l.IndexOf(4));
		Print(l.IndexOf(4, 3));
		Print(l.Join());
		Print(l.Join(""|""));
		Print(l.Map((v, i, a) => v + 1);
		Print(l.Map(v => v + 1);
		l.Reverse();
		Print(l);
		Print(l.Some((v, i, a) => (int)v == 4));
		Print(l.Some(v => (int)v == 10));
		l.Sort();
		Print(l);
		l.Sort((a, b) => b - a);
		Print(l);
		l.Add(10);
		Print(l);
		l.AddRange(new[] { 11, 12, });
		Print(l);
		l.AddRange((IList<int>)(object)new[] { 13, 14 });
		Print(l);
		l.Insert(2, 20);
		Print(l);
		l.InsertRange(5, new[] { 21, 22, });
		Print(l);
		l.InsertRange(8, (IList<int>)(object)new[] { 23, 24 });
		Print(l);
		l.RemoveAt(3);
		Print(l);
		l.Remove(23);
		Print(l);
		l.RemoveRange(3, 4);
		Print(l);
		Print((Array)l);
		Print(l.Concat(67, 68, 69));
		l.Clear();
		Print(l.Count);

		return s;
	}
}
", "C.M");
			Assert.That(result, Is.EqualTo(
@"2
5
3
|1
|5
|3
5
4
3
8
8
false
1,4,3
1,4,3,5,6
true
true
false
4,3
1,4
1,3
1,3
|1
|4
|3
1
4
3
1
-1
1,4,3
1|4|3
2,5,4
2,5,4
3,4,1
true
false
1,3,4
4,3,1
4,3,1,10
4,3,1,10,11,12
4,3,1,10,11,12,13,14
4,3,20,1,10,11,12,13,14
4,3,20,1,10,21,22,11,12,13,14
4,3,20,1,10,21,22,11,23,24,12,13,14
4,3,20,10,21,22,11,23,24,12,13,14
4,3,20,10,21,22,11,24,12,13,14
4,3,20,24,12,13,14
4,3,20,24,12,13,14
4,3,20,24,12,13,14,67,68,69
0
".Replace("\r\n", "\n")));
		}
	}
}
