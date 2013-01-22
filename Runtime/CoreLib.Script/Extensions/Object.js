///////////////////////////////////////////////////////////////////////////////
// Object Extensions

Object.clearKeys = function#? DEBUG Object$clearKeys##(d) {
	for (var n in d) {
		if (d.hasOwnProperty(n))
			delete d[n];
	}
};

Object.keyExists = function#? DEBUG Object$keyExists##(d, key) {
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

	Object.getKeyCount = function#? DEBUG Object$getKeyCount##(d) {
		var count = 0;
		for (var n in d) {
			if (d.hasOwnProperty(n))
				count++;
		}
		return count;
	};
}
else {
	Object.getKeyCount = function#? DEBUG Object$getKeyCount##(d) {
		return Object.keys(d).length;
	};
}

Object.getObjectEnumerator = function#? DEBUG Object$getObjectEnumerator##(d) {
	return new ss_ObjectEnumerator(d);
};
