///////////////////////////////////////////////////////////////////////////////
// Lazy

var ss_Lazy = function#? DEBUG Lazy$##(valueFactory) {
	this._valueFactory = valueFactory;
	this.isValueCreated = false;
};
ss_Lazy.__typeName = 'ss.Lazy';
ss.Lazy = ss_Lazy;
ss.initClass(ss_Lazy, {
	value: function#? DEBUG Lazy$value##() {
		if (!this.isValueCreated) {
			this._value = this._valueFactory();
			delete this._valueFactory;
			this.isValueCreated = true;
		}
		return this._value;
	}
});
