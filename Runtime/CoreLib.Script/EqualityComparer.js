///////////////////////////////////////////////////////////////////////////////
// EqualityComparer

var ss_EqualityComparer = function#? DEBUG EqualityComparer$##() {
};
ss_EqualityComparer.__typeName = 'ss.EqualityComparer';
ss.EqualityComparer = ss_EqualityComparer;
ss.initClass(ss_EqualityComparer, ss, {
	areEqual: function#? DEBUG EqualityComparer$areEqual##(x, y) {
		return ss.staticEquals(x, y);
	},
	getObjectHashCode: function#? DEBUG EqualityComparer$getObjectHashCode##(obj) {
		return ss.isValue(obj) ? ss.getHashCode(obj) : 0;
	}
}, null, [ss_IEqualityComparer]);
ss_EqualityComparer.def = new ss_EqualityComparer();
