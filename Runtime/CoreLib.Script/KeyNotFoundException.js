////////////////////////////////////////////////////////////////////////////////
// KeyNotFoundException

var ss_KeyNotFoundException = function#? DEBUG KeyNotFoundException$##(message, innerException) {
	ss_Exception.call(this, message || 'Key not found.', innerException);
};
ss_KeyNotFoundException.__typeName = 'ss.KeyNotFoundException';
ss.KeyNotFoundException = ss_KeyNotFoundException;
ss.initClass(ss_KeyNotFoundException, ss_Exception);
