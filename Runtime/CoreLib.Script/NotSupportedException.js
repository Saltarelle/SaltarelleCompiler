////////////////////////////////////////////////////////////////////////////////
// NotSupportedException

var ss_NotSupportedException = function#? DEBUG NotSupportedException$##(message, innerException) {
	ss_Exception.call(this, message, innerException);
};
ss_NotSupportedException.__typeName = 'ss.NotSupportedException';
ss.registerClass(global, 'ss.NotSupportedException', ss_NotSupportedException, ss_Exception);
