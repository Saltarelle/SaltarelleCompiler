///////////////////////////////////////////////////////////////////////////////
// IEnumerable

var ss_IEnumerable = function#? DEBUG IEnumerable$##() { };
ss_IEnumerable.prototype = {
	getEnumerator: null
};

ss.registerInterface(global, 'ss.IEnumerable', ss_IEnumerable);
