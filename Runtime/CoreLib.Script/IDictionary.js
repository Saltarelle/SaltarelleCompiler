///////////////////////////////////////////////////////////////////////////////
// IDictionary

var ss_IDictionary = function#? DEBUG IDictionary$##() { };
ss_IDictionary.prototype = {
	get_item: null,
	set_item: null,
	get_keys: null,
	get_values: null,
	containsKey: null,
	add: null,
	remove: null,
	tryGetValue: null
};

ss_IDictionary.__typeName = 'ss.IDictionary';
ss.IDictionary = ss_IDictionary;
ss.initInterface(ss_IDictionary, [ss_IEnumerable]);
