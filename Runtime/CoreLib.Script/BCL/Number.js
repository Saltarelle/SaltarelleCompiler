///////////////////////////////////////////////////////////////////////////////
// Number Extensions

ss.formatNumber = function#? DEBUG ss$formatNumber##(num, format) {
	if (ss.isNullOrUndefined(format) || (format.length == 0) || (format == 'i')) {
		return num.toString();
	}
	return ss._netFormatNumber(num, format, false);
};

ss.localeFormatNumber = function#? DEBUG ss$localeFormatNumber##(num, format) {
	if (ss.isNullOrUndefined(format) || (format.length == 0) || (format == 'i')) {
		return num.toLocaleString();
	}
	return ss._netFormatNumber(num, format, true);
};

ss._commaFormatNumber = function#? DEBUG ss$_commaFormat##(number, groups, decimal, comma) {
	var decimalPart = null;
	var decimalIndex = number.indexOf(decimal);
	if (decimalIndex > 0) {
		decimalPart = number.substr(decimalIndex);
		number = number.substr(0, decimalIndex);
	}

	var negative = number.startsWith('-');
	if (negative) {
		number = number.substr(1);
	}

	var groupIndex = 0;
	var groupSize = groups[groupIndex];
	if (number.length < groupSize) {
		return (negative ? '-' : '') + (decimalPart ? number + decimalPart : number);
	}

	var index = number.length;
	var s = '';
	var done = false;
	while (!done) {
		var length = groupSize;
		var startIndex = index - length;
		if (startIndex < 0) {
			groupSize += startIndex;
			length += startIndex;
			startIndex = 0;
			done = true;
		}
		if (!length) {
			break;
		}
		
		var part = number.substr(startIndex, length);
		if (s.length) {
			s = part + comma + s;
		}
		else {
			s = part;
		}
		index -= length;

		if (groupIndex < groups.length - 1) {
			groupIndex++;
			groupSize = groups[groupIndex];
		}
	}

	if (negative) {
		s = '-' + s;
	}    
	return decimalPart ? s + decimalPart : s;
};

ss._netFormatNumber = function#? DEBUG ss$_netFormatNumber##(num, format, useLocale) {
	var nf = useLocale ? ss_CultureInfo.CurrentCulture.numberFormat : ss_CultureInfo.InvariantCulture.numberFormat;

	var s = '';    
	var precision = -1;
	
	if (format.length > 1) {
		precision = parseInt(format.substr(1));
	}

	var fs = format.charAt(0);
	switch (fs) {
		case 'd': case 'D':
			s = parseInt(Math.abs(num)).toString();
			if (precision != -1) {
				s = s.padLeft(precision, 0x30);
			}
			if (num < 0) {
				s = '-' + s;
			}
			break;
		case 'x': case 'X':
			s = parseInt(Math.abs(num)).toString(16);
			if (fs == 'X') {
				s = s.toUpperCase();
			}
			if (precision != -1) {
				s = s.padLeft(precision, 0x30);
			}
			break;
		case 'e': case 'E':
			if (precision == -1) {
				s = num.toExponential();
			}
			else {
				s = num.toExponential(precision);
			}
			if (fs == 'E') {
				s = s.toUpperCase();
			}
			break;
		case 'f': case 'F':
		case 'n': case 'N':
			if (precision == -1) {
				precision = nf.numberDecimalDigits;
			}
			s = num.toFixed(precision).toString();
			if (precision && (nf.numberDecimalSeparator != '.')) {
				var index = s.indexOf('.');
				s = s.substr(0, index) + nf.numberDecimalSeparator + s.substr(index + 1);
			}
			if ((fs == 'n') || (fs == 'N')) {
				s = ss._commaFormatNumber(s, nf.numberGroupSizes, nf.numberDecimalSeparator, nf.numberGroupSeparator);
			}
			break;
		case 'c': case 'C':
			if (precision == -1) {
				precision = nf.currencyDecimalDigits;
			}
			s = Math.abs(num).toFixed(precision).toString();
			if (precision && (nf.currencyDecimalSeparator != '.')) {
				var index = s.indexOf('.');
				s = s.substr(0, index) + nf.currencyDecimalSeparator + s.substr(index + 1);
			}
			s = ss._commaFormatnumber(s, nf.currencyGroupSizes, nf.currencyDecimalSeparator, nf.currencyGroupSeparator);
			if (num < 0) {
				s = String.format(nf.currencyNegativePattern, s);
			}
			else {
				s = String.format(nf.currencyPositivePattern, s);
			}
			break;
		case 'p': case 'P':
			if (precision == -1) {
				precision = nf.percentDecimalDigits;
			}
			s = (Math.abs(num) * 100.0).toFixed(precision).toString();
			if (precision && (nf.percentDecimalSeparator != '.')) {
				var index = s.indexOf('.');
				s = s.substr(0, index) + nf.percentDecimalSeparator + s.substr(index + 1);
			}
			s = ss._commaFormatNumber(s, nf.percentGroupSizes, nf.percentDecimalSeparator, nf.percentGroupSeparator);
			if (num < 0) {
				s = String.format(nf.percentNegativePattern, s);
			}
			else {
				s = String.format(nf.percentPositivePattern, s);
			}
			break;
	}

	return s;
};
