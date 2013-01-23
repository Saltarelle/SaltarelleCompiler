///////////////////////////////////////////////////////////////////////////////
// IEnumerable

var ss_IEnumerable = function#? DEBUG IEnumerable$##() { };
ss_IEnumerable.prototype = {
	getEnumerator: null
};

ss.registerInterface(global, 'ss.IEnumerable', ss_IEnumerable);

ss.getEnumerator = function#? DEBUG ss$getEnumerator##(obj) {
	return ss.isArray(obj) ? new ss_ArrayEnumerator(obj) : obj.getEnumerator();
};
