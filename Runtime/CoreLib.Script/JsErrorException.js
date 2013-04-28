////////////////////////////////////////////////////////////////////////////////
// JsErrorException

var ss_JsErrorException = function#? DEBUG JsErrorException$##(error) {
	ss_Exception.call(this, error.message);
	this._error = error;
};
ss_JsErrorException.prototype = {
	get_error: function#? DEBUG JsErrorException$get_error##() {
		return this._error;
	}
};
ss_JsErrorException.__typeName = 'ss.JsErrorException';
ss.registerClass(global, 'ss.JsErrorException', ss_JsErrorException, ss_Exception);
