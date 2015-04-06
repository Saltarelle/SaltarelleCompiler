///////////////////////////////////////////////////////////////////////////////
// IteratorBlockEnumerable

var ss_IteratorBlockEnumerable = ss.IteratorBlockEnumerable = ss.mkType(ss, 'ss.IteratorBlockEnumerable',
	function#? DEBUG IteratorBlockEnumerable$##(getEnumerator, $this) {
		this._getEnumerator = getEnumerator;
		this._this = $this;
	},
	{
		getEnumerator: function#? DEBUG IteratorBlockEnumerable$getEnumerator##() {
			return this._getEnumerator.call(this._this);
		}
	}
);

ss.initClass(ss_IteratorBlockEnumerable, null, [ss_IEnumerable]);
