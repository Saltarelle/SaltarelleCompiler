///////////////////////////////////////////////////////////////////////////////
// Int32

var ss_Int32 = function#? DEBUG Int32$##() { };

ss_Int32.__typeName = 'ss.Int32';
ss.Int32 = ss_Int32;
ss.initClass(ss_Int32, Object, [ ss_IEquatable, ss_IComparable, ss_IFormattable ]);
ss_Int32.__class = false;

ss_Int32.isInstanceOfType = function#? DEBUG Int32$isInstanceOfType##(instance) {
	return typeof(instance) === 'number' && isFinite(instance) && Math.round(instance, 0) == instance;
};

ss_Int32.getDefaultValue = ss_Int32.createInstance = function#? DEBUG Int32$getDefaultValue##() {
	return 0;
};

ss_Int32.div = function#? DEBUG Int32$div##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? (a / b) | 0 : null;
};

ss_Int32.trunc = function#? DEBUG Int32$trunc##(n) {
	return ss.isValue(n) ? n | 0 : null;
};

ss_Int32.tryParse = function#? DEBUG Int32$tryParse##(s, result, min, max) {
	result.$ = 0;
	if (!/^[+-]?[0-9]+$/.test(s))
		return 0;
	var n = parseInt(s);
	if (n < min || n > max)
		return false;
	result.$ = n;
	return true;
};
