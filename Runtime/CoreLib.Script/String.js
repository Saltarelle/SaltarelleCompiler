///////////////////////////////////////////////////////////////////////////////
// String Extensions

ss.compareStrings = function#? DEBUG ss$compareStrings##(s1, s2, ignoreCase) {
	if (ignoreCase) {
		if (s1) {
			s1 = s1.toUpperCase();
		}
		if (s2) {
			s2 = s2.toUpperCase();
		}
	}
	s1 = s1 || '';
	s2 = s2 || '';

	if (s1 == s2) {
		return 0;
	}
	if (s1 < s2) {
		return -1;
	}
	return 1;
};

ss.concatStrings = function#? DEBUG ss$concatStrings##() {
	return Array.prototype.join.call(arguments, '');
};

ss.endsWithString = function#? DEBUG ss$endsWithString##(s, suffix) {
	if (!suffix.length) {
		return true;
	}
	if (suffix.length > s.length) {
		return false;
	}
	return (s.substr(s.length - suffix.length) == suffix);
};

ss._formatString = function#? DEBUG ss$_formatString##(format, values, useLocale) {
	if (!ss._formatRE) {
		ss._formatRE = /(\{[^\}^\{]+\})/g;
	}

	return format.replace(ss._formatRE,
						  function(str, m) {
							  var index = parseInt(m.substr(1));
							  var value = values[index + 1];
							  if (ss.isNullOrUndefined(value)) {
								  return '';
							  }
							  if (ss.isInstanceOfType(value, ss_IFormattable)) {
								  var formatSpec = null;
								  var formatIndex = m.indexOf(':');
								  if (formatIndex > 0) {
									  formatSpec = m.substring(formatIndex + 1, m.length - 1);
								  }
								  return ss.format(value, formatSpec);
							  }
							  else {
								  return useLocale ? value.toLocaleString() : value.toString();
							  }
						  });
};

ss.formatString = function#? DEBUG String$format##(format) {
	return ss._formatString(format, arguments, /* useLocale */ false);
};

ss.stringFromChar = function#? DEBUG ss$stringFromChar##(ch, count) {
	var s = ch;
	for (var i = 1; i < count; i++) {
		s += ch;
	}
	return s;
};

ss.htmlDecode = function#? DEBUG ss$htmlDecode##(s) {
	var div = document.createElement('div');
	div.innerHTML = s;
	return div.textContent || div.innerText;
};

ss.htmlEncode = function#? DEBUG ss$htmlEncode##(s) {
	var div = document.createElement('div');
	div.appendChild(document.createTextNode(s));
	return div.innerHTML.replace(/\"/g, '&quot;');
};

ss.indexOfAnyString = function#? DEBUG ss$indexOfAnyString##(s, chars, startIndex, count) {
	var length = s.length;
	if (!length) {
		return -1;
	}

	chars = String.fromCharCode.apply(null, chars);
	startIndex = startIndex || 0;
	count = count || length;

	var endIndex = startIndex + count - 1;
	if (endIndex >= length) {
		endIndex = length - 1;
	}

	for (var i = startIndex; i <= endIndex; i++) {
		if (chars.indexOf(s.charAt(i)) >= 0) {
			return i;
		}
	}
	return -1;
};

ss.insertString = function#? DEBUG ss$insertString##(s, index, value) {
	if (!value) {
		return s;
	}
	if (!index) {
		return value + s;
	}
	var s1 = s.substr(0, index);
	var s2 = s.substr(index);
	return s1 + value + s2;
};

ss.isNullOrEmptyString = function#? DEBUG ss$isNullOrEmptyString##(s) {
	return !s || !s.length;
};

ss.lastIndexOfAnyString = function#? DEBUG ss$lastIndexOfAnyString##(s, chars, startIndex, count) {
	var length = s.length;
	if (!length) {
		return -1;
	}

	chars = String.fromCharCode.apply(null, chars);
	startIndex = startIndex || length - 1;
	count = count || length;

	var endIndex = startIndex - count + 1;
	if (endIndex < 0) {
		endIndex = 0;
	}

	for (var i = startIndex; i >= endIndex; i--) {
		if (chars.indexOf(s.charAt(i)) >= 0) {
			return i;
		}
	}
	return -1;
};

ss.localeFormatString = function#? DEBUG ss$localeFormatString##(format) {
	return ss._formatString(format, arguments, /* useLocale */ true);
};

ss.padLeftString = function#? DEBUG ss$padLeftString##(s, totalWidth, ch) {
	if (s.length < totalWidth) {
		ch = String.fromCharCode(ch || 0x20);
		return ss.stringFromChar(ch, totalWidth - s.length) + s;
	}
	return s;
};

ss.padRightString = function#? DEBUG ss$padRightString##(s, totalWidth, ch) {
	if (s.length < totalWidth) {
		ch = String.fromCharCode(ch || 0x20);
		return s + ss.stringFromChar(ch, totalWidth - s.length);
	}
	return s;
};

ss.removeString = function#? DEBUG ss$removeString##(s, index, count) {
	if (!count || ((index + count) > this.length)) {
		return s.substr(0, index);
	}
	return s.substr(0, index) + s.substr(index + count);
};

ss.replaceAllString = function#? DEBUG ss$replaceAllString##(s, oldValue, newValue) {
	newValue = newValue || '';
	return s.split(oldValue).join(newValue);
};

ss.startsWithString = function#? DEBUG ss$startsWithString##(s, prefix) {
	if (!prefix.length) {
		return true;
	}
	if (prefix.length > s.length) {
		return false;
	}
	return (s.substr(0, prefix.length) == prefix);
};

if (!String.prototype.trim) {
	String.prototype.trim = function#? DEBUG String$trim##() {
		return ss.trimStartString(ss.trimEndString(this));
	};
}

ss.trimEndString = function#? DEBUG ss$trimEndString##(s) {
	return s.replace(/\s*$/, '');
};

ss.trimStartString = function#? DEBUG ss$trimStartString##(s) {
	return s.replace(/^\s*/, '');
};

ss.trimWithCharsString = function#? DEBUG ss$trimWithCharsString##(s, chars) {
    return ss.trimStartWithCharsString(ss.trimEndWithCharsString(s, chars), chars);
};

ss.trimStartWithCharsString = function#? DEBUG ss$trimStartWithCharsString##(s, chars) {
    chars = String.fromCharCode.apply(null, chars);
    return s.replace(new RegExp('^[' + chars + ']+'), '');
};

ss.trimEndWithCharsString = function#? DEBUG ss$trimEndWithCharsString##(s, chars) {
    chars = String.fromCharCode.apply(null, chars);
    return s.replace(new RegExp('[' + chars + ']+$'), '');
};

ss.fromCharArray = function#? DEBUG ss$fromCharArray##(chars) {
    return String.fromCharCode.apply(null, chars);
};

ss.lastIndexOfWithStartIndexAndCount = function#? DEBUG ss$lastIndexOfWithStartIndexAndCount##(s, search, startIndex, count) {
    var index = s.lastIndexOf(search, startIndex);
    return (index < (startIndex - count + 1)) ? -1 : index;
};

ss.indexOfWithStartIndexAndCount = function#? DEBUG ss$indexOfWithStartIndexAndCount##(s, search, startIndex, count) {
    var index = s.indexOf(search, startIndex);
    return ((index + search.length) <= (startIndex + count)) ? index : -1;
};
