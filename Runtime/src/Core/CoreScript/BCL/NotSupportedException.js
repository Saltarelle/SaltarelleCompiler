////////////////////////////////////////////////////////////////////////////////
// NotSupportedException

ss.NotSupportedException = function#? DEBUG NotSupportedException$##(message, innerException) {
	ss.Exception.call(this, message, innerException);
};
ss.NotSupportedException.registerClass('ss.NotSupportedException', ss.Exception);
