///////////////////////////////////////////////////////////////////////////////
// IEnumerable

var ss_ICollection = function#? DEBUG ICollection$##() { };
ss_ICollection.prototype = {
	get_count: null,
	add: null,
	clear: null,
	contains: null,
	remove: null
};

ss_ICollection.isAssignableFrom = function#? DEBUG ICollection$isAssignableFrom##(type) {
	if (type == Array)
		return true;
	else
		return Type.prototype.isAssignableFrom.call(this, type);
};

Type.registerInterface(global, 'ss.ICollection', ss_IEnumerable);
