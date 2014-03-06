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

ss.getKeyCount = function#? DEBUG ss$getKeyCount##(d) {
	return Object.keys(d).length;
};
