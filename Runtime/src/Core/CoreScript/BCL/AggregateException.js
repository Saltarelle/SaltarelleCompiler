////////////////////////////////////////////////////////////////////////////////
// AggregateException

ss.AggregateException = function#? DEBUG AggregateException$##(message, innerExceptions) {
	if (typeof(message) !== 'string') {
		innerExceptions = message;
		message = 'One or more errors occurred.';
	}
	innerExceptions = ss.isValue(innerExceptions) ? Array.fromEnumerable(innerExceptions) : null;

	ss.Exception.call(this, message, innerExceptions && innerExceptions.length ? innerExceptions[0] : null);
	this._innerExceptions = innerExceptions;
};
ss.AggregateException.prototype = {
	get_innerExceptions: function#? DEBUG Exception$get_innerExceptions##() {
		return this._innerExceptions;
	}
};
ss.AggregateException.registerClass('ss.AggregateException', ss.Exception);
