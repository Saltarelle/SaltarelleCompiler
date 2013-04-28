///////////////////////////////////////////////////////////////////////////////
// IEqualityComparer

var ss_IEqualityComparer = function#? DEBUG IEqualityComparer$##() { };
ss_IEqualityComparer.prototype = {
	areEqual: null,
	getObjectHashCode: null
};

ss_IEqualityComparer.__typeName = 'ss.IEqualityComparer';
ss.registerInterface(global, 'ss.IEqualityComparer', ss_IEqualityComparer);
