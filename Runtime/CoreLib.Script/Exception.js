///////////////////////////////////////////////////////////////////////////////
// Exception

var ss_Exception = function#? DEBUG Exception$##(message, innerException) {
	this._message = message || 'An error occurred.';
	this._innerException = innerException || null;
	this._error = new Error();
}

ss_Exception.__typeName = 'ss.Exception';
ss.Exception = ss_Exception;
ss.initClass(ss_Exception, ss, {
	get_message: function#? DEBUG Exception$get_message##() {
		return this._message;
	},
	get_innerException: function#? DEBUG Exception$get_innerException##() {
		return this._innerException;
	},
	get_stack: function#? DEBUG Exception$get_stack##() {
		return this._error.stack;
	}
});

ss_Exception.wrap = function#? DEBUG Exception$wrap##(o) {
	if (ss.isInstanceOfType(o, ss_Exception)) {
		return o;
	}
	else if (o instanceof TypeError) {
		// TypeError can either be 'cannot read property blah of null/undefined' (proper NullReferenceException), or it can be eg. accessing a non-existent method of an object.
		// As long as all code is compiled, they should with a very high probability indicate the use of a null reference.
		return new ss_NullReferenceException(o.message, new ss_JsErrorException(o));
	}
	else if (o instanceof RangeError) {
		return new ss_ArgumentOutOfRangeException(null, o.message, new ss_JsErrorException(o));
	}
	else if (o instanceof Error) {
		return new ss_JsErrorException(o);
	}
	else {
		return new ss_Exception(o.toString());
	}
};
