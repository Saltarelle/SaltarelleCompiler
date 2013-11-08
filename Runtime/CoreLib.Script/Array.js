///////////////////////////////////////////////////////////////////////////////
// Array Extensions

ss._flatIndex = function#? DEBUG ss$_flatIndex##(arr, indices) {
	if (indices.length != (arr._sizes ? arr._sizes.length : 1))
		throw new ss_ArgumentException('Invalid number of indices');

	if (indices[0] < 0 || indices[0] >= (arr._sizes ? arr._sizes[0] : arr.length))
		throw new ss_ArgumentException('Index 0 out of range');

	var idx = indices[0];
	if (arr._sizes) {
		for (var i = 1; i < arr._sizes.length; i++) {
			if (indices[i] < 0 || indices[i] >= arr._sizes[i])
				throw new ss_ArgumentException('Index ' + i + ' out of range');
			idx = idx * arr._sizes[i] + indices[i];
		}
	}
	return idx;
};

ss.arrayGet2 = function#? DEBUG ss$arrayGet2##(arr, indices) {
	var idx = ss._flatIndex(arr, indices);
	var r = arr[idx];
	return typeof r !== 'undefined' ? r : arr._defvalue;
};

ss.arrayGet = function#? DEBUG ss$arrayGet##(arr) {
	return ss.arrayGet2(arr, Array.prototype.slice.call(arguments, 1));
}

ss.arraySet2 = function#? DEBUG ss$arraySet2##(arr, value, indices) {
	var idx = ss._flatIndex(arr, indices);
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
		throw new ss_ArgumentException('Invalid dimension');
	return arr._sizes ? arr._sizes[dimension] : arr.length;
};

ss.arrayExtract = function#? DEBUG ss$arrayExtract##(arr, start, count) {
	if (!ss.isValue(count)) {
		return arr.slice(start);
	}
	return arr.slice(start, start + count);
};

ss.arrayAddRange = function#? DEBUG ss$arrayAddRange##(arr, items) {
	if (items instanceof Array) {
		arr.push.apply(arr, items);
	}
	else {
		var e = ss.getEnumerator(items);
		try {
			while (e.moveNext()) {
				ss.add(arr, e.current());
			}
		}
		finally {
			if (ss.isInstanceOfType(e, ss_IDisposable)) {
				ss.cast(e, ss_IDisposable).dispose();
			}
		}
	}
};

ss.arrayClone = function#? DEBUG ss$arrayClone##(arr) {
	if (arr.length === 1) {
		return [arr[0]];
	}
	else {
		return Array.apply(null, arr);
	}
};

ss.arrayPeekFront = function#? DEBUG ss$arrayPeekFront##(arr) {
	if (arr.length)
		return arr[0];
	throw new ss_InvalidOperationException('Array is empty');
};

ss.arrayPeekBack = function#? DEBUG ss$arrayPeekBack##(arr) {
	if (arr.length)
		return arr[arr.length - 1];
	throw new ss_InvalidOperationException('Array is empty');
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

ss.indexOfArray = function#? DEBUG ss$indexOfArray##(arr, item, startIndex) {
	startIndex = startIndex || 0;
	for (var i = startIndex; i < arr.length; i++) {
		if (ss.staticEquals(arr[i], item)) {
			return i;
		}
	}
	return -1;
}

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
		var e = ss.getEnumerator(items);
		try {
			while (e.moveNext()) {
				arr.insert(index, e.current());
				index++;
			}
		}
		finally {
			if (ss.isInstanceOfType(e, ss_IDisposable)) {
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
	if (!ss.isValue(enm))
		return null;

	var e = ss.getEnumerator(enm), r = [];
	try {
		while (e.moveNext())
			r.push(e.current());
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

ss.repeat = function#? DEBUG ss$repeat##(value, count) {
	var result = [];
	for (var i = 0; i < count; i++)
		result.push(value);
	return result;
};