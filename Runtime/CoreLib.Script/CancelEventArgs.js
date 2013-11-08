///////////////////////////////////////////////////////////////////////////////
// CancelEventArgs

var ss_CancelEventArgs = function#? DEBUG CancelEventArgs$##() {
	ss_EventArgs.call(this);
	this.cancel = false;
}

ss_CancelEventArgs.__typeName = 'ss.CancelEventArgs';
ss.CancelEventArgs = ss_CancelEventArgs;
ss.initClass(ss_CancelEventArgs, ss, {}, ss_EventArgs);
