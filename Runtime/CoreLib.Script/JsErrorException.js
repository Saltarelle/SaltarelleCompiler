////////////////////////////////////////////////////////////////////////////////
// JsErrorException

var ss_JsErrorException = function#? DEBUG JsErrorException$##(error, message, innerException) {
	ss_Exception.call(this, message || error.message, innerException);
	this.error = error;
};
ss_JsErrorException.__typeName = 'ss.JsErrorException';
ss.JsErrorException = ss_JsErrorException;
ss.initClass(ss_JsErrorException, ss, {
	get_stack: function#? DEBUG Exception$get_stack##() {
		return this.error.stack;
	}
}, ss_Exception);
