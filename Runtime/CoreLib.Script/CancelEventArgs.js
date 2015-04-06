///////////////////////////////////////////////////////////////////////////////
// CancelEventArgs

var ss_CancelEventArgs = ss.CancelEventArgs = ss.mkType(ss, 'ss.CancelEventArgs',
	function#? DEBUG CancelEventArgs$##() {
		ss_EventArgs.call(this);
		this.cancel = false;
	}
);

ss.initClass(ss_CancelEventArgs, ss_EventArgs);
