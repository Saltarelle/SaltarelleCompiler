////////////////////////////////////////////////////////////////////////////////
// AggregateException

var ss_AggregateException = function#? DEBUG AggregateException$##(message, innerExceptions) {
	this.innerExceptions = ss.isValue(innerExceptions) ? ss.arrayFromEnumerable(innerExceptions) : [];
	ss_Exception.call(this, message || 'One or more errors occurred.', this.innerExceptions.length ? this.innerExceptions[0] : null);
};

ss_AggregateException.__typeName = 'ss.AggregateException';
ss.AggregateException = ss_AggregateException;
ss.initClass(ss_AggregateException, ss, {}, ss_Exception);
