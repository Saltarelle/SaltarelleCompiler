///////////////////////////////////////////////////////////////////////////////
// CancelEventArgs

var ss_CancelEventArgs = function#? DEBUG CancelEventArgs$##() {
	ss_CancelEventArgs.call(this);
	this.cancel = false;
}

ss_CancelEventArgs.__typeName = 'ss.CancelEventArgs';
ss.registerClass(global, 'ss.CancelEventArgs', ss_CancelEventArgs, ss_EventArgs);
