using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeJS.CryptoModule;

namespace NodeJS.TlsModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("tls")]
	public static class Tls {
		public static Server CreateServer(CreateServerOptions options) { return null; }
		public static Server CreateServer(CreateServerOptions options, Action<CleartextStream> secureConnectionListener) { return null; }

		public static CleartextStream Connect(ConnectOptions options) { return null; } 
		public static CleartextStream Connect(ConnectOptions options, Action secureConnectListener) { return null; } 
		public static CleartextStream Connect(int port) { return null; } 
		public static CleartextStream Connect(int port, Action secureConnectListener) { return null; } 
		public static CleartextStream Connect(int port, string host) { return null; } 
		public static CleartextStream Connect(int port, string host, Action secureConnectListener) { return null; } 
		public static CleartextStream Connect(int port, string host, ConnectOptions options) { return null; } 
		public static CleartextStream Connect(int port, string host, ConnectOptions options, Action secureConnectListener) { return null; }

		public static SecurePair CreateSecurePair(Credentials credentials) { return null; }
		public static SecurePair CreateSecurePair(Credentials credentials, bool isServer) { return null; }
		public static SecurePair CreateSecurePair(Credentials credentials, bool isServer, bool requestCert) { return null; }
		public static SecurePair CreateSecurePair(Credentials credentials, bool isServer, bool requestCert, bool rejectUnauthorized) { return null; }
	}
}
