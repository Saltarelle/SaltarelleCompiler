////////////////////////////////////////////////////////////////////////////////
// TaskCanceledException

var ss_TaskCanceledException = function#? DEBUG TaskCanceledException$##(message, task, innerException) {
	ss_OperationCanceledException.call(this, message || 'A task was canceled.', null, innerException);
	this.task = task || null;
};

ss_TaskCanceledException.__typeName = 'ss.TaskCanceledException';
ss.TaskCanceledException = ss_TaskCanceledException;
ss.initClass(ss_TaskCanceledException, ss, {}, ss_OperationCanceledException);
