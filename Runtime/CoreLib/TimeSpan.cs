// TimeSpan.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System
{
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public struct TimeSpan : IComparable<TimeSpan>, IEquatable<TimeSpan> {
		[InlineConstant]
		public const long TicksPerMillisecond = 10000L;
		[InlineConstant]
		public const long TicksPerSecond = 10000000L;
		[InlineConstant]
		public const long TicksPerMinute = 600000000L;
		[InlineConstant]
		public const long TicksPerHour = 36000000000L;
		[InlineConstant]
		public const long TicksPerDay = 864000000000L;
		public static readonly TimeSpan Zero = new TimeSpan(0);
		//TODO
		//public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);
		//public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);

		[ScriptName("")]
		public TimeSpan(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		[ScriptName("")]
		public TimeSpan(long ticks) {
		}

		[InlineCode("{$System.TimeSpan}.fromValues(0, {hours}, {minutes}, {seconds}, 0)")]
		public TimeSpan(int hours, int minutes, int seconds) {
		}

		[InlineCode("{$System.TimeSpan}.fromValues({days}, {hours}, {minutes}, {seconds}, 0)")]
		public TimeSpan(int days, int hours, int minutes, int seconds) {
		}

		[InlineCode("{$System.TimeSpan}.fromValues({days}, {hours}, {minutes}, {seconds}, {milliseconds})")]
		public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds) {
		}

		public int Days {
			[InlineCode("{this}.ticks / 864000000000 | 0")]
			get { return 0; }
		}

		public int Hours {
			[InlineCode("{this}.ticks / 36000000000 % 24 | 0")]
			get { return 0; }
		}

		public int Milliseconds {
			[InlineCode("{this}.ticks / 10000 % 1000 | 0")]
			get { return 0; }
		}

		public int Minutes {
			[InlineCode("{this}.ticks / 600000000 % 60 | 0")]
			get { return 0; }
		}

		public int Seconds {
			[InlineCode("{this}.ticks / 10000000 % 60 | 0")]
			get { return 0; }
		}

		[IntrinsicProperty]
		public long Ticks {
			get { return 0; }
		}

		public double TotalDays {
			[InlineCode("{this}.ticks / 864000000000")]
			get { return 0; }
		}
		
		public double TotalHours {
			[InlineCode("{this}.ticks / 36000000000")]
			get { return 0; }
		}
		
		public double TotalMilliseconds {
			[InlineCode("{this}.ticks / 10000")]
			get { return 0; }
		}
		
		public double TotalMinutes {
			[InlineCode("{this}.ticks / 600000000")]
			get { return 0; }
		}
		
		public double TotalSeconds {
			[InlineCode("{this}.ticks / 10000000")]
			get { return 0; }
		}

		public int CompareTo(TimeSpan other) {
			return 0;
		}

		public bool Equals(TimeSpan other) {
			return false;
		}

		public string ToString(string format) {
			return null;
		}

		[InlineCode("new {$System.TimeSpan}({value})")]
		public static TimeSpan FromTicks(long value) {
			return default(TimeSpan);
		}
	}
}
