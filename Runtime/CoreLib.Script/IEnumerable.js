///////////////////////////////////////////////////////////////////////////////
// IEnumerable

var ss_IEnumerable = ss.IEnumerable = ss.mkType(ss, 'ss.IEnumerable');

ss.initInterface(ss_IEnumerable);

ss.getEnumerator = function#? DEBUG ss$getEnumerator##(obj) {
	return obj.getEnumerator ? obj.getEnumerator() : new ss_ArrayEnumerator(obj);
};
