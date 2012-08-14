///////////////////////////////////////////////////////////////////////////////
// IEnumerator

ss.IEqualityComparer = function#? DEBUG IEqualityComparer$##() { };
#if DEBUG
ss.IEqualityComparer.prototype = {
    equals: null,
    getHashCode: null
}
#endif // DEBUG

ss.IEqualityComparer.registerInterface('ss.IEqualityComparer');
