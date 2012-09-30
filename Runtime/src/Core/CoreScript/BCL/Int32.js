///////////////////////////////////////////////////////////////////////////////
// Int32

ss.Int32 = function#? DEBUG Int32$##() { };

ss.Int32.registerClass('ss.Int32');
ss.Int32.__class = false;

ss.Int32.isInstanceOfType = function#? DEBUG Int32$isInstanceOfType##(instance) {
	return typeof(instance) === 'number' && isFinite(instance) && Math.round(instance, 0) == instance;
}

ss.Int32.getDefaultValue = ss.Int32.createInstance = function#? DEBUG Int32$getDefaultValue##() {
	return 0;
}

ss.Int32.div = function#? DEBUG Int32$div##(a, b) {
	return ss.isValue(a) && ss.isValue(b) ? (a / b) | 0 : null;
}

ss.Int32.trunc = function#? DEBUG Int32$trunc##(n) {
	return ss.isValue(n) ? n | 0 : null;
}
