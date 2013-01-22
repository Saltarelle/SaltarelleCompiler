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

ss.registerInterface(global, 'ss.IList', ss_IList, ss_ICollection, ss_IEnumerable);
