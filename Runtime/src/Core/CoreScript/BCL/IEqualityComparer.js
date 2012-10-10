///////////////////////////////////////////////////////////////////////////////
// IEnumerator

var ss_IEqualityComparer = function#? DEBUG IEqualityComparer$##() { };
ss_IEqualityComparer.prototype = {
    equals: null,
    getHashCode: null
};

Type.registerInterface(global, 'ss.IEqualityComparer', ss_IEqualityComparer);
