using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;

namespace NodeJS.ZLibModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("zlib")]
	public static class ZLib {
		public static ReadWriteStream CreateGzip() { return null; }

		public static ReadWriteStream CreateGzip(ZLibOptions options) { return null; }


		public static ReadWriteStream CreateGunzip() { return null; }

		public static ReadWriteStream CreateGunzip(ZLibOptions options) { return null; }


		public static ReadWriteStream CreateDeflate() { return null; }

		public static ReadWriteStream CreateDeflate(ZLibOptions options) { return null; }


		public static ReadWriteStream CreateInflate() { return null; }

		public static ReadWriteStream CreateInflate(ZLibOptions options) { return null; }


		public static ReadWriteStream CreateDeflateRaw() { return null; }

		public static ReadWriteStream CreateDeflateRaw(ZLibOptions options) { return null; }


		public static ReadWriteStream CreateInflateRaw() { return null; }

		public static ReadWriteStream CreateInflateRaw(ZLibOptions options) { return null; }


		public static ReadWriteStream CreateUnzip() { return null; }

		public static ReadWriteStream CreateUnzip(ZLibOptions options) { return null; }

#warning TODO: Task methods
		public static void Deflate(Buffer buf, Action<Error, Buffer> callback) {}

		public static Task<Buffer> DeflateTask(Buffer buf) { return null; }


		public static void DeflateRaw(Buffer buf, Action<Error, Buffer> callback) {}

		public static Task<Buffer> DeflateRawTask(Buffer buf) { return null; }


		public static void Gzip(Buffer buf, Action<Error, Buffer> callback) {}

		public static Task<Buffer> GzipTask(Buffer buf) { return null; }


		public static void Gunzip(Buffer buf, Action<Error, Buffer> callback) {}

		public static Task<Buffer> GunzipTask(Buffer buf) { return null; }


		public static void Inflate(Buffer buf, Action<Error, Buffer> callback) {}

		public static Task<Buffer> InflateTask(Buffer buf) { return null; }


		public static void InflateRaw(Buffer buf, Action<Error, Buffer> callback) {}

		public static Task<Buffer> InflateRawTask(Buffer buf) { return null; }


		public static void Unzip(Buffer buf, Action<Error, Buffer> callback) {}

		public static Task<Buffer> UnzipTask(Buffer buf) { return null; }

#warning TODO: Constants
/*
   // Constants
   export var Z_NO_FLUSH: number;
   export var Z_PARTIAL_FLUSH: number;
   export var Z_SYNC_FLUSH: number;
   export var Z_FULL_FLUSH: number;
   export var Z_FINISH: number;
   export var Z_BLOCK: number;
   export var Z_TREES: number;
   export var Z_OK: number;
   export var Z_STREAM_END: number;
   export var Z_NEED_DICT: number;
   export var Z_ERRNO: number;
   export var Z_STREAM_ERROR: number;
   export var Z_DATA_ERROR: number;
   export var Z_MEM_ERROR: number;
   export var Z_BUF_ERROR: number;
   export var Z_VERSION_ERROR: number;
   export var Z_NO_COMPRESSION: number;
   export var Z_BEST_SPEED: number;
   export var Z_BEST_COMPRESSION: number;
   export var Z_DEFAULT_COMPRESSION: number;
   export var Z_FILTERED: number;
   export var Z_HUFFMAN_ONLY: number;
   export var Z_RLE: number;
   export var Z_FIXED: number;
   export var Z_DEFAULT_STRATEGY: number;
   export var Z_BINARY: number;
   export var Z_TEXT: number;
   export var Z_ASCII: number;
   export var Z_UNKNOWN: number;
   export var Z_DEFLATED: number;
   export var Z_NULL: number;
*/
	}
}
