///////////////////////////////////////////////////////////////////////////////
// IEnumerable

ss.ICollection = function#? DEBUG ICollection$##() { };
#if DEBUG
ss.ICollection.prototype = {
	get_count: null,
	add: null,
	clear: null,
	contains: null,
	remove: null
}
#endif // DEBUG

ss.ICollection.isAssignableFrom = function#? DEBUG ICollection$isAssignableFrom##(type) {
	if (type == Array)
		return true;
	else
		return Type.prototype.isAssignableFrom.call(this, type);
};

ss.ICollection.registerInterface('ss.ICollection', ss.IEnumerable);
