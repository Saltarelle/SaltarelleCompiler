////////////////////////////////////////////////////////////////////////////////
// InvalidCastException

var ss_InvalidCastException = function#? DEBUG InvalidCastException$##(message, innerException) {
	ss_Exception.call(this, message || 'The cast is not valid.', innerException);
};
ss_InvalidCastException.__typeName = 'ss.InvalidCastException';
ss.InvalidCastException = ss_InvalidCastException;
ss.initClass(ss_InvalidCastException, ss_Exception);
