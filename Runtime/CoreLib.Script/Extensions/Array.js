///////////////////////////////////////////////////////////////////////////////
// Array Extensions

//TODO
Array.prototype.get_item = function#? DEBUG Array$get_item##(index) {
	return this[index];
};

//TODO
Array.prototype.set_item = function#? DEBUG Array$set_item##(index, value) {
	this[index] = value;
};

//TODO
Array.prototype.get_count = function#? DEBUG Array$get_count##() {
	return this.length;
};

ss.arrayGet2 = function#? DEBUG ss$arrayGet2##(arr, indices) {
	if (indices.length != (arr._sizes ? arr._sizes.length : 1))
		throw 'Invalid number of indices';

	if (indices[0] < 0 || indices[0] >= (arr._sizes ? arr._sizes[0] : arr.length))
		throw 'Index 0 out of range';

	var idx = indices[0];
	if (arr._sizes) {
		for (var i = 1; i < arr._sizes.length; i++) {
			if (indices[i] < 0 || indices[i] >= arr._sizes[i])
				throw 'Index ' + i + ' out of range';
			idx = idx * arr._sizes[i] + indices[i];
		}
	}
	var r = arr[idx];
	return typeof r !== 'undefined' ? r : arr._defvalue;
};

ss.arrayGet = function#? DEBUG ss$arrayGet##(arr) {
	return ss.arrayGet2(arr, Array.prototype.slice.call(arguments, 1));
}

ss.arraySet2 = function#? DEBUG ss$arraySet2##(arr, value, indices) {
	if (indices.length != (arr._sizes ? arr._sizes.length : 1))
		throw 'Invalid number of indices';

	if (indices[0] < 0 || indices[0] >= (arr._sizes ? arr._sizes[0] : arr.length))
		throw 'Index 0 out of range';

	var idx = indices[0];
	if (arr._sizes) {
		for (var i = 1; i < arr._sizes.length; i++) {
			if (indices[i] < 0 || indices[i] >= arr._sizes[i])
				throw 'Index ' + i + ' out of range';
			idx = idx * arr._sizes[i] + indices[i];
		}
	}
	arr[idx] = value;
};

ss.arraySet = function#? DEBUG ss$arraySet##() {
	return ss.arraySet2(arguments[0], arguments[arguments.length - 1], Array.prototype.slice.call(arguments, 1, arguments.length - 1));
};

ss.arrayRank = function#? DEBUG ss$arrayRank##(arr) {
	return arr._sizes ? arr._sizes.length : 1;
};

ss.arrayLength = function#? DEBUG ss$arrayLength##(arr, dimension) {
	if (dimension >= (arr._sizes ? arr._sizes.length : 1))
		throw 'Invalid dimension';
	return arr._sizes ? arr._sizes[dimension] : arr.length;
};

ss.arrayExtract = function#? DEBUG ss$arrayExtract##(arr, start, count) {
	if (!ss.isValue(count)) {
		return arr.slice(start);
	}
	return arr.slice(start, start + count);
};

//TODO
Array.prototype.add = function#? DEBUG Array$add##(item) {
	this[this.length] = item;
};

ss.arrayAddRange = function#? DEBUG ss$arrayAddRange##(arr, items) {
	if (items instanceof Array) {
		arr.push.apply(arr, items);
	}
	else {
		var e = items.getEnumerator();
		try {
			while (e.moveNext()) {
				arr.add(e.get_current());
			}
		}
		finally {
			if (ss_IDisposable.isInstanceOfType(e)) {
				ss.cast(e, ss_IDisposable).dispose();
			}
		}
	}
};

//TODO
Array.prototype.clear = function#? DEBUG Array$clear##() {
	this.length = 0;
};

ss.arrayClone = function#? DEBUG ss$arrayClone##(arr) {
	if (arr.length === 1) {
		return [arr[0]];
	}
	else {
		return Array.apply(null, arr);
	}
};

//TODO
Array.prototype.contains = function#? DEBUG Array$contains##(item) {
	var index = this.indexOf(item);
	return (index >= 0);
};

ss.arrayPeekFront = function#? DEBUG ss$arrayPeekFront##(arr) {
	if (arr.length)
		return arr[0];
	throw 'Array is empty';
};

ss.arrayPeekBack = function#? DEBUG ss$arrayPeekBack##(arr) {
	if (arr.length)
		return arr[arr.length - 1];
	throw 'Array is empty';
};

if (!Array.prototype.every) {
	Array.prototype.every = function#? DEBUG Array$every##(callback, instance) {
		var length = this.length;
		for (var i = 0; i < length; i++) {
			if (i in this && !callback.call(instance, this[i], i, this)) {
				return false;
			}
		}
		return true;
	};
}

if (!Array.prototype.filter) {
	Array.prototype.filter = function#? DEBUG Array$filter##(callback, instance) {
		var length = this.length;    
		var filtered = [];
		for (var i = 0; i < length; i++) {
			if (i in this) {
				var val = this[i];
				if (callback.call(instance, val, i, this)) {
					filtered.push(val);
				}
			}
		}
		return filtered;
	};
}

if (!Array.prototype.forEach) {
	Array.prototype.forEach = function#? DEBUG Array$forEach##(callback, instance) {
		var length = this.length;
		for (var i = 0; i < length; i++) {
			if (i in this) {
				callback.call(instance, this[i], i, this);
			}
		}
	};
}

//TODO
Array.prototype.getEnumerator = function#? DEBUG Array$getEnumerator##() {
	return new ss_ArrayEnumerator(this);
};

if (!Array.prototype.indexOf) {
	Array.prototype.indexOf = function#? DEBUG Array$indexOf##(item, startIndex) {
		startIndex = startIndex || 0;
		var length = this.length;
		if (length) {
			for (var index = startIndex; index < length; index++) {
				if (this[index] === item) {
					return index;
				}
			}
		}
		return -1;
	};
}

//TODO
Array.prototype.insert = function#? DEBUG Array$insert##(index, item) {
	this.splice(index, 0, item);
};

ss.arrayInsertRange = function#? DEBUG ss$arrayInsertRange##(arr, index, items) {
	if (items instanceof Array) {
		if (index === 0) {
			arr.unshift.apply(arr, items);
		}
		else {
			for (var i = 0; i < items.length; i++) {
				arr.splice(index + i, 0, items[i]);
			}
		}
	}
	else {
		var e = items.getEnumerator();
		try {
			while (e.moveNext()) {
				arr.insert(index, e.get_current());
				index++;
			}
		}
		finally {
			if (ss_IDisposable.isInstanceOfType(e)) {
				ss.cast(e, ss_IDisposable).dispose();
			}
		}
	}
};

if (!Array.prototype.map) {
	Array.prototype.map = function#? DEBUG Array$map##(callback, instance) {
		var length = this.length;
		var mapped = new Array(length);
		for (var i = 0; i < length; i++) {
			if (i in this) {
				mapped[i] = callback.call(instance, this[i], i, this);
			}
		}
		return mapped;
	};
}

//TODO
Array.prototype.remove = function#? DEBUG Array$remove##(item) {
	var index = this.indexOf(item);
	if (index >= 0) {
		this.splice(index, 1);
		return true;
	}
	return false;
};

//TODO
Array.prototype.removeAt = function#? DEBUG Array$removeAt##(index) {
	this.splice(index, 1);
};

ss.arrayRemoveRange = function#? DEBUG ss$arrayRemoveRange##(arr, index, count) {
	arr.splice(index, count);
};

if (!Array.prototype.some) {
	Array.prototype.some = function#? DEBUG Array$some##(callback, instance) {
		var length = this.length;
		for (var i = 0; i < length; i++) {
			if (i in this && callback.call(instance, this[i], i, this)) {
				return true;
			}
		}
		return false;
	};
}

ss.arrayFromEnumerable = function#? DEBUG ss$arrayFromEnumerable##(enm) {
	var e = enm.getEnumerator(), r = [];
	try {
		while (e.moveNext())
			r.push(e.get_current());
	}
	finally {
		e.dispose();
	}
	return r;
};

ss.multidimArray = function#? DEBUG ss$multidimArray##(defvalue, sizes) {
	var arr = [];
	arr._defvalue = defvalue;
	arr._sizes = [arguments[1]];
	var length = arguments[1];
	for (var i = 2; i < arguments.length; i++) {
		length *= arguments[i];
		arr._sizes[i - 1] = arguments[i];
	}
	arr.length = length;
	return arr;
};