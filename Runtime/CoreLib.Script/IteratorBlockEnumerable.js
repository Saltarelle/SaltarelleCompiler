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

ss_IteratorBlockEnumerable.__typeName = 'ss.IteratorBlockEnumerable';
ss.IteratorBlockEnumerable = ss_IteratorBlockEnumerable;
ss.initClass(ss_IteratorBlockEnumerable, null, [ss_IEnumerable]);
