///////////////////////////////////////////////////////////////////////////////
// Dictionary
ss.Dictionary$2 = function#? DEBUG Dictionary$2$##(TKey, TValue) {
	var $type = function(o) {
		this._o = {};
		if (ss.IDictionary.isInstanceOfType(o)) {
			var e = Type.cast(o, ss.IDictionary).getEnumerator();
			try {
				while (e.moveNext()) {
					var c = e.get_current();
					this._o[c.key] = c.value;
				}
			}
			finally {
				if (ss.IDisposable.isInstanceOfType(e)) {
					Type.cast(e, ss.IDisposable).dispose();
				}
			}
		}
		else if (o) {
			var keys = Object.keys(o);
			for (var i = 0; i < keys.length; i++) {
				this._o[keys[i]] = o[keys[i]];
			}
		}
	};
	$type.prototype = {
		get_count: function#? DEBUG Dictionary$2$get_count##() {
			return Object.getKeyCount(this._o);
		},
		get_keys: function#? DEBUG Dictionary$2$get_keys##() {
			return Object.keys(this._o);
		},
		get_values: function#? DEBUG Dictionary$2$get_values##() {
			var result = [];
			var keys = Object.keys(this._o);
			for (var i = 0; i < keys.length; i++)
				result.push(this._o[keys[i]]);
			return result;
		},
		get_item: function#? DEBUG Dictionary$2$get_item##(key) {
			if (!Object.keys(this._o, key))
				throw 'Key ' + key + ' does not exist.';
			return this._o[key];
		},
		set_item: function#? DEBUG Dictionary$2$set_item##(key, value) {
			this._o[key] = value;
		},
		add: function#? DEBUG Dictionary$2$add##(key, value) {
			if (Object.keyExists(this._o, key))
				throw 'Key ' + key + ' already exists.';
			this._o[key] = value;
		},
		getEnumerator: function#? DEBUG Dictionary$2$getEnumerator##() {
			return new ss.ObjectEnumerator(this._o);
		},
		remove: function#? DEBUG Dictionary$2$remove##(key, value) {
			delete this._o[key];
		},
		containsKey: function#? DEBUG Dictionary$2$containsKey##(key) {
			return Object.keyExists(this._o, key);
		},
		tryGetValue: function#? DEBUG Dictionary$2$tryGetValue##(key, value) {
			if (Object.keyExists(this._o, key)) {
				value.$ = this._o[key];
				return true;
			}
			else {
				value.$ = TValue.getDefaultValue();
				return false;
			}
		},
		clear: function#? DEBUG Dictionary$2$clear##() {
			Object.clearKeys(this._o);
		}
	};
	$type.registerGenericClassInstance($type, ss.Dictionary$2, [TKey, TValue], function() { return null }, function() { return [ ss.IDictionary, ss.IEnumerable ] });
	return $type;
};
ss.Dictionary$2.registerGenericClass('ss.Dictionary$2', 2);
