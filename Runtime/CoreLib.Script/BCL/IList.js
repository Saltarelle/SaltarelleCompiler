///////////////////////////////////////////////////////////////////////////////
// IList

var ss_IList = function#? DEBUG IList$##() { };
ss_IList.prototype = {
	get_item: null,
	set_item: null,
	indexOf: null,
	insert: null,
	removeAt: null
};

ss_IList.isAssignableFrom = function#? DEBUG IList$isAssignableFrom##(type) {
	if (type == Array)
		return true;
	else
		return Type.prototype.isAssignableFrom.call(this, type);
};

Type.registerInterface(global, 'ss.IList', ss_IList, ss_ICollection, ss_IEnumerable);
