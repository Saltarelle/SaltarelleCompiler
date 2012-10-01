///////////////////////////////////////////////////////////////////////////////
// Exception

ss.Exception = function#? DEBUG Exception$##(message, innerException) {
	this._message = message || null;
	this._innerException = innerException || null;
}
ss.Exception.registerClass('ss.Exception');

ss.Exception.prototype = {
	get_message: function#? DEBUG Exception$get_message##() {
		return this._message;
	},
	get_innerException: function#? DEBUG Exception$get_innerException##() {
		return this._innerException;
	}
};

ss.Exception.wrap = function#? DEBUG Exception$get_message##(o) {
	if (ss.Exception.isInstanceOfType(o)) {
		return o;
	}
	else {
		return new ss.Exception(o);
	}
};
