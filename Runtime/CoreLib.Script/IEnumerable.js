///////////////////////////////////////////////////////////////////////////////
// IEnumerable

var ss_IEnumerable = function#? DEBUG IEnumerable$##() { };

ss_IEnumerable.__typeName = 'ss.IEnumerable';
ss.IEnumerable = ss_IEnumerable;
ss.initInterface(ss_IEnumerable, { getEnumerator: null });
ss.getEnumerator = function#? DEBUG ss$getEnumerator##(obj) {
	return obj.getEnumerator ? obj.getEnumerator() : new ss_ArrayEnumerator(obj);
};
