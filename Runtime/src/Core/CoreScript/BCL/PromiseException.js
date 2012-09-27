////////////////////////////////////////////////////////////////////////////////
// PromiseException

ss.PromiseException = function#? DEBUG PromiseException##(args) {
	this._arguments = args.clone();
};
ss.PromiseException.prototype = {
	get_arguments: function#? DEBUG PromiseException$get_arguments##() {
		return this._arguments;
	}
};
ss.PromiseException.registerClass('ss.PromiseException', ss.Exception);
