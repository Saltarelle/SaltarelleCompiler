using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;
using NodeJS.EventsModule;
using NodeJS.NetModule;

namespace NodeJS.StringDecoderModule {
	[Imported]
	[ModuleName("string_decoder")]
	[IgnoreNamespace]
	public class StringDecoder {
		public StringDecoder() {}

		public StringDecoder(Encoding encoding) {}

		public string Write(Buffer buffer) { return null; }
	}
}
