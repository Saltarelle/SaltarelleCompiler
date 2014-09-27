using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Saltarelle.Compiler.Tests.AttributeStoreTests {
	public class Test1Attribute : Attribute {
	}

	public class Test2Attribute : Attribute {
		public string S { get; private set; }
		public int X { get; private set; }

		public Test2Attribute(int x) {
			X = x;
		}

		public Test2Attribute(string s, int x = 12) {
			S = s;
			X = x;
		}
	}

	public class Test3Attribute : Attribute {
		public string S { get; private set; }
		public int F1;
		public int P1 { get; set; }

		public Test3Attribute() {
		}

		public Test3Attribute(string s) {
			S = s;
		}
	}

	public class Test4Attribute : Test3Attribute {
		public int F2;
		public int P2 { get; set; }
	}

	public enum Test5AttributeEnum {
		Test1,
		Test2 = 17,
	}

	public class Test5Attribute : Attribute {
		public double d;
		public float f;
		public long l;
		public ulong ul;
		public int i;
		public uint ui;
		public short s;
		public short us;
		public byte b;
		public sbyte sb;
		public string st;
		public Test5AttributeEnum e;
		public Type t;
		public object o;
	}

	public class Test6Attribute : Attribute {
		public int[] ai;
		public object[] ao;
		public object o;
	}

	public class Test7Attribute :
	#if !TEST_ASSEMBLY
		PluginAttributeBase
	#else
		Attribute
	#endif
	 {
		public string Data { get; private set; }


		public Test7Attribute(string data) {
			Data = data;
		}

#if !TEST_ASSEMBLY
		private static List<Tuple<Microsoft.CodeAnalysis.ISymbol, string>> _allApplications = new List<Tuple<Microsoft.CodeAnalysis.ISymbol, string>>();

		public static List<Tuple<Microsoft.CodeAnalysis.ISymbol, string>> AllApplications {
			get { return _allApplications; }
		}

		public override void ApplyTo(Microsoft.CodeAnalysis.ISymbol symbol, IAttributeStore attributeStore, IErrorReporter errorReporter) {
			_allApplications.Add(Tuple.Create(symbol, Data));
		}
#endif
	}

	public class Test8Attribute : Attribute {
		public int Line { get; private set; }
		public string Path { get; private set; }
		public string Member { get; private set; }

		public Test8Attribute([CallerLineNumber] int line = 0, [CallerFilePath] string path = null, [CallerMemberName] string member = null) {
			Line = line;
			Path = path;
			Member = member;
		}
	}
}
