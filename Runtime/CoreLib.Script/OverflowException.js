////////////////////////////////////////////////////////////////////////////////
// OverflowException

var ss_OverflowException = ss.OverflowException = ss.mkType(ss, 'ss.OverflowException',
	function#? DEBUG OverflowException$##(message, innerException) {
		ss_ArithmeticException.call(this, message || 'Arithmetic operation resulted in an overflow.', innerException);
	}
);

ss.initClass(ss_OverflowException, ss_ArithmeticException);
