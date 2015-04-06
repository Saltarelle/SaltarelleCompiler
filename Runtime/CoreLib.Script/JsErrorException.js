////////////////////////////////////////////////////////////////////////////////
// JsErrorException

var ss_JsErrorException = ss.JsErrorException = ss.mkType(ss, 'ss.JsErrorException', function#? DEBUG JsErrorException$##(error, message, innerException) {
		ss_Exception.call(this, message || error.message, innerException);
		this.error = error;
	},
	{
		get_stack: function#? DEBUG Exception$get_stack##() {
			return this.error.stack;
		}
	}
);

ss.initClass(ss_JsErrorException, ss_Exception);
