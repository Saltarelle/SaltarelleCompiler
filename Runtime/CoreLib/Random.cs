using System.Runtime.CompilerServices;

namespace System {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public class Random {
		/// <summary>
		/// Initializes a new instance of the <see cref="Random"/> class, using a time-dependent default seed value.
		/// </summary>
		[ScriptName("")]
		public Random() {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Random"/> class, using the specified seed value.
		/// </summary>
		/// <param name="seed">
		/// A number used to calculate a starting value for the pseudo-random number sequence.
		/// If a negative number is specified, the absolute value of the number is used.
		/// </param>
		[ScriptName("")]
		public Random(int seed) {
		}

		/// <summary>
		/// Returns a nonnegative random number.
		/// </summary>
		public virtual int Next() {
			return 0;
		}

		/// <summary>
		/// Returns a nonnegative random number less than the specified maximum.
		/// </summary>
		[ScriptName("nextMax")]
		public virtual int Next(int maxValue) {
			return 0;
		}

		/// <summary>
		/// Returns a random number within a specified range.
		/// </summary>
		[ScriptName("nextMinMax")]
		public virtual int Next(int minValue, int maxValue) {
			return 0;
		}

		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		public virtual void NextBytes(byte[] buffer) {
		}

		/// <summary>
		/// Returns a random number between 0.0 and 1.0.
		/// </summary>
		/// <returns></returns>
		public virtual double NextDouble() {
			return 0;
		}

		/// <summary>
		/// Returns a random number between 0.0 and 1.0.
		/// </summary>
		protected virtual double Sample() {
			return 0;
		}
	}
}
