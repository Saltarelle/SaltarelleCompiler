using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using QUnit;

namespace CoreLib.TestScript {
	[TestFixture]
	public class GuidTests {
		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(Guid).FullName, "ss.Guid");
			Assert.IsFalse(typeof(Guid).IsClass);
			Assert.IsTrue(typeof(IComparable<Guid>).IsAssignableFrom(typeof(Guid)));
			Assert.IsTrue(typeof(IEquatable<Guid>).IsAssignableFrom(typeof(Guid)));
			object o = new Guid();
			Assert.IsTrue(o is Guid);
			Assert.IsTrue(o is IComparable<Guid>);
			Assert.IsTrue(o is IEquatable<Guid>);

			var interfaces = typeof(JsDate).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 2);
			Assert.IsTrue(interfaces.Contains(typeof(IComparable<Guid>)));
			Assert.IsTrue(interfaces.Contains(typeof(IEquatable<Guid>)));

			Assert.IsFalse((object)1 is Guid);
			Assert.IsFalse((object)"abcd" is Guid);
			Assert.IsFalse((object)"{00000000-0000-0000-0000-000000000000}" is Guid);
		}

		[Test]
		public void DefaultValueWorks() {
			object result = default(Guid);
			Assert.IsTrue(result is Guid);
			Assert.AreEqual(result.ToString(), "00000000-0000-0000-0000-000000000000");
		}

		[Test]
		public void CreateInstanceWorks() {
			object result = Activator.CreateInstance<Guid>();
			Assert.IsTrue(result is Guid);
			Assert.AreEqual(result.ToString(), "00000000-0000-0000-0000-000000000000");
		}

		[Test]
		public void DefaultConstructorWorks() {
			object result = new Guid();
			Assert.IsTrue(result is Guid);
			Assert.AreEqual(result.ToString(), "00000000-0000-0000-0000-000000000000");
		}

		[Test]
		public void EmptyWorks() {
			Assert.AreEqual(Guid.Empty.ToString(), "00000000-0000-0000-0000-000000000000");
		}

		[Test]
		public void ToStringWithoutArgumentsWorks() {
			var guid = new Guid("223310CC-1F48-4489-B87E-88C779C77CB3");
			Assert.AreEqual(guid.ToString(), "223310cc-1f48-4489-b87e-88c779c77cb3");
		}

		[Test]
		public void ByteArrayConstructorWorks() {
			var g = new Guid(new byte[] { 0x78, 0x95, 0x62, 0xa8, 0x26, 0x7a, 0x45, 0x61, 0x90, 0x32, 0xd9, 0x1a, 0x3d, 0x54, 0xbd, 0x68 });
			Assert.IsTrue((object)g is Guid, "Should be Guid");
			Assert.AreEqual(g.ToString(), "789562a8-267a-4561-9032-d91a3d54bd68", "value");
			Assert.Throws(() => new Guid(new byte[] { 0x78, 0x95, 0x62, 0xa8, 0x26, 0x7a }), typeof(ArgumentException), "Invalid array should throw");
		}

		[Test]
		public void Int32Int16Int16ByteArrayConstructorWorks() {
			var g = new Guid((int)0x789562a8, (short)0x267a, (short)0x4561, new byte[] { 0x90, 0x32, 0xd9, 0x1a, 0x3d, 0x54, 0xbd, 0x68 });
			Assert.IsTrue((object)g is Guid, "Should be Guid");
			Assert.AreEqual(g.ToString(), "789562a8-267a-4561-9032-d91a3d54bd68", "value");
		}

		[Test]
		public void Int32Int16Int16BytesConstructorWorks() {
			var g = new Guid((int)0x789562a8, (short)0x267a, (short)0x4561, (byte)0x90, (byte)0x32, (byte)0xd9, (byte)0x1a, (byte)0x3d, (byte)0x54, (byte)0xbd, (byte)0x68);
			Assert.IsTrue((object)g is Guid, "Should be Guid");
			Assert.AreEqual(g.ToString(), "789562a8-267a-4561-9032-d91a3d54bd68", "value");
		}

		[Test]
		public void UInt32UInt16UInt16BytesConstructorWorks() {
			var g = new Guid((uint)0x789562a8, (ushort)0x267a, (ushort)0x4561, (byte)0x90, (byte)0x32, (byte)0xd9, (byte)0x1a, (byte)0x3d, (byte)0x54, (byte)0xbd, (byte)0x68);
			Assert.IsTrue((object)g is Guid, "Should be Guid");
			Assert.AreEqual(g.ToString(), "789562a8-267a-4561-9032-d91a3d54bd68", "value");
		}

		[Test]
		public void StringConstructorWorks() {
			object g1 = new Guid("A6993C0A-A8CB-45D9-994B-90E7203E4FC6");
			object g2 = new Guid("{A6993C0A-A8CB-45D9-994B-90E7203E4FC6}");
			object g3 = new Guid("(A6993C0A-A8CB-45D9-994B-90E7203E4FC6)");
			object g4 = new Guid("A6993C0AA8CB45D9994B90E7203E4FC6");
			Assert.IsTrue(g1 is Guid);
			Assert.IsTrue(g2 is Guid);
			Assert.IsTrue(g3 is Guid);
			Assert.IsTrue(g4 is Guid);
			Assert.AreEqual(g1.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g1");
			Assert.AreEqual(g2.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g2");
			Assert.AreEqual(g3.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g3");
			Assert.AreEqual(g4.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g4");
			Assert.Throws(() => new Guid("x"), typeof(FormatException), "Invalid should throw");
		}

		[Test]
		public void ParseWorks() {
			object g1 = Guid.Parse("A6993C0A-A8CB-45D9-994B-90E7203E4FC6");
			object g2 = Guid.Parse("{A6993C0A-A8CB-45D9-994B-90E7203E4FC6}");
			object g3 = Guid.Parse("(A6993C0A-A8CB-45D9-994B-90E7203E4FC6)");
			object g4 = Guid.Parse("A6993C0AA8CB45D9994B90E7203E4FC6");
			Assert.IsTrue(g1 is Guid);
			Assert.IsTrue(g2 is Guid);
			Assert.IsTrue(g3 is Guid);
			Assert.IsTrue(g4 is Guid);
			Assert.AreEqual(g1.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g1");
			Assert.AreEqual(g2.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g2");
			Assert.AreEqual(g3.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g3");
			Assert.AreEqual(g4.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g4");
			Assert.Throws(() => Guid.Parse("x"), typeof(FormatException), "Invalid should throw");
		}

		[Test]
		public void ParseExactWorks() {
			object g1 = Guid.ParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "D");
			object g2 = Guid.ParseExact("{A6993C0A-A8CB-45D9-994B-90E7203E4FC6}", "B");
			object g3 = Guid.ParseExact("(A6993C0A-A8CB-45D9-994B-90E7203E4FC6)", "P");
			object g4 = Guid.ParseExact("A6993C0AA8CB45D9994B90E7203E4FC6", "N");
			Assert.IsTrue(g1 is Guid);
			Assert.IsTrue(g2 is Guid);
			Assert.IsTrue(g3 is Guid);
			Assert.IsTrue(g4 is Guid);
			Assert.AreEqual(g1.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g1");
			Assert.AreEqual(g2.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g2");
			Assert.AreEqual(g3.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g3");
			Assert.AreEqual(g4.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g4");
			Assert.Throws(() => Guid.ParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "B"), typeof(FormatException), "Invalid B should throw");
			Assert.Throws(() => Guid.ParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "P"), typeof(FormatException), "Invalid P should throw");
			Assert.Throws(() => Guid.ParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "N"), typeof(FormatException), "Invalid N should throw");
			Assert.Throws(() => Guid.ParseExact("A6993C0AA8CB45D9994B90E7203E4FC6", "D"), typeof(FormatException), "Invalid D should throw");
		}

		[Test]
		public void TryParseWorks() {
			Guid g1, g2, g3, g4, g5;
			Assert.IsTrue (Guid.TryParse("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", out g1), "g1 result");
			Assert.IsTrue (Guid.TryParse("{A6993C0A-A8CB-45D9-994B-90E7203E4FC6}", out g2), "g2 result");
			Assert.IsTrue (Guid.TryParse("(A6993C0A-A8CB-45D9-994B-90E7203E4FC6)", out g3), "g3 result");
			Assert.IsTrue (Guid.TryParse("A6993C0AA8CB45D9994B90E7203E4FC6", out g4), "g4 result");
			Assert.IsFalse(Guid.TryParse("x", out g5), "Invalid should throw");
			Assert.IsTrue((object)g1 is Guid, "g1 is Guid");
			Assert.IsTrue((object)g2 is Guid, "g2 is Guid");
			Assert.IsTrue((object)g3 is Guid, "g3 is Guid");
			Assert.IsTrue((object)g4 is Guid, "g4 is Guid");
			Assert.IsTrue((object)g5 is Guid, "g5 is Guid");
			Assert.AreEqual(g1.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g1");
			Assert.AreEqual(g2.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g2");
			Assert.AreEqual(g3.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g3");
			Assert.AreEqual(g4.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g4");
			Assert.AreEqual(g5.ToString(), "00000000-0000-0000-0000-000000000000", "g5");
		}

		[Test]
		public void TryParseExactWorks() {
			Guid g1, g2, g3, g4, g5, g6, g7, g8;
			Assert.IsTrue (Guid.TryParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "D", out g1), "g1 result");
			Assert.IsTrue (Guid.TryParseExact("{A6993C0A-A8CB-45D9-994B-90E7203E4FC6}", "B", out g2), "g2 result");
			Assert.IsTrue (Guid.TryParseExact("(A6993C0A-A8CB-45D9-994B-90E7203E4FC6)", "P", out g3), "g3 result");
			Assert.IsTrue (Guid.TryParseExact("A6993C0AA8CB45D9994B90E7203E4FC6", "N", out g4), "g4 result");
			Assert.IsFalse (Guid.TryParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "B", out g5), "g5 result");
			Assert.IsFalse (Guid.TryParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "P", out g6), "g6 result");
			Assert.IsFalse (Guid.TryParseExact("A6993C0A-A8CB-45D9-994B-90E7203E4FC6", "N", out g7), "g7 result");
			Assert.IsFalse (Guid.TryParseExact("A6993C0AA8CB45D9994B90E7203E4FC6", "D", out g8), "g8 result");
			Assert.IsTrue((object)g1 is Guid);
			Assert.IsTrue((object)g2 is Guid);
			Assert.IsTrue((object)g3 is Guid);
			Assert.IsTrue((object)g4 is Guid);
			Assert.IsTrue((object)g5 is Guid);
			Assert.IsTrue((object)g6 is Guid);
			Assert.IsTrue((object)g7 is Guid);
			Assert.IsTrue((object)g8 is Guid);
			Assert.AreEqual(g1.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g1");
			Assert.AreEqual(g2.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g2");
			Assert.AreEqual(g3.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g3");
			Assert.AreEqual(g4.ToString(), "a6993c0a-a8cb-45d9-994b-90e7203e4fc6", "g4");
			Assert.AreEqual(g5.ToString(), "00000000-0000-0000-0000-000000000000", "g5");
			Assert.AreEqual(g6.ToString(), "00000000-0000-0000-0000-000000000000", "g6");
			Assert.AreEqual(g7.ToString(), "00000000-0000-0000-0000-000000000000", "g7");
			Assert.AreEqual(g8.ToString(), "00000000-0000-0000-0000-000000000000", "g8");
		}

		[Test]
		public void CompareToWorks() {
			var g = new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C");
			Assert.AreEqual(g.CompareTo(new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C")), 0, "equal");
			Assert.AreNotEqual(g.CompareTo(new Guid("E4C221BE-9B39-4398-B82A-48BA4648CAE0")), 0, "not equal");
		}

		[Test]
		public void IComparableCompareToWorks() {
			var g = (IComparable<Guid>)new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C");
			Assert.AreEqual(g.CompareTo(new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C")), 0, "Equal");
			Assert.AreNotEqual(g.CompareTo(new Guid("E4C221BE-9B39-4398-B82A-48BA4648CAE0")), 0, "Not equal");
		}

		[Test]
		public void EqualsObjectWorks() {
			var g = new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C");
			Assert.IsTrue (g.Equals((object)new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C")), "Equal");
			Assert.IsFalse(g.Equals((object)new Guid("E4C221BE-9B39-4398-B82A-48BA4648CAE0")), "Not equal");
			Assert.IsFalse(g.Equals("X"), "Not equal");
		}

		[Test]
		public void EqualsGuidWorks() {
			var g = new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C");
			Assert.IsTrue (g.Equals(new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C")), "Equal");
			Assert.IsFalse(g.Equals(new Guid("E4C221BE-9B39-4398-B82A-48BA4648CAE0")), "Not equal");
		}

		[Test]
		public void IEquatableEqualsWorks() {
			var g = (IEquatable<Guid>)new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C");
			Assert.IsTrue (g.Equals(new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C")), "Equal");
			Assert.IsFalse(g.Equals(new Guid("E4C221BE-9B39-4398-B82A-48BA4648CAE0")), "Not equal");
		}

		[Test]
		public void GetHashCodeWorks() {
			Assert.AreEqual(new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C").GetHashCode(), new Guid("f3d8b3c0-88f0-4148-844c-232ed03c153c").GetHashCode());
			Assert.AreNotEqual(new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153C").GetHashCode(), new Guid("F3D8B3C0-88F0-4148-844C-232ED03C153D").GetHashCode());
		}

		[Test]
		public void EqualityOperatorWorks() {
			Assert.IsTrue (new Guid("D311FC20-D7B6-40B6-88DB-9CD92AED6628") == new Guid("D311FC20-D7B6-40B6-88DB-9CD92AED6628"), "Equal");
			Assert.IsFalse(new Guid("D311FC20-D7B6-40B6-88DB-9CD92AED6628") == new Guid("A317804C-A583-4857-804F-A0D276008C82"), "Not equal");
		}

		[Test]
		public void InequalityOperatorWorks() {
			Assert.IsFalse(new Guid("D311FC20-D7B6-40B6-88DB-9CD92AED6628") != new Guid("D311FC20-D7B6-40B6-88DB-9CD92AED6628"), "Equal");
			Assert.IsTrue (new Guid("D311FC20-D7B6-40B6-88DB-9CD92AED6628") != new Guid("A317804C-A583-4857-804F-A0D276008C82"), "Not equal");
		}

		[Test]
		public void ToStringWithFormatWorks() {
			var g = new Guid("DE33AC65-09CB-465C-AD7E-53124B2104E8");
			Assert.AreEqual(g.ToString("N"), "de33ac6509cb465cad7e53124b2104e8", "N");
			Assert.AreEqual(g.ToString("D"), "de33ac65-09cb-465c-ad7e-53124b2104e8", "D");
			Assert.AreEqual(g.ToString("B"), "{de33ac65-09cb-465c-ad7e-53124b2104e8}", "B");
			Assert.AreEqual(g.ToString("P"), "(de33ac65-09cb-465c-ad7e-53124b2104e8)", "P");
			Assert.AreEqual(g.ToString(""), "de33ac65-09cb-465c-ad7e-53124b2104e8", "empty");
			Assert.AreEqual(g.ToString(null), "de33ac65-09cb-465c-ad7e-53124b2104e8", "null");
		}

		[Test]
		public void NewGuidWorks() {
			var d = new JsDictionary<string, object>();
			for (int i = 0; i < 1000; i++) {
				var g = Guid.NewGuid();
				Assert.IsTrue((object)g is Guid, "Generated Guid should be Guid");
				string s = g.ToString("N");
				Assert.IsTrue(s[16] == '8' || s[16] == '9' || s[16] == 'a' || s[16] == 'b', "Should be standard guid");
				Assert.IsTrue(s[12] == '4', "Should be type 4 guid");
				d[s] = null;
			}
			Assert.AreEqual(d.Count, 1000, "No duplicates should have been generated");
		}

		[Test]
		public void ToByteArrayWorks() {
			var g = new Guid("8440F854-0C0B-4355-9722-1608D62E8F87");
			Assert.AreEqual(g.ToByteArray(), new byte[] { 0x84, 0x40, 0xf8, 0x54, 0x0c, 0x0b, 0x43, 0x55, 0x97, 0x22, 0x16, 0x08, 0xd6, 0x2e, 0x8f, 0x87 });
		}
	}
}
