///////////////////////////////////////////////////////////////////////////////
// IDictionary

ss.IDictionary = function#? DEBUG IDictionary$##() { };
#if DEBUG
ss.IDictionary.prototype = {
	get_item: null,
	set_item: null,
	get_keys: null,
	get_values: null,
	containsKey: null,
	add: null,
	remove: null,
	tryGetValue: null
}
#endif // DEBUG

ss.IDictionary.registerInterface('ss.IDictionary', [ss.IEnumerable]);
