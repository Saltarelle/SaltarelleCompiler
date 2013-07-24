using System.Runtime.CompilerServices;

namespace System {
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

	    public static readonly TimeSpan Zero = default(TimeSpan);
		//TODO
		//public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);
		//public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);

		private TimeSpan(DummyTypeUsedToAddAttributeToDefaultValueTypeConstructor _) {
		}

		public TimeSpan(long ticks) {
		}

		[InlineCode("new {$System.TimeSpan}(((({hours} * 60 + {minutes}) * 60) + {seconds}) * 10000000)")]
		public TimeSpan(int hours, int minutes, int seconds) {
		}

		[InlineCode("new {$System.TimeSpan}((((({days} * 24 + {hours}) * 60 + {minutes}) * 60) + {seconds}) * 10000000)")]
		public TimeSpan(int days, int hours, int minutes, int seconds) {
		}

		[InlineCode("new {$System.TimeSpan}(((((({days} * 24 + {hours}) * 60 + {minutes}) * 60) + {seconds}) * 1000 + {milliseconds}) * 10000)")]
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

		[InlineCode("new {$System.TimeSpan}({this}.ticks + {ts}.ticks)")]
		public TimeSpan Add(TimeSpan ts) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({this}.ticks - {ts}.ticks)")]
		public TimeSpan Subtract(TimeSpan ts) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({$System.Math}.abs({this}.ticks))")]
		public TimeSpan Duration() {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}(-{this}.ticks)")]
		public TimeSpan Negate() {
			return default(TimeSpan);
		}

		[InlineCode("{t1}.compareTo({t2})")]
		public static int Compare(TimeSpan t1, TimeSpan t2) {
			return 0;
		}

		public int CompareTo(TimeSpan other) {
			return 0;
		}

		public bool Equals(TimeSpan other) {
			return false;
		}

		[InlineCode("{t1}.ticks === {t2}.ticks")]
		public static bool Equals(TimeSpan t1, TimeSpan t2) {
			return false;
		}

		[InlineCode("new {$System.TimeSpan}({value} * 864000000000)")]
		public static TimeSpan FromDays(double value) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({value} * 36000000000)")]
		public static TimeSpan FromHours(double value) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({value} * 10000)")]
		public static TimeSpan FromMilliseconds(double value) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({value} * 600000000)")]
		public static TimeSpan FromMinutes(double value) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({value} * 10000000)")]
		public static TimeSpan FromSeconds(double value) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({value})")]
		public static TimeSpan FromTicks(long value) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({t1}.ticks + {t2}.ticks)")]
		public static TimeSpan operator+(TimeSpan t1, TimeSpan t2) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({t1}.ticks - {t2}.ticks)")]
		public static TimeSpan operator-(TimeSpan t1, TimeSpan t2) {
			return default(TimeSpan);
		}

		[InlineCode("{t1}.ticks === {t2}.ticks")]
		public static bool operator==(TimeSpan t1, TimeSpan t2) {
			return false;
		}

		[InlineCode("{t1}.ticks !== {t2}.ticks")]
		public static bool operator!=(TimeSpan t1, TimeSpan t2) {
			return false;
		}

		[InlineCode("{t1}.ticks > {t2}.ticks")]
		public static bool operator>(TimeSpan t1, TimeSpan t2) {
			return false;
		}

		[InlineCode("{t1}.ticks >= {t2}.ticks")]
		public static bool operator>=(TimeSpan t1, TimeSpan t2) {
			return false;
		}

		[InlineCode("{t1}.ticks < {t2}.ticks")]
		public static bool operator<(TimeSpan t1, TimeSpan t2) {
			return false;
		}

		[InlineCode("{t1}.ticks <= {t2}.ticks")]
		public static bool operator<=(TimeSpan t1, TimeSpan t2) {
			return false;
		}

		[InlineCode("new {$System.TimeSpan}(-{ts}.ticks)")]
		public static TimeSpan operator-(TimeSpan ts) {
			return default(TimeSpan);
		}

		[InlineCode("new {$System.TimeSpan}({ts}.ticks)")]
		public static TimeSpan operator+(TimeSpan ts) {
			return default(TimeSpan);
		}
	}
}
