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
	Object.keys = (function() {
		'use strict';
		var hasOwnProperty = Object.prototype.hasOwnProperty,
			hasDontEnumBug = !({toString: null}).propertyIsEnumerable('toString'),
			dontEnums = ['toString','toLocaleString','valueOf','hasOwnProperty','isPrototypeOf','propertyIsEnumerable','constructor'],
			dontEnumsLength = dontEnums.length;

		return function (obj) {
			if (typeof obj !== 'object' && (typeof obj !== 'function' || obj === null)) {
				throw new TypeError('Object.keys called on non-object');
			}

			var result = [], prop, i;

			for (prop in obj) {
				if (hasOwnProperty.call(obj, prop)) {
					result.push(prop);
				}
			}

			if (hasDontEnumBug) {
				for (i = 0; i < dontEnumsLength; i++) {
					if (hasOwnProperty.call(obj, dontEnums[i])) {
						result.push(dontEnums[i]);
					}
				}
			}
			return result;
		};
	}());
}

ss.getKeyCount = function#? DEBUG ss$getKeyCount##(d) {
	return Object.keys(d).length;
};
