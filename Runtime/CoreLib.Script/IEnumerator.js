///////////////////////////////////////////////////////////////////////////////
// IEnumerator

var ss_IEnumerator = function#? DEBUG IEnumerator$##() { };
ss_IEnumerator.prototype = {
	current: null,
	moveNext: null,
	reset: null
};

ss_IEnumerator.__typeName = 'ss.IEnumerator';
ss.registerInterface(global, 'ss.IEnumerator', ss_IEnumerator, [ss_IDisposable]);
