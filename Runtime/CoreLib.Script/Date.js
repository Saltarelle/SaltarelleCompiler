///////////////////////////////////////////////////////////////////////////////
// Date Extensions

ss.utcNow = function#? DEBUG ss$utcNow##() {
	var d = new Date();
	return new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate(), d.getHours(), d.getMinutes(), d.getSeconds(), d.getMilliseconds()));
};

ss.today = function#? DEBUG ss$today##() {
	var d = new Date();
	return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}

ss.formatDate = function#? DEBUG ss$formatDate##(date, format) {
	if (ss.isNullOrUndefined(format) || (format.length == 0) || (format == 'i')) {
		return date.toString();
	}
	if (format == 'id') {
		return date.toDateString();
	}
	if (format == 'it') {
		return date.toTimeString();
	}

	return ss._netFormatDate(date, format, false);
};

ss.localeFormatDate = function#? DEBUG ss$localeFormatDate##(date, format) {
	if (ss.isNullOrUndefined(format) || (format.length == 0) || (format == 'i')) {
		return date.toLocaleString();
	}
	if (format == 'id') {
		return date.toLocaleDateString();
	}
	if (format == 'it') {
		return date.toLocaleTimeString();
	}

	return ss._netFormatDate(date, format, true);
};

ss._netFormatDate = function#? DEBUG ss$_netFormatDate##(dt, format, useLocale) {
	var dtf = useLocale ? ss_CultureInfo.CurrentCulture.dateTimeFormat : ss_CultureInfo.InvariantCulture.dateTimeFormat;

	if (format.length == 1) {
		switch (format) {
			case 'f': format = dtf.longDatePattern + ' ' + dtf.shortTimePattern; break;
			case 'F': format = dtf.dateTimePattern; break;

			case 'd': format = dtf.shortDatePattern; break;
			case 'D': format = dtf.longDatePattern; break;

			case 't': format = dtf.shortTimePattern; break;
			case 'T': format = dtf.longTimePattern; break;

			case 'g': format = dtf.shortDatePattern + ' ' + dtf.shortTimePattern; break;
			case 'G': format = dtf.shortDatePattern + ' ' + dtf.longTimePattern; break;

			case 'R': case 'r':
				dtf = ss_CultureInfo.InvariantCulture.dateTimeFormat;
				format = dtf.gmtDateTimePattern;
				break;
			case 'u': format = dtf.universalDateTimePattern; break;
			case 'U':
				format = dtf.dateTimePattern;
				dt = new Date(dt.getUTCFullYear(), dt.getUTCMonth(), dt.getUTCDate(),
							  dt.getUTCHours(), dt.getUTCMinutes(), dt.getUTCSeconds(), dt.getUTCMilliseconds());
				break;

			case 's': format = dtf.sortableDateTimePattern; break;
		}
	}

	if (format.charAt(0) == '%') {
		format = format.substr(1);
	}

	if (!Date._formatRE) {
		Date._formatRE = /'.*?[^\\]'|dddd|ddd|dd|d|MMMM|MMM|MM|M|yyyy|yy|y|hh|h|HH|H|mm|m|ss|s|tt|t|fff|ff|f|zzz|zz|z/g;
	}

	var re = Date._formatRE;
	var sb = new ss_StringBuilder();

	re.lastIndex = 0;
	while (true) {
		var index = re.lastIndex;
		var match = re.exec(format);

		sb.append(format.slice(index, match ? match.index : format.length));
		if (!match) {
			break;
		}

		var fs = match[0];
		var part = fs;
		switch (fs) {
			case 'dddd':
				part = dtf.dayNames[dt.getDay()];
				break;
			case 'ddd':
				part = dtf.shortDayNames[dt.getDay()];
				break;
			case 'dd':
				part = ss.padLeftString(dt.getDate().toString(), 2, 0x30);
				break;
			case 'd':
				part = dt.getDate();
				break;
			case 'MMMM':
				part = dtf.monthNames[dt.getMonth()];
				break;
			case 'MMM':
				part = dtf.shortMonthNames[dt.getMonth()];
				break;
			case 'MM':
				part = ss.padLeftString((dt.getMonth() + 1).toString(), 2, 0x30);
				break;
			case 'M':
				part = (dt.getMonth() + 1);
				break;
			case 'yyyy':
				part = dt.getFullYear();
				break;
			case 'yy':
				part = ss.padLeftString((dt.getFullYear() % 100).toString(), 2, 0x30);
				break;
			case 'y':
				part = (dt.getFullYear() % 100);
				break;
			case 'h': case 'hh':
				part = dt.getHours() % 12;
				if (!part) {
					part = '12';
				}
				else if (fs == 'hh') {
					part = ss.padLeftString(part.toString(), 2, 0x30);
				}
				break;
			case 'HH':
				part = ss.padLeftString(dt.getHours().toString(), 2, 0x30);
				break;
			case 'H':
				part = dt.getHours();
				break;
			case 'mm':
				part = ss.padLeftString(dt.getMinutes().toString(), 2, 0x30);
				break;
			case 'm':
				part = dt.getMinutes();
				break;
			case 'ss':
				part = ss.padLeftString(dt.getSeconds().toString(), 2, 0x30);
				break;
			case 's':
				part = dt.getSeconds();
				break;
			case 't': case 'tt':
				part = (dt.getHours() < 12) ? dtf.amDesignator : dtf.pmDesignator;
				if (fs == 't') {
					part = part.charAt(0);
				}
				break;
			case 'fff':
				part = ss.padLeftString(dt.getMilliseconds().toString(), 3, 0x30);
				break;
			case 'ff':
				part = ss.padLeftString(dt.getMilliseconds().toString(), 3).substr(0, 2);
				break;
			case 'f':
				part = ss.padLeftString(dt.getMilliseconds().toString(), 3).charAt(0);
				break;
			case 'z':
				part = dt.getTimezoneOffset() / 60;
				part = ((part >= 0) ? '-' : '+') + Math.floor(Math.abs(part));
				break;
			case 'zz': case 'zzz':
				part = dt.getTimezoneOffset() / 60;
				part = ((part >= 0) ? '-' : '+') + Math.floor(ss.padLeftString(Math.abs(part)).toString(), 2, 0x30);
				if (fs == 'zzz') {
					part += dtf.timeSeparator + Math.abs(ss.padLeftString(dt.getTimezoneOffset() % 60).toString(), 2, 0x30);
				}
				break;
			default:
				if (part.charAt(0) == '\'') {
					part = part.substr(1, part.length - 2).replace(/\\'/g, '\'');
				}
				break;
		}
		sb.append(part);
	}

	return sb.toString();
};

ss._parseExactDate = function#? DEBUG ss$_parseExactDate##(val, format, culture, utc) {
	culture = culture || ss_CultureInfo.CurrentCulture;
	var AM = culture.amDesignator, PM = culture.pmDesignator;

	var _isInteger = function(val) {
		var digits="1234567890";
		for (var i=0; i < val.length; i++) {
			if (digits.indexOf(val.charAt(i))==-1) {
				return false;
			}
		}
		return true;
	};

	var _getInt = function(str,i,minlength,maxlength) {
		for (var x=maxlength; x>=minlength; x--) {
			var token=str.substring(i,i+x);
			if (token.length < minlength) {
				return null;
			}
			if (_isInteger(token)) {
				return token;
			}
		}
		return null;
	};

	val = val + "";
	format = format + "";
	var i_val = 0;
	var i_format = 0;
	var c = "";
	var token = "";

	var year = 0, month = 1, date = 1, hh = 0, mm = 0, _ss = 0, ampm = "";
		
	while (i_format < format.length) {
		// Get next token from format string
		c = format.charAt(i_format);
		token = "";
		while ((format.charAt(i_format) == c) && (i_format < format.length)) {
			token += format.charAt(i_format++);
		}
		// Extract contents of value based on format token
		if (token=="yyyy" || token=="yy" || token=="y") {
			if (token == "yyyy")
				year = _getInt(val, i_val, 4, 4);
			if (token == "yy")
				year = _getInt(val, i_val, 2, 2);
			if (token == "y")
				year = _getInt(val, i_val, 2, 4);

			if (year == null)
				return null;

			i_val += year.length;
			if (year.length == 2) {
				if (year > 30) {
					year = 1900 + (year-0);
				}
				else {
					year = 2000 + (year-0);
				}
			}
		}
		else if (token == "MM" || token == "M") {
			month = _getInt(val, i_val, token.length, 2);
			if (month == null || (month < 1) || (month > 12))
				return null;
			i_val += month.length;
		}
		else if (token=="dd"||token=="d") {
			date = _getInt(val, i_val, token.length, 2);
			if (date == null || (date < 1) || (date > 31))
				return null;
			i_val += date.length;
		}
		else if (token=="hh"||token=="h") {
			hh = _getInt(val, i_val, token.length, 2);
			if (hh == null || (hh < 1) || (hh > 12))
				return null;
			i_val += hh.length;
		}
		else if (token=="HH"||token=="H") {
			hh = _getInt(val, i_val, token.length, 2);
			if (hh == null || (hh < 0) || (hh > 23))
				return null;
			i_val += hh.length;
		}
		else if (token == "mm" || token == "m") {
			mm = _getInt(val, i_val, token.length, 2);
			if (mm == null || (mm < 0) || (mm > 59))
				return null;
			i_val += mm.length;
		}
		else if (token == "ss" || token == "s") {
			_ss = _getInt(val, i_val, token.length, 2);
			if (_ss == null || (_ss < 0) || (_ss > 59))
				return null;
			i_val += _ss.length;
		}
		else if (token == "t") {
			if (val.substring(i_val, i_val + 1).toLowerCase() == AM.charAt(0).toLowerCase())
				ampm = AM;
			else if (val.substring(i_val, i_val + 1).toLowerCase() == PM.charAt(0).toLowerCase())
				ampm = PM;
			else
				return null;
			i_val += 1;
		}
		else if (token == "tt") {
			if (val.substring(i_val, i_val + 2).toLowerCase() == AM.toLowerCase())
				ampm = AM;
			else if (val.substring(i_val,i_val+2).toLowerCase() == PM.toLowerCase())
				ampm = PM;
			else
				return null;
			i_val += 2;
		}
		else {
			if (val.substring(i_val, i_val + token.length) != token)
				return null;
			else
				i_val += token.length;
		}
	}
	// If there are any trailing characters left in the value, it doesn't match
	if (i_val != val.length)
		return null;

	// Is date valid for month?
	if (month == 2) {
		// Check for leap year
		if (((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0)) { // leap year
			if (date > 29)
				return null;
		}
		else if (date > 28)
			return null;
	}
	if ((month == 4) || (month == 6) || (month == 9) || (month == 11)) {
		if (date > 30) {
			return null;
		}
	}
	// Correct hours value
	if (hh < 12 && ampm == PM) {
		hh = hh - 0 + 12;
	}
	else if (hh > 11 && ampm == AM) {
		hh -= 12;
	}

	if (utc)
		return new Date(Date.UTC(year, month - 1, date, hh, mm, _ss));
	else
		return new Date(year, month - 1, date, hh, mm, _ss);
};

ss.parseExactDate = function#? DEBUG ss$parseExactDate##(val, format, culture) {
	return ss._parseExactDate(val, format, culture, false);
};

ss.parseExactDateUTC = function#? DEBUG ss$parseExactDateUTC##(val, format, culture) {
	return ss._parseExactDate(val, format, culture, true);
};
