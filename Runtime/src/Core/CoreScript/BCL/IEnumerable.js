///////////////////////////////////////////////////////////////////////////////
// IEnumerable

ss.IEnumerable = function#? DEBUG IEnumerable$##() { };
#if DEBUG
ss.IEnumerable.prototype = {
    getEnumerator: null
}
#endif // DEBUG

ss.IEnumerable.isAssignableFrom = function#? DEBUG IEnumerable$isAssignableFrom##(type) {
	if (type == Array)
		return true;
	else
		return Type.prototype.isAssignableFrom.call(this, type);
};

ss.IEnumerable.registerInterface('ss.IEnumerable');
