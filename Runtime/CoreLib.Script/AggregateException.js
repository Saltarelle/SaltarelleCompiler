////////////////////////////////////////////////////////////////////////////////
// AggregateException

var ss_AggregateException = function#? DEBUG AggregateException$##(message, innerExceptions) {
	this.innerExceptions = ss.isValue(innerExceptions) ? ss.arrayFromEnumerable(innerExceptions) : [];
	ss_Exception.call(this, message || 'One or more errors occurred.', this.innerExceptions.length ? this.innerExceptions[0] : null);
};

ss_AggregateException.__typeName = 'ss.AggregateException';
ss.AggregateException = ss_AggregateException;
ss.initClass(ss_AggregateException, ss, {
	flatten: function #? DEBUG AggregateException$flatten##() {
		var inner = [];
		for (var i = 0; i < this.innerExceptions.length; i++) {
			var e = this.innerExceptions[i];
			if (ss.isInstanceOfType(e, ss_AggregateException)) {
				inner.push.apply(inner, e.flatten().innerExceptions);
			}
			else {
				inner.push(e);
			}
		}
		return new ss_AggregateException(this._message, inner);
	}
}, ss_Exception);
