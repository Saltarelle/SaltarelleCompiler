///////////////////////////////////////////////////////////////////////////////
// Lazy

var ss_Lazy = ss.Lazy = ss.mkType(ss, 'ss.Lazy',
	function#? DEBUG Lazy$##(valueFactory) {
		this._valueFactory = valueFactory;
		this.isValueCreated = false;
	},
	{
		value: function#? DEBUG Lazy$value##() {
			if (!this.isValueCreated) {
				this._value = this._valueFactory();
				delete this._valueFactory;
				this.isValueCreated = true;
			}
			return this._value;
		}
	}
);

ss.initClass(ss_Lazy);
