///////////////////////////////////////////////////////////////////////////////
// NumberFormatInfo

var ss_NumberFormatInfo = function#? DEBUG NumberFormatInfo$##() {
};

ss_NumberFormatInfo.__typeName = 'ss.NumberFormatInfo';
ss.NumberFormatInfo = ss_NumberFormatInfo;
ss.initClass(ss_NumberFormatInfo, ss, {
	getFormat:  function#? DEBUG NumberFormatInfo$getFormat##(type) {
		return (type === ss_NumberFormatInfo) ? this : null;
	},
	
    getInstance: function#? DEBUG NumberFormatInfo$getInstance##(formatProvider) {
        if (ss.isInstanceOfType(formatProvider, ss_CultureInfo)) {
            return formatProvider.numberFormat;
        }
        else if (ss.isInstanceOfType(formatProvider, ss_NumberFormatInfo)) {
            return formatProvider;
        }
        else if (formatProvider != null) {
            numberFormatInfo = formatProvider.getFormat(ss_NumberFormatInfo);
            if (numberFormatInfo != null) {
                return numberFormatInfo;
            }
        }
        else {
            return ss_CultureInfo.CurrentCulture.numberFormat;
        }
    }
}, null, [ss_IFormatProvider]);

ss_NumberFormatInfo.invariantInfo = new ss_NumberFormatInfo();
ss.shallowCopy({
	naNSymbol: 'NaN',
	negativeSign: '-',
	positiveSign: '+',
	negativeInfinitySymbol: '-Infinity',
	positiveInfinitySymbol: 'Infinity',

	percentSymbol: '%',
	percentGroupSizes: [3],
	percentDecimalDigits: 2,
	percentDecimalSeparator: '.',
	percentGroupSeparator: ',',
	percentPositivePattern: 0,
	percentNegativePattern: 0,

	currencySymbol: '$',
	currencyGroupSizes: [3],
	currencyDecimalDigits: 2,
	currencyDecimalSeparator: '.',
	currencyGroupSeparator: ',',
	currencyNegativePattern: 0,
	currencyPositivePattern: 0,

	numberGroupSizes: [3],
	numberDecimalDigits: 2,
	numberDecimalSeparator: '.',
	numberGroupSeparator: ','
}, ss_NumberFormatInfo.invariantInfo);
