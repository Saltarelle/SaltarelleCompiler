////////////////////////////////////////////////////////////////////////////////
// OperationCanceledException

var ss_OperationCanceledException = function#? DEBUG OperationCanceledException$##(message, token, innerException) {
	ss_Exception.call(this, message || 'Operation was canceled.', innerException);
	this.cancellationToken = token || ss_CancellationToken.none;
};

ss_OperationCanceledException.__typeName = 'ss.OperationCanceledException';
ss.OperationCanceledException = ss_OperationCanceledException;
ss.initClass(ss_OperationCanceledException, ss, {}, ss_Exception);
