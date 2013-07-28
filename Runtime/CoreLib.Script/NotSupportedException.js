////////////////////////////////////////////////////////////////////////////////
// NotSupportedException

var ss_NotSupportedException = function#? DEBUG NotSupportedException$##(message, innerException) {
	ss_Exception.call(this, message || 'Specified method is not supported.', innerException);
};
ss_NotSupportedException.__typeName = 'ss.NotSupportedException';
ss.NotSupportedException = ss_NotSupportedException;
ss.initClass(ss_NotSupportedException, {}, ss_Exception);
