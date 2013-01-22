///////////////////////////////////////////////////////////////////////////////
// EqualityComparer

var ss_EqualityComparer = function#? DEBUG EqualityComparer$##() {
};
ss_EqualityComparer.prototype.areEqual = function#? DEBUG EqualityComparer$areEqual##(x, y) {
	return ss.staticEquals(x, y);
};
ss_EqualityComparer.prototype.getObjectHashCode = function#? DEBUG EqualityComparer$getObjectHashCode##(obj) {
	return ss.isValue(obj) ? ss.getHashCode(obj) : 0;
};
ss.registerClass(global, 'ss.EqualityComparer', ss_EqualityComparer, null, ss_IEqualityComparer);
ss_EqualityComparer.def = new ss_EqualityComparer();
