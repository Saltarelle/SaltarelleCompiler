////////////////////////////////////////////////////////////////////////////////
// DivideByZeroException

var ss_DivideByZeroException = ss.DivideByZeroException = ss.mkType(ss, 'ss.DivideByZeroException',
	function#? DEBUG DivideByZeroException$##(message, innerException) {
		ss_ArithmeticException.call(this, message || 'Division by 0.', innerException);
	}
);

ss.initClass(ss_DivideByZeroException, ss_ArithmeticException);
