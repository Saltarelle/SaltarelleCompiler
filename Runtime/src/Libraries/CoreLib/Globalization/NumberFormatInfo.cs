// NumberFormatInfo.cs
// Script#/Libraries/CoreLib
// This source code is subject to terms and conditions of the Apache License, Version 2.0.
//

using System.Runtime.CompilerServices;

namespace System.Globalization {

    [Imported]
	[Serializable]
    public sealed class NumberFormatInfo {

        private NumberFormatInfo() {
        }

        public string NaNSymbol {
            get {
                return null;
            }
        }

        public string NegativeSign {
            get {
                return null;
            }
        }

        public string PositiveSign {
            get {
                return null;
            }
        }

        public string NegativeInfinityText {
            get {
                return null;
            }
        }

        public string PositiveInfinityText {
            get {
                return null;
            }
        }

        public string PercentSymbol {
            get {
                return null;
            }
        }

        public int[] PercentGroupSizes {
            get {
                return null;
            }
        }

        public int PercentDecimalDigits {
            get {
                return 0;
            }
        }

        public string PercentDecimalSeparator {
            get {
                return null;
            }
        }

        public string PercentGroupSeparator {
            get {
                return null;
            }
        }

        public string PercentPositivePattern {
            get {
                return null;
            }
        }

        public string PercentNegativePattern {
            get {
                return null;
            }
        }

        public string CurrencySymbol {
            get {
                return null;
            }
        }

        public int[] CurrencyGroupSizes {
            get {
                return null;
            }
        }

        public int CurrencyDecimalDigits {
            get {
                return 0;
            }
        }

        public string CurrencyDecimalSeparator {
            get {
                return null;
            }
        }

        public string CurrencyGroupSeparator {
            get {
                return null;
            }
        }

        public string CurrencyPositivePattern {
            get {
                return null;
            }
        }

        public string CurrencyNegativePattern {
            get {
                return null;
            }
        }

        public int[] NumberGroupSizes {
            get {
                return null;
            }
        }

        public int NumberDecimalDigits {
            get {
                return 0;
            }
        }

        public string NumberDecimalSeparator {
            get {
                return null;
            }
        }

        public string NumberGroupSeparator {
            get {
                return null;
            }
        }
    }
}
