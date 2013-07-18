// NumberFormatInfo.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Globalization {

	[ScriptNamespace("ss")]
	[Imported(ObeysTypeSystem = true)]
	public sealed class NumberFormatInfo : IFormatProvider {
		private NumberFormatInfo() {
		}

		[IntrinsicProperty]
		public static NumberFormatInfo InvariantInfo {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string NaNSymbol {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string NegativeSign {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string PositiveSign {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string NegativeInfinitySymbol {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string PositiveInfinitySymbol {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string PercentSymbol {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public int[] PercentGroupSizes {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public int PercentDecimalDigits {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public string PercentDecimalSeparator {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string PercentGroupSeparator {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public int PercentPositivePattern {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public int PercentNegativePattern {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public string CurrencySymbol {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public int[] CurrencyGroupSizes {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public int CurrencyDecimalDigits {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public string CurrencyDecimalSeparator {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string CurrencyGroupSeparator {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public int CurrencyPositivePattern {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public int CurrencyNegativePattern {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public int[] NumberGroupSizes {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public int NumberDecimalDigits {
			get {
				return 0;
			}
		}

		[IntrinsicProperty]
		public string NumberDecimalSeparator {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string NumberGroupSeparator {
			get {
				return null;
			}
		}

		public object GetFormat(Type formatType) {
			return null;
		}
	}
}
