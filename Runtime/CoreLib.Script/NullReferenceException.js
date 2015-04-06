////////////////////////////////////////////////////////////////////////////////
// NullReferenceException

var ss_NullReferenceException = ss.NullReferenceException = ss.mkType(ss, 'ss.NullReferenceException',
	function#? DEBUG NullReferenceException$##(message, innerException) {
		ss_Exception.call(this, message || 'Object is null.', innerException);
	}
);

ss.initClass(ss_NullReferenceException, ss_Exception);
