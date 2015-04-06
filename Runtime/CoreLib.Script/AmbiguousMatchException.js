////////////////////////////////////////////////////////////////////////////////
// InvalidOperationException

var ss_AmbiguousMatchException = ss.AmbiguousMatchException = ss.mkType(ss, 'ss.AmbiguousMatchException',
	function#? DEBUG AmbiguousMatchException$##(message, innerException) {
		ss_Exception.call(this, message || 'Ambiguous match.', innerException);
	}
);

ss.initClass(ss_AmbiguousMatchException, ss_Exception);
