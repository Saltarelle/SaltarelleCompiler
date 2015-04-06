////////////////////////////////////////////////////////////////////////////////
// OperationCanceledException

var ss_OperationCanceledException = ss.OperationCanceledException = ss.mkType(ss, 'ss.OperationCanceledException',
	function#? DEBUG OperationCanceledException$##(message, token, innerException) {
		ss_Exception.call(this, message || 'Operation was canceled.', innerException);
		this.cancellationToken = token || ss_CancellationToken.none;
	}
);

ss.initClass(ss_OperationCanceledException, ss_Exception);
