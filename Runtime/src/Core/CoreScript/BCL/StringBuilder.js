///////////////////////////////////////////////////////////////////////////////
// StringBuilder

var ss_StringBuilder = function#? DEBUG StringBuilder$##(s) {
    this._parts = ss.isNullOrUndefined(s) || s === '' ? [] : [s];
    this.isEmpty = this._parts.length == 0;
}
ss_StringBuilder.prototype = {
    append: function#? DEBUG StringBuilder$append##(s) {
        if (!ss.isNullOrUndefined(s) && s !== '') {
            this._parts.add(s);
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
    },

    toString: function#? DEBUG StringBuilder$toString##() {
        return this._parts.join('');
    }
};

Type.registerClass(global, 'ss.StringBuilder', ss_StringBuilder);
