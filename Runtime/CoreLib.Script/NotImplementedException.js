////////////////////////////////////////////////////////////////////////////////
// NotImplementedException

var ss_NotImplementedException = ss.NotImplementedException = ss.mkType(ss, 'ss.NotImplementedException',
	function#? DEBUG NotImplementedException$##(message, innerException) {
		ss_Exception.call(this, message || 'The method or operation is not implemented.', innerException);
	}
);

ss.initClass(ss_NotImplementedException, ss_Exception);
