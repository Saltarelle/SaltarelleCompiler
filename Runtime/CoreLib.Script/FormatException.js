////////////////////////////////////////////////////////////////////////////////
// FormatException

var ss_FormatException = ss.FormatException = ss.mkType(ss, 'ss.FormatException',
	function#? DEBUG FormatException$##(message, innerException) {
		ss_Exception.call(this, message || 'Invalid format.', innerException);
	}
);

ss.initClass(ss_FormatException, ss_Exception);
