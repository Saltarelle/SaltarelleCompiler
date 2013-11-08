///////////////////////////////////////////////////////////////////////////////
// TaskCompletionSource

var ss_TaskCompletionSource = function#? DEBUG TaskCompletionSource$##() {
	this.task = new ss_Task();
	this.task.status = 3;
};

ss_TaskCompletionSource.__typeName = 'ss.TaskCompletionSource';
ss.TaskCompletionSource = ss_TaskCompletionSource;
ss.initClass(ss_TaskCompletionSource, ss, {
	setCanceled: function#? DEBUG TaskCompletionSource$setCanceled##() {
		if (!this.task._cancel())
			throw new ss_InvalidOperationException('Task was already completed.');
	},
	setResult: function#? DEBUG TaskCompletionSource$setResult##(result) {
		if (!this.task._complete(result))
			throw new ss_InvalidOperationException('Task was already completed.');
	},
	setException: function#? DEBUG TaskCompletionSource$setException##(exception) {
		if (!this.trySetException(exception))
			throw new ss_InvalidOperationException('Task was already completed.');
	},
	trySetCanceled: function#? DEBUG TaskCompletionSource$trySetCanceled##() {
		return this.task._cancel();
	},
	trySetResult: function#? DEBUG TaskCompletionSource$setResult##(result) {
		return this.task._complete(result);
	},
	trySetException: function#? DEBUG TaskCompletionSource$setException##(exception) {
		if (!ss.isInstanceOfType(exception, ss_AggregateException)) {
			if (ss.isInstanceOfType(exception, ss_Exception))
				exception = [exception];
			exception = new ss_AggregateException(null, exception);
		}
		return this.task._fail(exception);
	}
});
