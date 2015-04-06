////////////////////////////////////////////////////////////////////////////////
// TaskCanceledException

var ss_TaskCanceledException = ss.TaskCanceledException = ss.mkType(ss, 'ss.TaskCanceledException',
	function#? DEBUG TaskCanceledException$##(message, task, innerException) {
		ss_OperationCanceledException.call(this, message || 'A task was canceled.', null, innerException);
		this.task = task || null;
	}
);

ss.initClass(ss_TaskCanceledException, ss_OperationCanceledException);
