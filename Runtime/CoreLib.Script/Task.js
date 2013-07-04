///////////////////////////////////////////////////////////////////////////////
// Task

var ss_Task = function#? DEBUG Task$##(action, state) {
	this._action = action;
	this._state = state;
	this.exception = null;
	this.status = 0;
	this._thens = [];
	this._result = null;
};

ss_Task.prototype = {
	continueWith: function#? DEBUG Task$continueWith##(continuation) {
		var tcs = new ss_TaskCompletionSource();
		var _this = this;
		var fn = function() {
			try {
				tcs.setResult(continuation(_this));
			}
			catch (e) {
				tcs.setException(ss_Exception.wrap(e));
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
			throw new ss_InvalidOperationException('Task was already started.');
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
				_this._fail(new ss_AggregateException(null, [ss_Exception.wrap(e)]));
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
				throw new ss_InvalidOperationException('Task was cancelled.');
			case 7:
				throw this.exception;
			default:
				throw new ss_InvalidOperationException('Task is not yet completed.');
		}
	},
	dispose: function#? DEBUG Task$dispose##() {
	}
};

ss_Task.delay = function#? DEBUG Task$delay##(delay) {
	var tcs = new ss_TaskCompletionSource();
	setTimeout(function() {
		tcs.setResult(0);
	}, delay);
	return tcs.task;
};

ss_Task.fromResult = function#? DEBUG Task$fromResult##(result) {
	var t = new ss_Task();
	t.status = 5;
	t._result = result;
	return t;
};

ss_Task.run = function#? DEBUG Task$fromResult##(f) {
	var tcs = new ss_TaskCompletionSource();
	setTimeout(function() {
		try {
			tcs.setResult(f());
		}
		catch (e) {
			tcs.setException(ss_Exception.wrap(e));
		}
	}, 0);
	return tcs.task;
};

ss_Task.whenAll = function#? DEBUG Task$whenAll##(tasks) {
	var tcs = new ss_TaskCompletionSource();
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
							ss.arrayAddRange(exceptions, t.exception.innerExceptions);
							break;
						default:
							throw new ss_InvalidOperationException('Invalid task status ' + t.status);
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

ss_Task.whenAny = function#? DEBUG Task$whenAny##(tasks) {
	if (!tasks.length)
		throw new ss_ArgumentException('Must wait for at least one task', 'tasks');

	var tcs = new ss_TaskCompletionSource();
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
					tcs.trySetException(t.exception.innerExceptions);
					break;
				default:
					throw new ss_InvalidOperationException('Invalid task status ' + t.status);
			}
		});
	}
	return tcs.task;
};

ss_Task.fromDoneCallback = function#? DEBUG Task$fromDoneCallback##(t, i, m) {
	var tcs = new ss_TaskCompletionSource(), args;
    if (typeof(i) === 'number') {
        args = Array.prototype.slice.call(arguments, 3);
        if (i < 0)
            i += args.length + 1;
    }
    else {
        args = Array.prototype.slice.call(arguments, 2);
        m = i;
        i = args.length;
    }

	var cb = function(v) {
		tcs.setResult(v);
	};
	
    args = args.slice(0, i).concat(cb, args.slice(i));

	t[m].apply(t, args);
	return tcs.task;
};

ss_Task.fromPromise = function#? DEBUG Task$fromPromise##(p, f) {
	var tcs = new ss_TaskCompletionSource();
	if (typeof(f) === 'number')
		f = (function(i) { return function() { return arguments[i >= 0 ? i : (arguments.length + i)]; }; })(f);
    else if (typeof(f) !== 'function')
        f = function() { return Array.prototype.slice.call(arguments, 0); };

	p.then(function() {
		tcs.setResult(typeof(f) === 'function' ? f.apply(null, arguments) : null);
	}, function() {
		tcs.setException(new ss_PromiseException(Array.prototype.slice.call(arguments, 0)));
	});
	return tcs.task;
};

ss_Task.fromNode = function #? DEBUG Task$fromNode##(t, f, m) {
	var tcs = new ss_TaskCompletionSource(), args;
    if (typeof(f) === 'function') {
        args = Array.prototype.slice.call(arguments, 3);
    }
    else {
        args = Array.prototype.slice.call(arguments, 2);
        m = f;
		f = function() { return arguments[0]; };
    }

	var cb = function(e) {
		if (e)
			tcs.setException(ss_Exception.wrap(e));
		else
			tcs.setResult(f.apply(null, Array.prototype.slice.call(arguments, 1)));
	};
	
	args.push(cb);

	t[m].apply(t, args);
	return tcs.task;
};

ss_Task.__typeName = 'ss.Task';
ss.Task = ss_Task;
ss.initClass(ss_Task, null, [ss_IDisposable]);

////////////////////////////////////////////////////////////////////////////////
// TaskStatus
var ss_TaskStatus = function() {
};
ss_TaskStatus.prototype = { created: 0, running: 3, ranToCompletion: 5, canceled: 6, faulted: 7 };
ss_TaskStatus.__typeName = 'ss.TaskStatus';
ss.TaskStatus = ss_TaskStatus;
ss.initEnum(ss_TaskStatus);
