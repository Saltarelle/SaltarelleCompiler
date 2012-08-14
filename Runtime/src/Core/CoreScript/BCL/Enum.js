///////////////////////////////////////////////////////////////////////////////
// Enum

ss.Enum = function#? DEBUG Enum$##() {
};
ss.Enum.registerClass('ss.Enum');

ss.Enum.parse = function #? DEBUG Enum$parse##(enumType, s) {
	var values = enumType.prototype;
	if (!enumType.__flags) {
		for (var f in values) {
			if (f === s) {
				return values[f];
			}
		}
	}
	else {
		var parts = s.split('|');
		var value = 0;
		var parsed = true;

		for (var i = parts.length - 1; i >= 0; i--) {
			var part = parts[i].trim();
			var found = false;

			for (var f in values) {
				if (f === part) {
					value |= values[f];
					found = true;
					break;
				}
			}
			if (!found) {
				parsed = false;
				break;
			}
		}

		if (parsed) {
			return value;
		}
	}
	throw 'Invalid Enumeration Value';
};

ss.Enum.toString = function #? DEBUG Enum$toString##(enumType, value) {
	var values = enumType.prototype;
	if (!enumType.__flags || (value === 0)) {
		for (var i in values) {
			if (values[i] === value) {
				return i;
			}
		}
		throw 'Invalid Enumeration Value';
	}
	else {
		var parts = [];
		for (var i in values) {
			if (values[i] & value) {
				parts.add(i);
			}
		}
		if (!parts.length) {
			throw 'Invalid Enumeration Value';
		}
		return parts.join(' | ');
	}
};
