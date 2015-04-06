////////////////////////////////////////////////////////////////////////////////
// InvalidCastException

var ss_InvalidCastException = ss.InvalidCastException = ss.mkType(ss, 'ss.InvalidCastException',
	function#? DEBUG InvalidCastException$##(message, innerException) {
		ss_Exception.call(this, message || 'The cast is not valid.', innerException);
	}
);

ss.initClass(ss_InvalidCastException, ss_Exception);
