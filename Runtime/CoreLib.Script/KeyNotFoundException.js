////////////////////////////////////////////////////////////////////////////////
// KeyNotFoundException

var ss_KeyNotFoundException = ss.KeyNotFoundException = ss.mkType(ss, 'ss.KeyNotFoundException',
	function#? DEBUG KeyNotFoundException$##(message, innerException) {
		ss_Exception.call(this, message || 'Key not found.', innerException);
	}
);
ss.initClass(ss_KeyNotFoundException, ss_Exception);
