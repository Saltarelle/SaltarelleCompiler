///////////////////////////////////////////////////////////////////////////////
// Exception

var ss_Exception = function#? DEBUG Exception$##(message, innerException) {
	this._message = message || null;
	this._innerException = innerException || null;
}
Type.registerClass(global, 'ss.Exception', ss_Exception);

ss_Exception.prototype = {
	get_message: function#? DEBUG Exception$get_message##() {
		return this._message;
	},
	get_innerException: function#? DEBUG Exception$get_innerException##() {
		return this._innerException;
	}
};

ss_Exception.wrap = function#? DEBUG Exception$wrap##(o) {
	if (ss_Exception.isInstanceOfType(o)) {
		return o;
	}
	else if (o instanceof Error) {
		return new ss_JsErrorException(o);
	}
	else {
		return new ss_Exception(o.toString());
	}
};
