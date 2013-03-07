// TimeSpan.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System
{
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public struct TimeSpan : IComparable<TimeSpan>, IEquatable<TimeSpan>, IFormattable
	{
    public const long TicksPerMillisecond = 10000L;
    public const long TicksPerSecond = 10000000L;
    public const long TicksPerMinute = 600000000L;
    public const long TicksPerHour = 36000000000L;
    public const long TicksPerDay = 864000000000L;
    public static readonly TimeSpan Zero = new TimeSpan(0);
		//TODO
		//public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);
		//public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);

		[InlineCode("{$System.TimeSpan}.fromTicks({ticks})")]
		public TimeSpan(long ticks)
		{
		}

		[InlineCode("{$System.TimeSpan}.fromValues(0, {hours}, {minutes}, {seconds}, 0)")]
		public TimeSpan(int hours, int minutes, int seconds)
		{
		}

		[InlineCode("{$System.TimeSpan}.fromValues({days}, {hours}, {minutes}, {seconds}, 0)")]
		public TimeSpan(int days, int hours, int minutes, int seconds)
		{
		}

		[InlineCode("{$System.TimeSpan}.fromValues({days}, {hours}, {minutes}, {seconds}, {milliseconds})")]
		public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
		{
		}

		[IntrinsicProperty]
		public int Days
		{
			get { return 0; }
		}

		[IntrinsicProperty]
		public int Hours
		{
			get { return 0; }
		}

		[IntrinsicProperty]
		public int Milliseconds
		{
			get { return 0; }
		}

		[IntrinsicProperty]
		public int Minutes
		{
			get { return 0; }
		}

		[IntrinsicProperty]
		public int Seconds
		{
			get { return 0; }
		}

		[IntrinsicProperty]
		public long Ticks
		{
			get { return 0; }
		}

		[IntrinsicProperty]
		public double TotalDays
		{
			get { return 0; }
		}
		
		[IntrinsicProperty]
		public double TotalHours
		{
			get { return 0; }
		}
		
		[IntrinsicProperty]
		public double TotalMilliseconds
		{
			get { return 0; }
		}
		
		[IntrinsicProperty]
		public double TotalMinutes
		{
			get { return 0; }
		}
		
		[IntrinsicProperty]
		public double TotalSeconds
		{
			get { return 0; }
		}

		public int CompareTo(TimeSpan other)
		{
			return 0;
		}

		public bool Equals(TimeSpan other)
		{
			return false;
		}

		public string ToString(string format)
		{
			return null;
		}

		public static TimeSpan FromTicks(long value)
		{
			return default(TimeSpan);
		}
	}
}
