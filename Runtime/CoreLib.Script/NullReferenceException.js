////////////////////////////////////////////////////////////////////////////////
// NullReferenceException

var ss_NullReferenceException = function#? DEBUG NullReferenceException$##(message, innerException) {
	ss_Exception.call(this, message || 'Object is null.', innerException);
};
ss_NullReferenceException.__typeName = 'ss.NullReferenceException';
ss.NullReferenceException = ss_NullReferenceException;
ss.initClass(ss_NullReferenceException, ss_Exception);
