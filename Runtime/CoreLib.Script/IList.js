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

ss.registerInterface(global, 'ss.IList', ss_IList, [ss_ICollection, ss_IEnumerable]);

ss.getItem = function#? DEBUG ss$getItem##(obj, index) {
	return obj.get_item ? obj.get_item(index) : obj[index];
}

ss.setItem = function#? DEBUG ss$setItem##(obj, index, value) {
	obj.set_item ? obj.set_item(index, value) : (obj[index] = value);
}

ss.indexOf = function#? DEBUG ss$indexOf##(obj, item) {
	if (ss.isArrayOrTypedArray(obj)) {
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
	if (obj.insert)
		obj.insert(index, item);
	else if (ss.isArray(obj))
		obj.splice(index, 0, item);
	else
		throw new ss_NotSupportedException();
};

ss.removeAt = function#? DEBUG ss$removeAt##(obj, index) {
	if (obj.removeAt)
		obj.removeAt(index);
	else if (ss.isArray(obj))
		obj.splice(index, 1);
	else
		throw new ss_NotSupportedException();
};
