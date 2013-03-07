///////////////////////////////////////////////////////////////////////////////
// IDictionary

var ss_IReadOnlyDictionary = function#? DEBUG IReadOnlyDictionary$##() { };
ss_IReadOnlyDictionary.prototype = {
	get_item: null,
	get_keys: null,
	get_values: null,
	containsKey: null,
	tryGetValue: null
};


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

ss.registerInterface(global, 'ss.IReadOnlyDictionary', ss_IReadOnlyDictionary, [ss_IEnumerable]);
ss.registerInterface(global, 'ss.IDictionary', ss_IDictionary, [ss_IEnumerable, ss_IReadOnlyDictionary]);
