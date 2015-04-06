////////////////////////////////////////////////////////////////////////////////
// ArgumentException

var ss_ArgumentException = ss.ArgumentException = ss.mkType(ss, 'ss.ArgumentException',
	function#? DEBUG ArgumentException$##(message, paramName, innerException) {
		ss_Exception.call(this, message || 'Value does not fall within the expected range.', innerException);
		this.paramName = paramName || null;
	}
);

ss.initClass(ss_ArgumentException, ss_Exception);
