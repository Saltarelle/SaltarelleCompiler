///////////////////////////////////////////////////////////////////////////////
// StringBuilder

var ss_StringBuilder = function#? DEBUG StringBuilder$##(s) {
	this._parts = (ss.isValue(s) && s != '') ? [s] : [];
	this.length = ss.isValue(s) ? s.length : 0;
}

ss_StringBuilder.__typeName = 'ss.StringBuilder';
ss.StringBuilder = ss_StringBuilder;
ss.initClass(ss_StringBuilder, {
	append: function#? DEBUG StringBuilder$append##(o) {
		if (ss.isValue(o)) {
			var s = o.toString();
			ss.add(this._parts, s);
			this.length += s.length;
		}
		return this;
	},

	appendChar: function#? DEBUG StringBuilder$appendChar##(c) {
		return this.append(String.fromCharCode(c));
	},

	appendLine: function#? DEBUG StringBuilder$appendLine##(s) {
		this.append(s);
		this.append('\r\n');
		return this;
	},

	appendLineChar: function#? DEBUG StringBuilder$appendLineChar##(c) {
		return this.appendLine(String.fromCharCode(c));
	},

	clear: function#? DEBUG StringBuilder$clear##() {
		this._parts = [];
		this.length = 0;
	},

	toString: function#? DEBUG StringBuilder$toString##() {
		return this._parts.join('');
	}
});
