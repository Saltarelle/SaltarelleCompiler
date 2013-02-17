///////////////////////////////////////////////////////////////////////////////
// IteratorBlockEnumerable

var ss_IteratorBlockEnumerable = function#? DEBUG IteratorBlockEnumerable$##(getEnumerator, $this) {
	this._getEnumerator = getEnumerator;
	this._this = $this;
};

ss_IteratorBlockEnumerable.prototype = {
	getEnumerator: function#? DEBUG IteratorBlockEnumerable$getEnumerator##() {
		return this._getEnumerator.call(this._this);
	}
};

ss.registerClass(global, 'ss.IteratorBlockEnumerable', ss_IteratorBlockEnumerable, null, [ss_IEnumerable]);
