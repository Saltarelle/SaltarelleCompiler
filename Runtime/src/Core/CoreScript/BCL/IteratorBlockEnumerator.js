///////////////////////////////////////////////////////////////////////////////
// IteratorBlockEnumerator

ss.IteratorBlockEnumerator = function#? DEBUG IteratorBlockEnumerator$##(moveNext, getCurrent, dispose, $this) {
    this._moveNext = moveNext;
    this._getCurrent = getCurrent;
    this._dispose = dispose;
    this._this = $this;
};

ss.IteratorBlockEnumerator.prototype = {
	moveNext: function#? DEBUG IteratorBlockEnumerator$moveNext##() {
        try {
		    return this._moveNext.call(this._this);
        }
        catch (ex) {
            if (this._dispose)
                this._dispose.call(this._this);
            throw ex;
        }
	},
	get_current: function#? DEBUG IteratorBlockEnumerator$get_current##() {
		return this._getCurrent.call(this._this);
	},
	reset: function#? DEBUG IteratorBlockEnumerator$reset##() {
		throw new ss.NotSupportedException('Reset is not supported.');
	},
	dispose: function#? DEBUG IteratorBlockEnumerator$dispose##() {
		if (this._dispose)
            this._dispose.call(this._this);
	}
};

ss.IteratorBlockEnumerator.registerClass('ss.IteratorBlockEnumerator', null, [ss.IEnumerator, ss.IDisposable]);
