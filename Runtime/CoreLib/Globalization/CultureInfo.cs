// CultureInfo.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Globalization {
	[Imported(ObeysTypeSystem = true)]
	[ScriptNamespace("ss")]
	public sealed class CultureInfo : IFormatProvider {
		private CultureInfo() {
		}

		[PreserveCase]
		[IntrinsicProperty]
		public static CultureInfo CurrentCulture {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public DateTimeFormatInfo DateTimeFormat {
			get {
				return null;
			}
		}

		[PreserveCase]
		[IntrinsicProperty]
		public static CultureInfo InvariantCulture {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public string Name {
			get {
				return null;
			}
		}

		[IntrinsicProperty]
		public NumberFormatInfo NumberFormat {
			get {
				return null;
			}
		}

		public object GetFormat(Type formatType) {
			return null;
		}
	}
}
