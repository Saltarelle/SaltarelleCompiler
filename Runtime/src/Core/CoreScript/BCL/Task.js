///////////////////////////////////////////////////////////////////////////////
// Task

ss.Task = function#? DEBUG Task$##(action, state) {
	this._action = action;
	this._state = state;
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
			try {
				tcs.setResult(continuation(_this));
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
	start: function#? DEBUG Task$start##() {
		if (this.status !== 0)
			throw 'Task was already started.';
		var _this = this;
		this.status = 3;
		setTimeout(function() {
			try {
				var result = _this._action(_this._state);
				delete _this._action;
				delete _this._state;
				_this._complete(result);
			}
			catch (e) {
				_this._fail(new ss.AggregateException([ss.Exception.wrap(e)]));
			}
		}, 0);
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
	setTimeout(function() {
		tcs.setResult(0);
	}, delay);
	return tcs.task;
};

ss.Task.fromResult = function#? DEBUG Task$fromResult##(result) {
	var t = new ss.Task();
	t.status = 5;
	t._result = result;
	return t;
};

ss.Task.run = function#? DEBUG Task$fromResult##(f) {
	var tcs = new ss.TaskCompletionSource();
	setTimeout(function() {
		try {
			tcs.setResult(f());
		}
		catch (e) {
			tcs.setException(ss.Exception.wrap(e));
		}
	}, 0);
	return tcs.task;
};

ss.Task.whenAll = function#? DEBUG Task$whenAll##(tasks) {
	var tcs = new ss.TaskCompletionSource();
	if (tasks.length === 0) {
		tcs.setResult([]);
	}
	else {
		var result = new Array(tasks.length), remaining = tasks.length, cancelled = false, exceptions = [];
		for (var i = 0; i < tasks.length; i++) {
			(function(i) {
				tasks[i].continueWith(function(t) {
					switch (t.status) {
						case 5:
							result[i] = t.getResult();
							break;
						case 6:
							cancelled = true;
							break;
						case 7:
							exceptions.addRange(t.exception.get_innerExceptions());
							break;
						default:
							throw 'Invalid task status ' + t.status;
					}
					if (--remaining === 0) {
						if (exceptions.length > 0)
							tcs.setException(exceptions);
						else if (cancelled)
							tcs.setCanceled();
						else
							tcs.setResult(result);
					}
				});
			})(i);
		}
	}
	return tcs.task;
};

ss.Task.whenAny = function#? DEBUG Task$whenAny##(tasks) {
	if (!tasks.length)
		throw 'Must wait for at least one task';

	var tcs = new ss.TaskCompletionSource();
	for (var i = 0; i < tasks.length; i++) {
		tasks[i].continueWith(function(t) {
			switch (t.status) {
				case 5:
					tcs.trySetResult(t);
					break;
				case 6:
					tcs.trySetCanceled();
					break;
				case 7:
					tcs.trySetException(t.exception.get_innerExceptions());
					break;
				default:
					throw 'Invalid task status ' + t.status;
			}
		});
	}
	return tcs.task;
};

ss.Task.registerClass('ss.Task', null, ss.IDisposable);
