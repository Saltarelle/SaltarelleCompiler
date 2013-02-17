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

ss.registerInterface(global, 'ss.ICollection', ss_ICollection);

ss.count = function#? DEBUG ss$count##(obj) {
	return ss.isArray(obj) ? obj.length : obj.get_count();
};

ss.add = function#? DEBUG ss$add##(obj, item) {
	ss.isArray(obj) ? obj.push(item) : obj.add(item);
};

ss.clear = function#? DEBUG ss$clear##(obj, item) {
	ss.isArray(obj) ? (obj.length = 0) : obj.clear();
};

ss.remove = function#? DEBUG ss$remove##(obj, item) {
	if (ss.isArray(obj)) {
		var index = ss.indexOf(obj, item);
		if (index >= 0) {
			obj.splice(index, 1);
			return true;
		}
		return false;
	}
	else
		return obj.remove(item);
};

ss.contains = function#? DEBUG ss$contains##(obj, item) {
	return ss.isArray(obj) ? (ss.indexOf(obj, item) >= 0) : obj.contains(item);
};