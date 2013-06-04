///////////////////////////////////////////////////////////////////////////////
// StringBuilder

var ss_StringBuilder = function#? DEBUG StringBuilder$##(s) {
	this._parts = ss.isNullOrUndefined(s) || s === '' ? [] : [s];
	this.isEmpty = this._parts.length == 0;
	this.length = 0;
	for (var i = 0; i < this._parts.length; i++) {
	  this.length += this._parts[i].length;
	}
}
ss_StringBuilder.prototype = {
	append: function#? DEBUG StringBuilder$append##(s) {
		if (!ss.isNullOrUndefined(s) && s !== '') {
			ss.add(this._parts, s);
	    this.length += s.length;
			this.isEmpty = false;
		}
		return this;
	},

	appendChar: function#? DEBUG StringBuilder$appendChar##(c) {
		return this.append(String.fromCharCode(c));
	},

	appendLine: function#? DEBUG StringBuilder$appendLine##(s) {
		this.append(s);
		this.append('\r\n');
		this.isEmpty = false;
		return this;
	},

	appendLineChar: function#? DEBUG StringBuilder$appendLineChar##(c) {
		return this.appendLine(String.fromCharCode(c));
	},

	clear: function#? DEBUG StringBuilder$clear##() {
		this._parts = [];
		this.isEmpty = true;
		this.length = 0;
	},

	length: function#? DEBUG StringBuilder$length##() {
		return this.length;
	},

	toString: function#? DEBUG StringBuilder$toString##() {
		return this._parts.join('');
	}
};

ss_StringBuilder.__typeName = 'ss.StringBuilder';
ss.StringBuilder = ss_StringBuilder;
ss.initClass(ss_StringBuilder);
