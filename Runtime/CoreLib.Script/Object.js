///////////////////////////////////////////////////////////////////////////////
// Object Extensions

ss.clearKeys = function#? DEBUG ss$clearKeys##(d) {
	for (var n in d) {
		if (d.hasOwnProperty(n))
			delete d[n];
	}
};

ss.keyExists = function#? DEBUG ss$keyExists##(d, key) {
	return d[key] !== undefined;
};

if (!Object.keys) {
	Object.keys = function#? DEBUG Object$keys##(d) {
		var keys = [];
		for (var n in d) {
			if (d.hasOwnProperty(n))
				keys.push(n);
		}
		return keys;
	};

	ss.getKeyCount = function#? DEBUG ss$getKeyCount##(d) {
		var count = 0;
		for (var n in d) {
			if (d.hasOwnProperty(n))
				count++;
		}
		return count;
	};
}
else {
	ss.getKeyCount = function#? DEBUG ss$getKeyCount2##(d) {
		return Object.keys(d).length;
	};
}
