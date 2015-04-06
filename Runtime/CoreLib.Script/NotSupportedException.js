////////////////////////////////////////////////////////////////////////////////
// NotSupportedException

var ss_NotSupportedException = ss.NotSupportedException = ss.mkType(ss, 'ss.NotSupportedException',
	function#? DEBUG NotSupportedException$##(message, innerException) {
		ss_Exception.call(this, message || 'Specified method is not supported.', innerException);
	}
);

ss.initClass(ss_NotSupportedException, ss_Exception);
