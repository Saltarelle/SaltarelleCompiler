////////////////////////////////////////////////////////////////////////////////
// ArithmeticException

var ss_ArithmeticException = ss.ArithmeticException = ss.mkType(ss, 'ss.ArithmeticException',
	function#? DEBUG ArithmeticException$##(message, innerException) {
		ss_Exception.call(this, message || 'Overflow or underflow in the arithmetic operation.', innerException);
	}
);

ss.initClass(ss_ArithmeticException, ss_Exception);
