////////////////////////////////////////////////////////////////////////////////
// FormatException

var ss_FormatException = function#? DEBUG FormatException$##(message, innerException) {
	ss_Exception.call(this, message || 'Invalid format.', innerException);
};
ss_FormatException.__typeName = 'ss.FormatException';
ss.FormatException = ss_FormatException;
ss.initClass(ss_FormatException, {}, ss_Exception);
