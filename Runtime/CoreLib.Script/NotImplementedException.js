////////////////////////////////////////////////////////////////////////////////
// NotImplementedException

var ss_NotImplementedException = function#? DEBUG NotImplementedException$##(message, innerException) {
	ss_Exception.call(this, message || 'The method or operation is not implemented.', innerException);
};
ss_NotImplementedException.__typeName = 'ss.NotImplementedException';
ss.NotImplementedException = ss_NotImplementedException;
ss.initClass(ss_NotImplementedException, ss, {}, ss_Exception);
