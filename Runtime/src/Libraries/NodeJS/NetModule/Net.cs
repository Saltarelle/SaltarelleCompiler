using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.NetModule {
	[GlobalMethods]
	[ModuleName("net")]
	public static class Net {
		public static Server CreateServer() { return null; }

		public static Server CreateServer(ServerOptions options) { return null; }

		public static Server CreateServer(Action<Socket> connectionListener) { return null; }

		public static Server CreateServer(ServerOptions options, Action<Socket> connectionListener) { return null; }


		public static Socket Connect(ConnectOptions options) { return null; }

		public static Socket Connect(ConnectOptions options, Action<Socket> connectListener) { return null; }

		public static Socket CreateConnection(ConnectOptions options) { return null; }

		public static Socket CreateConnection(ConnectOptions options, Action<Socket> connectListener) { return null; }


		public static Socket Connect(int port) { return null; }

		public static Socket Connect(int port, string host) { return null; }

		public static Socket Connect(int port, Action<Socket> connectListener) { return null; }

		public static Socket Connect(int port, string host, Action<Socket> connectListener) { return null; }

		public static Socket CreateConnection(int port) { return null; }

		public static Socket CreateConnection(int port, string host) { return null; }

		public static Socket CreateConnection(int port, Action<Socket> connectListener) { return null; }

		public static Socket CreateConnection(int port, string host, Action<Socket> connectListener) { return null; }


		public static bool IsIP(string input) { return false; }

		public static bool IsIPv4(string input) { return false; }

		public static bool IsIPv6(string input) { return false; }
	}
}
