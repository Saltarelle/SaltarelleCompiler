///////////////////////////////////////////////////////////////////////////////
// IteratorBlockEnumerable

ss.IteratorBlockEnumerable = function#? DEBUG IteratorBlockEnumerable$##(getEnumerator, $this) {
    this._getEnumerator = getEnumerator;
    this._this = $this;
};

ss.IteratorBlockEnumerable.prototype = {
    getEnumerator: function#? DEBUG IteratorBlockEnumerable$getEnumerator##() {
        return this._getEnumerator.call(this._this);
    }
};

ss.IteratorBlockEnumerable.registerClass('ss.IteratorBlockEnumerable', null, ss.IEnumerable);
