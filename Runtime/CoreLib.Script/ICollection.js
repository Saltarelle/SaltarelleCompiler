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
	return obj.get_count ? obj.get_count() : obj.length;
};

ss.add = function#? DEBUG ss$add##(obj, item) {
	if (obj.add)
		obj.add(item);
	else if (ss.isArray(obj))
		obj.push(item);
	else
		throw new ss_NotSupportedException();
};

ss.clear = function#? DEBUG ss$clear##(obj) {
	if (obj.clear)
		obj.clear();
	else if (ss.isArray(obj))
		obj.length = 0;
	else
		throw new ss_NotSupportedException();
};

ss.remove = function#? DEBUG ss$remove##(obj, item) {
	if (obj.remove)
		return obj.remove(item);
	else if (ss.isArray(obj)) {
		var index = ss.indexOf(obj, item);
		if (index >= 0) {
			obj.splice(index, 1);
			return true;
		}
		return false;
	}
	else
		throw new ss_NotSupportedException();
};

ss.contains = function#? DEBUG ss$contains##(obj, item) {
	if (obj.contains)
		return obj.contains(item);
	else
		return ss.indexOf(obj, item) >= 0;
};