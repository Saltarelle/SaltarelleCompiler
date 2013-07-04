////////////////////////////////////////////////////////////////////////////////
// InvalidOperationException

var ss_InvalidOperationException = function#? DEBUG InvalidOperationException$##(message, innerException) {
	ss_Exception.call(this, message || 'Operation is not valid due to the current state of the object.', innerException);
};
ss_InvalidOperationException.__typeName = 'ss.InvalidOperationException';
ss.InvalidOperationException = ss_InvalidOperationException;
ss.initClass(ss_InvalidOperationException, ss_Exception);
