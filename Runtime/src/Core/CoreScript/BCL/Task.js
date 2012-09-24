///////////////////////////////////////////////////////////////////////////////
// Task

ss.Task = function#? DEBUG Task$##() {
	this.exception = null;
	this.status = 0;
	this._thens = [];
	this._result = null;
}
ss.Task.prototype = {
	continueWith: function#? DEBUG Task$continueWith##(continuation) {
		var tcs = new ss.TaskCompletionSource();
		var _this = this;
		var fn = function() {
			tcs.task.status = 3;
			try {
				var result = continuation(_this);
				tcs.setResult(result);
			}
			catch (e) {
				tcs.setException(ss.Exception.wrap(e));
			}
		};
		if (this.isCompleted()) {
			setTimeout(fn, 0);
		}
		else {
			this._thens.push(fn);
		}
		return tcs.task;
	},
	_runCallbacks: function#? DEBUG Task$_runCallbacks##() {
		for (var i = 0; i < this._thens.length; i++)
			this._thens[i](this);
		delete this._thens;
	},
	_complete: function#? DEBUG Task$_complete##(result) {
		if (this.isCompleted())
			return false;
		this._result = result;
		this.status = 5;
		this._runCallbacks();
		return true;
	},
	_fail: function#? DEBUG Task$_fail##(exception) {
		if (this.isCompleted())
			return false;
		this.exception = exception;
		this.status = 7;
		this._runCallbacks();
		return true;
	},
	_cancel: function#? DEBUG Task$_cancel##() {
		if (this.isCompleted())
			return false;
		this.status = 6;
		this._runCallbacks();
		return true;
	},
	isCanceled: function#? DEBUG Task$isCanceled##() {
		return this.status === 6;
	},
	isCompleted: function#? DEBUG Task$isCompleted##() {
		return this.status >= 5;
	},
	isFaulted: function#? DEBUG Task$isFaulted##() {
		return this.status === 7;
	},
	getResult: function#? DEBUG Task$getResult##() {
		switch (this.status) {
			case 5:
				return this._result;
			case 6:
				throw 'Task was cancelled.';
			case 7:
				throw this.exception;
			default:
				throw 'Task is not yet completed.';
		}
	},
	dispose: function#? DEBUG Task$dispose##() {
	}
};

ss.Task.delay = function#? DEBUG Task$delay##(delay) {
	var tcs = new ss.TaskCompletionSource();
	tcs.task.status = 3;
	setTimeout(function() {
		tcs.setResult(0);
	}, delay);
	return tcs.task;
};

ss.Task.registerClass('ss.Task', null, ss.IDisposable);
