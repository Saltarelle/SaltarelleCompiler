////////////////////////////////////////////////////////////////////////////////
// DivideByZeroException

var ss_DivideByZeroException = function#? DEBUG DivideByZeroException$##(message, innerException) {
	ss_Exception.call(this, message || 'Division by 0.', innerException);
};
ss_DivideByZeroException.__typeName = 'ss.DivideByZeroException';
ss.DivideByZeroException = ss_DivideByZeroException;
ss.initClass(ss_DivideByZeroException, ss, {}, ss_Exception);
