// Stopwatch.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Diagnostics {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class Stopwatch {
		public static readonly long Frequency = 0;
		public static readonly bool IsHighResolution = false;

		public static Stopwatch StartNew() {
			return null;
		}

		public TimeSpan Elapsed {
			[ScriptName("timeSpan")]
			get { return default(TimeSpan); }
		}

		public long ElapsedMilliseconds {
			[ScriptName("milliseconds")]
			get { return 0; }
		}

		public long ElapsedTicks {
			[ScriptName("ticks")]
			get { return 0; }
		}

		[IntrinsicProperty]
		public bool IsRunning {
			get { return false; }
		}

		public void Reset() {
		}

		public void Start() {
		}

		public void Stop() {
		}

		public void Restart() {
		}

		public static long GetTimestamp() {
			return 0;
		}
	}
}
