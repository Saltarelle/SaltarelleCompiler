///////////////////////////////////////////////////////////////////////////////
// ICollection

var ss_ICollection = function#? DEBUG ICollection$##() { };
ss_ICollection.prototype = {
	get_count: null,
	add: null,
	clear: null,
	contains: null,
	remove: null
};

ss.registerInterface(global, 'ss.ICollection', ss_IEnumerable);
