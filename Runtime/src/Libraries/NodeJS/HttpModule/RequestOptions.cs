using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NodeJS.HttpModule {
	[Imported]
	[Serializable]
	public class RequestOptions {
		public string Host { get; set; }

		public string Hostname { get; set; }

		public int? Port { get; set; }

		public string LocalAddress { get; set; }

		public string SocketPath { get; set; }

		public string Method { get; set; }

		public string Path { get; set; }

		public JsDictionary<string, string> Headers { get; set; }

		public string Auth { get; set; }

		/// <summary>
		/// Possible values are: Script.Undefined, an Agent, or 'false'
		/// </summary>
		public TypeOption<object, Agent, bool> Agent { get; set; }
	}
}
