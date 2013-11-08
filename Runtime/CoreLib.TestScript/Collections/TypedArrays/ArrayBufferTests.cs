﻿using System;
using System.Collections.Generic;
using System.Collections.TypedArrays;
using QUnit;

namespace CoreLib.TestScript.Collections.TypedArrays {
	[TestFixture]
	public class ArrayBufferTests {
		private byte[] GetArray(ArrayBuffer b) {
			var result = new byte[b.ByteLength];
			var a = new Uint8Array(b);
			for (int i = 0; i < result.Length; i++)
				result[i] = a[i];
			return result;
		}

		[Test]
		public void TypePropertiesAreCorrect() {
			Assert.AreEqual(typeof(ArrayBuffer).FullName, "ArrayBuffer", "FullName");

			var interfaces = typeof(DataView).GetInterfaces();
			Assert.AreEqual(interfaces.Length, 0, "Interface count should be empty");
		}

		[Test]
		public void ConstructorWorks() {
			var buf = new ArrayBuffer(14);
			Assert.IsTrue((object)buf is ArrayBuffer, "is ArrayBuffer");
			Assert.AreEqual(buf.ByteLength, 14, "ByteLength");
		}

		[Test]
		public void ByteLengthPropertyWorks() {
			var buf = new ArrayBuffer(10);
			Assert.AreEqual(buf.ByteLength, 10, "ByteLength");
		}

		/*
		These tests don't work because Node does not have the ArrayBuffer.slice method (at least not v0.8.20)

		[Test]
		public void SliceWithBeginOnlyWorks() {
			var buf = new Uint8Array(new byte[] { 4, 67, 13, 22, 76, 58, 85 }).Buffer;
			Assert.AreEqual(GetArray(buf.Slice(3)), new byte[] { 22, 76, 58, 85 }, "3");
			Assert.AreEqual(GetArray(buf.Slice(-4)), new byte[] { 22, 76, 58, 85 }, "-4");
		}

		[Test]
		public void SliceWithBeginAndEndWorks() {
			var buf = new Uint8Array(new byte[] { 4, 67, 13, 22, 76, 58, 85, 45, 23 }).Buffer;
			Assert.AreEqual(GetArray(buf.Slice(3, 5)), new byte[] { 22, 76 }, "3, 5");
			Assert.AreEqual(GetArray(buf.Slice(3, -2)), new byte[] { 22, 76, 58, 85 }, "3, -2");
			Assert.AreEqual(GetArray(buf.Slice(-5, 8)), new byte[] { 76, 58, 85, 45 }, "-5, 8");
			Assert.AreEqual(GetArray(buf.Slice(-5, -2)), new byte[] { 76, 58, 85 }, "-5, -2");
		}*/
	}
}
