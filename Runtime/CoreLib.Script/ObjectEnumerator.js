///////////////////////////////////////////////////////////////////////////////
// ObjectEnumerator

var ss_ObjectEnumerator = function#? DEBUG ObjectEnumerator$##(o) {
	this._keys = Object.keys(o);
	this._index = -1;
	this._object = o;
};

ss_ObjectEnumerator.__typeName = 'ss.ObjectEnumerator';
ss.ObjectEnumerator = ss_ObjectEnumerator;
ss.initClass(ss_ObjectEnumerator, ss, {
	moveNext: function#? DEBUG ObjectEnumerator$moveNext##() {
		this._index++;
		return (this._index < this._keys.length);
	},
	reset: function#? DEBUG ObjectEnumerator$reset##() {
		this._index = -1;
	},
	current: function#? DEBUG ObjectEnumerator$current##() {
		if (this._index < 0 || this._index >= this._keys.length)
			throw new ss_InvalidOperationException('Invalid operation');
		var k = this._keys[this._index];
		return { key: k, value: this._object[k] };
	},
	dispose: function#? DEBUG ObjectEnumerator$dispose##() {
	}
}, null, [ss_IEnumerator, ss_IDisposable]);
