////////////////////////////////////////////////////////////////////////////////
// ArgumentException

var ss_ArgumentException = function#? DEBUG ArgumentException$##(message, paramName, innerException) {
	ss_Exception.call(this, message || 'Value does not fall within the expected range.', innerException);
	this.paramName = paramName || null;
};

ss_ArgumentException.__typeName = 'ss.ArgumentException';
ss.ArgumentException = ss_ArgumentException;
ss.initClass(ss_ArgumentException, {}, ss_Exception);
