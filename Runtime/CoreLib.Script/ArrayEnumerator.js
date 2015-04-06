///////////////////////////////////////////////////////////////////////////////
// ArrayEnumerator

var ss_ArrayEnumerator = ss.ArrayEnumerator = ss.mkType(ss, 'ss.ArrayEnumerator',
	function#? DEBUG ArrayEnumerator$##(array) {
		this._array = array;
		this._index = -1;
	},
	{
		moveNext: function#? DEBUG ArrayEnumerator$moveNext##() {
			this._index++;
			return (this._index < this._array.length);
		},
		reset: function#? DEBUG ArrayEnumerator$reset##() {
			this._index = -1;
		},
		current: function#? DEBUG ArrayEnumerator$current##() {
			if (this._index < 0 || this._index >= this._array.length)
				throw 'Invalid operation';
			return this._array[this._index];
		},
		dispose: function#? DEBUG ArrayEnumerator$dispose##() {
		}
	}
);

ss.initClass(ss_ArrayEnumerator, null, [ss_IEnumerator, ss_IDisposable]);
