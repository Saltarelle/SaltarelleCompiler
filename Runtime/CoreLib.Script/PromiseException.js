////////////////////////////////////////////////////////////////////////////////
// PromiseException

var ss_PromiseException = function#? DEBUG PromiseException##(args, message, innerException) {
	ss_Exception.call(this, message || (args.length && args[0] ? args[0].toString() : 'An error occurred'), innerException);
	this.arguments = ss.arrayClone(args);
};

ss_PromiseException.__typeName = 'ss.PromiseException';
ss.PromiseException = ss_PromiseException;
ss.initClass(ss_PromiseException, ss, {
	get_arguments: function#? DEBUG PromiseException$get_arguments##() {
		return this._arguments;
	}
}, ss_Exception);
