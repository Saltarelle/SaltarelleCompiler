////////////////////////////////////////////////////////////////////////////////
// InvalidOperationException

var ss_AmbiguousMatchException = function#? DEBUG AmbiguousMatchException$##(message, innerException) {
	ss_Exception.call(this, message || 'Ambiguous match.', innerException);
};
ss_AmbiguousMatchException.__typeName = 'ss.AmbiguousMatchException';
ss.AmbiguousMatchException = ss_AmbiguousMatchException;
ss.initClass(ss_AmbiguousMatchException, ss_Exception);
