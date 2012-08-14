///////////////////////////////////////////////////////////////////////////////
// ArrayEnumerator

ss.ArrayEnumerator = function#? DEBUG ArrayEnumerator$##(array) {
    this._array = array;
    this._index = -1;
}
ss.ArrayEnumerator.prototype = {
    moveNext: function#? DEBUG ArrayEnumerator$moveNext##() {
        this._index++;
        return (this._index < this._array.length);
    },
    reset: function#? DEBUG ArrayEnumerator$reset##() {
        this._index = -1;
    },
	get_current: function#? DEBUG ArrayEnumerator$get_current##() {
		if (this._index < 0 || this._index >= this._array.length)
			throw 'Invalid operation';
		return this._array[this._index];
	},
    dispose: function#? DEBUG ArrayEnumerator$dispose##() {
    }
}

ss.ArrayEnumerator.registerClass('ss.ArrayEnumerator', null, [ss.IEnumerator, ss.IDisposable]);
