///////////////////////////////////////////////////////////////////////////////
// IEnumerator

var ss_IEnumerator = function#? DEBUG IEnumerator$##() { };
ss_IEnumerator.prototype = {
	get_current: null,
	moveNext: null,
	reset: null
};

Type.registerInterface(global, 'ss.IEnumerator', ss_IEnumerator, ss_IDisposable);
