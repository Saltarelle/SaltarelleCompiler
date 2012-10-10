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

		
		public static void Deflate(Buffer buf, Action<Error, Buffer> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.ZLibModule.ZLib}, 'deflate', {buf})")]
		public static Task<Buffer> DeflateTask(Buffer buf) { return null; }


		public static void DeflateRaw(Buffer buf, Action<Error, Buffer> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.ZLibModule.ZLib}, 'deflateRaw', {buf})")]
		public static Task<Buffer> DeflateRawTask(Buffer buf) { return null; }


		public static void Gzip(Buffer buf, Action<Error, Buffer> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.ZLibModule.ZLib}, 'gzip', {buf})")]
		public static Task<Buffer> GzipTask(Buffer buf) { return null; }


		public static void Gunzip(Buffer buf, Action<Error, Buffer> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.ZLibModule.ZLib}, 'gunzip', {buf})")]
		public static Task<Buffer> GunzipTask(Buffer buf) { return null; }


		public static void Inflate(Buffer buf, Action<Error, Buffer> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.ZLibModule.ZLib}, 'inflate', {buf})")]
		public static Task<Buffer> InflateTask(Buffer buf) { return null; }


		public static void InflateRaw(Buffer buf, Action<Error, Buffer> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.ZLibModule.ZLib}, 'inflateRaw', {buf})")]
		public static Task<Buffer> InflateRawTask(Buffer buf) { return null; }


		public static void Unzip(Buffer buf, Action<Error, Buffer> callback) {}

		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.ZLibModule.ZLib}, 'unzip', {buf})")]
		public static Task<Buffer> UnzipTask(Buffer buf) { return null; }


		[ScriptName("Z_NO_FLUSH")]            public static int ZNoFlush;
		[ScriptName("Z_PARTIAL_FLUSH")]       public static int ZPartialFlush;
		[ScriptName("Z_SYNC_FLUSH")]          public static int ZSyncFlush;
		[ScriptName("Z_FULL_FLUSH")]          public static int ZFullFlush;
		[ScriptName("Z_FINISH")]              public static int ZFinish;
		[ScriptName("Z_BLOCK")]               public static int ZBlock;
		[ScriptName("Z_TREES")]               public static int Ztrees;
		[ScriptName("Z_OK")]                  public static int ZOK;
		[ScriptName("Z_STREAM_END")]          public static int ZStreamEnd;
		[ScriptName("Z_NEED_DICT")]           public static int ZNeedDict;
		[ScriptName("Z_ERRNO")]               public static int ZErrno;
		[ScriptName("Z_STREAM_ERROR")]        public static int ZStreamError;
		[ScriptName("Z_DATA_ERROR")]          public static int ZDataError;
		[ScriptName("Z_MEM_ERROR")]           public static int ZMemError;
		[ScriptName("Z_BUF_ERROR")]           public static int ZBufError;
		[ScriptName("Z_VERSION_ERROR")]       public static int ZVersionError;
		[ScriptName("Z_NO_COMPRESSION")]      public static int ZNoCompression;
		[ScriptName("Z_BEST_SPEED")]          public static int ZBestSpeed;
		[ScriptName("Z_BEST_COMPRESSION")]    public static int ZBestCompression;
		[ScriptName("Z_DEFAULT_COMPRESSION")] public static int ZDefaultCompression;
		[ScriptName("Z_FILTERED")]            public static int ZFiltered;
		[ScriptName("Z_HUFFMAN_ONLY")]        public static int ZHuffmanOnly;
		[ScriptName("Z_RLE")]                 public static int ZRle;
		[ScriptName("Z_FIXED")]               public static int ZFixed;
		[ScriptName("Z_DEFAULT_STRATEGY")]    public static int ZDefaultStrategy;
		[ScriptName("Z_BINARY")]              public static int ZBinary;
		[ScriptName("Z_TEXT")]                public static int ZText;
		[ScriptName("Z_ASCII")]               public static int ZAscii;
		[ScriptName("Z_UNKNOWN")]             public static int ZUnknown;
		[ScriptName("Z_DEFLATED")]            public static int ZDeflated;
		[ScriptName("Z_NULL")]                public static int ZNull;
	}
}
