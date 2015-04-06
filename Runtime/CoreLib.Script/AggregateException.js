////////////////////////////////////////////////////////////////////////////////
// AggregateException

var ss_AggregateException = ss.AggregateException = ss.mkType(ss, 'ss.AggregateException',
	function#? DEBUG AggregateException$##(message, innerExceptions) {
		this.innerExceptions = ss.isValue(innerExceptions) ? ss.arrayFromEnumerable(innerExceptions) : [];
		ss_Exception.call(this, message || 'One or more errors occurred.', this.innerExceptions.length ? this.innerExceptions[0] : null);
	},
	{
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
	}
);

ss.initClass(ss_AggregateException, ss_Exception);
