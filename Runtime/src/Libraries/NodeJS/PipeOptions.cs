using System;
using System.Runtime.CompilerServices;

namespace NodeJS {
	[Serializable]
	[Imported]
	public class PipeOptions {
		public bool End { get; set; }

		public PipeOptions(bool end) {
		}
	}
}