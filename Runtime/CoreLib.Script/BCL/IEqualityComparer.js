///////////////////////////////////////////////////////////////////////////////
// IEnumerator

var ss_IEqualityComparer = function#? DEBUG IEqualityComparer$##() { };
ss_IEqualityComparer.prototype = {
	areEqual: null,
	getObjectHashCode: null
};

Type.registerInterface(global, 'ss.IEqualityComparer', ss_IEqualityComparer);
