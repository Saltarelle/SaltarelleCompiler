///////////////////////////////////////////////////////////////////////////////
// IEnumerable

ss.IList = function#? DEBUG IList$##() { };
#if DEBUG
ss.IList.prototype = {
	get_item: null,
	set_item: null,
	indexOf: null,
	insert: null,
	removeAt: null
}
#endif // DEBUG

ss.IList.isAssignableFrom = function#? DEBUG IList$isAssignableFrom##(type) {
	if (type == Array)
		return true;
	else
		return Type.prototype.isAssignableFrom.call(this, type);
};

ss.IList.registerInterface('ss.IList', ss.ICollection, ss.IEnumerable);
