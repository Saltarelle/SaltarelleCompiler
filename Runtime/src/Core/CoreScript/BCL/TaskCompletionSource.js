///////////////////////////////////////////////////////////////////////////////
// TaskCompletionSource

ss.TaskCompletionSource = function#? DEBUG TaskCompletionSource$##() {
	this.task = new ss.Task();
	this.task.status = 3;
};
ss.TaskCompletionSource.prototype = {
	setCanceled: function#? DEBUG TaskCompletionSource$setCanceled##() {
		if (!this.task._cancel())
			throw 'Task was already completed.';
	},
	setResult: function#? DEBUG TaskCompletionSource$setResult##(result) {
		if (!this.task._complete(result))
			throw 'Task was already completed.';
	},
	setException: function#? DEBUG TaskCompletionSource$setException##(exception) {
		if (Type.canCast(exception, ss.Exception))
			exception = [exception];
		exception = new ss.AggregateException(exception);
		if (!this.task._fail(exception))
			throw 'Task was already completed.';
	},
	trySetCanceled: function#? DEBUG TaskCompletionSource$trySetCanceled##() {
		return this.task._cancel();
	},
	trySetResult: function#? DEBUG TaskCompletionSource$setResult##(result) {
		return this.task._complete(result);
	},
	trySetException: function#? DEBUG TaskCompletionSource$setException##(exception) {
		if (Type.canCast(exception, ss.Exception))
			exception = [exception];
		exception = new ss.AggregateException(exception);
		return this.task._fail(exception);
	}
};

ss.TaskCompletionSource.registerClass('ss.TaskCompletionSource');
