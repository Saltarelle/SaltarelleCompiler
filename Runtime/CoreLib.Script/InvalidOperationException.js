////////////////////////////////////////////////////////////////////////////////
// InvalidOperationException

var ss_InvalidOperationException = ss.InvalidOperationException = ss.mkType(ss, 'ss.InvalidOperationException',
	function#? DEBUG InvalidOperationException$##(message, innerException) {
		ss_Exception.call(this, message || 'Operation is not valid due to the current state of the object.', innerException);
	}
);
ss.initClass(ss_InvalidOperationException, ss_Exception);
