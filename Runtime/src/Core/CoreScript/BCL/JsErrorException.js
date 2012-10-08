////////////////////////////////////////////////////////////////////////////////
// JsErrorException

ss.JsErrorException = function#? DEBUG JsErrorException$##(error) {
	ss.Exception.call(this, error.message);
	this._error = error;
};
ss.JsErrorException.prototype = {
	get_error: function#? DEBUG JsErrorException$get_error##() {
		return this._error;
	}
};
ss.JsErrorException.registerClass('ss.JsErrorException', ss.Exception);
