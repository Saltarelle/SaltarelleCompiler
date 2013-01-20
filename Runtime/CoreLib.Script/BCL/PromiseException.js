////////////////////////////////////////////////////////////////////////////////
// PromiseException

var ss_PromiseException = function#? DEBUG PromiseException##(args) {
	ss_Exception.call(this, args[0] ? args[0].toString() : 'An error occurred');
	this._arguments = args.clone();
};
ss_PromiseException.prototype = {
	get_arguments: function#? DEBUG PromiseException$get_arguments##() {
		return this._arguments;
	}
};
Type.registerClass(global, 'ss.PromiseException', ss_PromiseException, ss_Exception);
