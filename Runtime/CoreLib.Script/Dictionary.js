///////////////////////////////////////////////////////////////////////////////
// Dictionary
var ss_$DictionaryCollection = function#? DEBUG $DictionaryCollection$##(dict, isKeys) {
	this._dict = dict;
	this._isKeys = isKeys;
};

var ss_Dictionary$2 = function#? DEBUG Dictionary$2$##(TKey, TValue) {
	var $type = function(o, cmp) {
		this.countField = 0;
		this.buckets = {};

		this.comparer = cmp || ss_EqualityComparer.def;

		if (ss.isInstanceOfType(o, ss_IDictionary)) {
			var e = ss.getEnumerator(o);
			try {
				while (e.moveNext()) {
					var c = e.current();
					this.add(c.key, c.value);
				}
			}
			finally {
				if (ss.isInstanceOfType(e, ss_IDisposable)) {
					ss.cast(e, ss_IDisposable).dispose();
				}
			}
		}
		else if (o) {
			var keys = Object.keys(o);
			for (var i = 0; i < keys.length; i++) {
				this.add(keys[i], o[keys[i]]);
			}
		}
	};

	ss.registerGenericClassInstance($type, ss_Dictionary$2, [TKey, TValue], {
		_setOrAdd: function(key, value, add) {
			var hash = this.comparer.getObjectHashCode(key);
			var entry = { key: key, value: value };
			if (this.buckets.hasOwnProperty(hash)) {
				var array = this.buckets[hash];
				for (var i = 0; i < array.length; i++) {
					if (this.comparer.areEqual(array[i].key, key)) {
						if (add)
							throw new ss_ArgumentException('Key ' + key + ' already exists.');
						array[i] = entry;
						return;
					}
				}
				array.push(entry);
			} else {
				this.buckets[hash] = [entry];
			}
			this.countField++;
		},

		add: function(key, value) {
			this._setOrAdd(key, value, true);
		},

		set_item: function(key, value) {
			this._setOrAdd(key, value, false);
		},

		_get: function(key) {
			var hash = this.comparer.getObjectHashCode(key);
			if (this.buckets.hasOwnProperty(hash)) {
				var array = this.buckets[hash];
				for (var i = 0; i < array.length; i++) {
					var entry = array[i];
					if (this.comparer.areEqual(entry.key, key))
						return entry.value !== undefined ? entry.value : null;
				}
			}
			return undefined;
		},

		get_item: function(key) {
			var v = this._get(key);
			if (v === undefined)
				throw new ss_KeyNotFoundException('Key ' + key + ' does not exist.');
			return v;
		},

		tryGetValue: function(key, value) {
			var v = this._get(key);
			if (v !== undefined) {
				value.$ = v;
				return true;
			}
			else {
				value.$ = ss.getDefaultValue(TValue);
				return false;
			}
		},

		containsKey: function(key) {
			var hash = this.comparer.getObjectHashCode(key);
			if (!this.buckets.hasOwnProperty(hash))
				return false;

			var array = this.buckets[hash];
			for (var i = 0; i < array.length; i++) {
				if (this.comparer.areEqual(array[i].key, key))
					return true;
			}
			return false;
		},

		clear: function() {
			this.countField = 0;
			this.buckets = {};
		},

		remove: function(key) {
			var hash = this.comparer.getObjectHashCode(key);
			if (!this.buckets.hasOwnProperty(hash))
				return false;

			var array = this.buckets[hash];
			for (var i = 0; i < array.length; i++) {
				if (this.comparer.areEqual(array[i].key, key)) {
					array.splice(i, 1);
					if (array.length == 0) delete this.buckets[hash];
					this.countField--;
					return true;
				}
			}
			return false;
		},

		get_count: function() {
			return this.countField;
		},

		_getEnumerator: function(projector) {
			var bucketKeys = Object.keys(this.buckets), bucketIndex = -1, arrayIndex;
			return new ss_IteratorBlockEnumerator(function() {
				if (bucketIndex < 0 || arrayIndex >= (this.buckets[bucketKeys[bucketIndex]].length - 1)) {
					arrayIndex = -1;
					bucketIndex++;
				}
				if (bucketIndex >= bucketKeys.length)
					return false;
				arrayIndex++;
				return true;
			}, function() { return projector(this.buckets[bucketKeys[bucketIndex]][arrayIndex]); }, null, this);
		},

		get_keys: function() {
			return new ss_$DictionaryCollection(this, true);
		},

		get_values: function() {
			return new ss_$DictionaryCollection(this, false);
		},

		getEnumerator: function() {
			return this._getEnumerator(function(e) { return e; });
		}
	}, function() { return null; }, function() { return [ ss_IDictionary, ss_IReadOnlyDictionary, ss_IEnumerable ]; });
	return $type;
};

ss_Dictionary$2.__typeName = 'ss.Dictionary$2';
ss.Dictionary$2 = ss_Dictionary$2;
ss.initGenericClass(ss_Dictionary$2, ss, 2);
ss_$DictionaryCollection.__typeName = 'ss.$DictionaryCollection';
ss.$DictionaryCollection = ss_$DictionaryCollection;
ss.initClass(ss_$DictionaryCollection, ss, {
	get_count: function#? DEBUG $DictionaryCollection$get_count##() {
		return this._dict.get_count();
	},
	contains: function#? DEBUG $DictionaryCollection$contains##(v) {
		if (this._isKeys) {
			return this._dict.containsKey(v);
		}
		else {
			for (var e in this._dict.buckets) {
				if (this._dict.buckets.hasOwnProperty(e)) {
					var bucket = this._dict.buckets[e];
					for (var i = 0; i < bucket.length; i++) {
						if (this._dict.comparer.areEqual(bucket[i].value, v))
							return true;
					}
				}
			}
			return false;
		}
	},
	getEnumerator: function#? DEBUG $DictionaryCollection$getEnumerator##(v) {
		return this._dict._getEnumerator(this._isKeys ? function(e) { return e.key; } : function(e) { return e.value; });
	},
	add: function#? DEBUG $DictionaryCollection$add##(v) {
		throw new ss_InvalidOperationException('Collection is read-only');
	},
	clear: function#? DEBUG $DictionaryCollection$clear##() {
		throw new ss_InvalidOperationException('Collection is read-only');
	},
	remove: function#? DEBUG $DictionaryCollection$remove##() {
		throw new ss_InvalidOperationException('Collection is read-only');
	}
}, null, [ss_IEnumerable, ss_ICollection]);
