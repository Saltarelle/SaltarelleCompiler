using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.BufferModule {
	[Imported]
	[ModuleName("buffer")]
	[IgnoreNamespace]
	public class Buffer {
		public Buffer(int size) {}

		public Buffer(Array data) {}

		public Buffer(string data) {}

		public Buffer(string data, Encoding encoding) {}


		public int Write(string data) { return 0; }

		public int Write(string data, int offset) { return 0; }

		public int Write(string data, int offset, int length) { return 0; }

		public int Write(string data, int offset, int length, Encoding encoding) { return 0; }


		public string ToString(Encoding encoding) { return null; }

		public string ToString(Encoding encoding, int start) { return null; }

		public string ToString(Encoding encoding, int start, int end) { return null; }


		[IntrinsicProperty]
		public byte this[int index] { get { return 0; } set {} }

		[IntrinsicProperty]
		public int Length { get; private set; }


		public void Copy(Buffer targetBuffer) {}

		public void Copy(Buffer targetBuffer, int targetStart) {}

		public void Copy(Buffer targetBuffer, int targetStart, int sourceStart) {}

		public void Copy(Buffer targetBuffer, int targetStart, int sourceStart, int sourceEnd) {}


		public Buffer Slice() { return null; }

		public Buffer Slice(int start) { return null; }

		public Buffer Slice(int start, int end) { return null; }


		public byte ReadUInt8(int offset) { return 0; }
		public byte ReadUInt8(int offset, bool noAssert) { return 0; }

		public ushort ReadUInt16LE(int offset) { return 0; }
		public ushort ReadUInt16LE(int offset, bool noAssert) { return 0; }

		public ushort ReadUInt16BE(int offset) { return 0; }
		public ushort ReadUInt16BE(int offset, bool noAssert) { return 0; }

		public uint ReadUInt32LE(int offset) { return 0; }
		public uint ReadUInt32LE(int offset, bool noAssert) { return 0; }

		public uint ReadUInt32BE(int offset) { return 0; }
		public uint ReadUInt32BE(int offset, bool noAssert) { return 0; }

		public sbyte ReadInt8(int offset) { return 0; }
		public sbyte ReadInt8(int offset, bool noAssert) { return 0; }

		public short ReadInt16LE(int offset) { return 0; }
		public short ReadInt16LE(int offset, bool noAssert) { return 0; }

		public short ReadInt16BE(int offset) { return 0; }
		public short ReadInt16BE(int offset, bool noAssert) { return 0; }

		public int ReadInt32LE(int offset) { return 0; }
		public int ReadInt32LE(int offset, bool noAssert) { return 0; }

		public int ReadInt32BE(int offset) { return 0; }
		public int ReadInt32BE(int offset, bool noAssert) { return 0; }

		public float ReadFloatLE(int offset) { return 0; }
		public float ReadFloatLE(int offset, bool noAssert) { return 0; }

		public float ReadFloatBE(int offset) { return 0; }
		public float ReadFloatBE(int offset, bool noAssert) { return 0; }

		public double ReadDoubleLE(int offset) { return 0; }
		public double ReadDoubleLE(int offset, bool noAssert) { return 0; }

		public double ReadDoubleBE(int offset) { return 0; }
		public double ReadDoubleBE(int offset, bool noAssert) { return 0; }


		public void WriteUInt8(byte value, int offset) {}
		public void WriteUInt8(byte value, int offset, bool noAssert) {}

		public void WriteUInt16LE(ushort value, int offset) {}
		public void WriteUInt16LE(ushort value, int offset, bool noAssert) {}

		public void WriteUInt16BE(ushort value, int offset) {}
		public void WriteUInt16BE(ushort value, int offset, bool noAssert) {}

		public void WriteUInt32LE(uint value, int offset) {}
		public void WriteUInt32LE(uint value, int offset, bool noAssert) {}

		public void WriteUInt32BE(uint value, int offset) {}
		public void WriteUInt32BE(uint value, int offset, bool noAssert) {}

		public void WriteInt8(sbyte value, int offset) {}
		public void WriteInt8(sbyte value, int offset, bool noAssert) {}

		public void WriteInt16LE(short value, int offset) {}
		public void WriteInt16LE(short value, int offset, bool noAssert) {}

		public void WriteInt16BE(short value, int offset) {}
		public void WriteInt16BE(short value, int offset, bool noAssert) {}

		public void WriteInt32LE(int value, int offset) {}
		public void WriteInt32LE(int value, int offset, bool noAssert) {}

		public void WriteInt32BE(int value, int offset) {}
		public void WriteInt32BE(int value, int offset, bool noAssert) {}

		public void WriteFloatLE(float value, int offset) {}
		public void WriteFloatLE(float value, int offset, bool noAssert) {}

		public void WriteFloatBE(float value, int offset) {}
		public void WriteFloatBE(float value, int offset, bool noAssert) {}

		public void WriteDoubleLE(double value, int offset) {}
		public void WriteDoubleLE(double value, int offset, bool noAssert) {}

		public void WriteDoubleBE(double value, int offset) {}
		public void WriteDoubleBE(double value, int offset, bool noAssert) {}


		public void Fill(byte value) {}

		public void Fill(byte value, int offset) {}

		public void Fill(byte value, int offset, int end) {}


		public void Fill(string value) {}

		public void Fill(string value, int offset) {}

		public void Fill(string value, int offset, int end) {}


		public static bool IsBuffer(object obj) { return false; }

		public static int ByteLength(string data) { return 0; }

		public static int ByteLength(string data, Encoding encoding) { return 0; }

		public static Buffer Concat(Buffer[] buffers) { return null; }

		public static Buffer Concat(Buffer[] buffers, int totalLength) { return null; }
	}
}
