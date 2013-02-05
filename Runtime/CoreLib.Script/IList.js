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

ss.getItem = function#? DEBUG ss$getItem##(obj, index) {
	return ss.isArray(obj) ? obj[index] : obj.get_item(index);
}

ss.setItem = function#? DEBUG ss$setItem##(obj, index, value) {
	ss.isArray(obj) ? (obj[index] = value) : obj.set_item(index, value);
}

ss.indexOf = function#? DEBUG ss$indexOf##(obj, item) {
	if (ss.isArray(obj)) {
		for (var i = 0; i < obj.length; i++) {
			if (ss.staticEquals(obj[i], item)) {
				return i;
			}
		}
		return -1;
	}
	else
		return obj.indexOf(item);
};

ss.insert = function#? DEBUG ss$insert##(obj, index, item) {
	ss.isArray(obj) ? obj.splice(index, 0, item) : obj.insert(index, item);
};

ss.removeAt = function#? DEBUG ss$removeAt##(obj, index) {
	ss.isArray(obj) ? obj.splice(index, 1) : obj.removeAt(index);
};
