///////////////////////////////////////////////////////////////////////////////
// IteratorBlockEnumerator

var ss_IteratorBlockEnumerator = function#? DEBUG IteratorBlockEnumerator$##(moveNext, getCurrent, dispose, $this) {
	this._moveNext = moveNext;
	this._getCurrent = getCurrent;
	this._dispose = dispose;
	this._this = $this;
};

ss_IteratorBlockEnumerator.prototype = {
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
	current: function#? DEBUG IteratorBlockEnumerator$current##() {
		return this._getCurrent.call(this._this);
	},
	reset: function#? DEBUG IteratorBlockEnumerator$reset##() {
		throw new ss_NotSupportedException('Reset is not supported.');
	},
	dispose: function#? DEBUG IteratorBlockEnumerator$dispose##() {
		if (this._dispose)
			this._dispose.call(this._this);
	}
};

ss.registerClass(global, 'ss.IteratorBlockEnumerator', ss_IteratorBlockEnumerator, null, [ss_IEnumerator, ss_IDisposable]);
