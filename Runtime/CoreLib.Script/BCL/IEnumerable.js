///////////////////////////////////////////////////////////////////////////////
// IEnumerable

var ss_IEnumerable = function#? DEBUG IEnumerable$##() { };
ss_IEnumerable.prototype = {
    getEnumerator: null
};

ss_IEnumerable.isAssignableFrom = function#? DEBUG IEnumerable$isAssignableFrom##(type) {
	if (type == Array)
		return true;
	else
		return Type.prototype.isAssignableFrom.call(this, type);
};

Type.registerInterface(global, 'ss.IEnumerable', ss_IEnumerable);
