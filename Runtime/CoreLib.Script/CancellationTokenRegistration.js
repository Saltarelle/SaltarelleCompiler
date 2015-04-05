///////////////////////////////////////////////////////////////////////////////
// CancellationTokenRegistration

var ss_CancellationTokenRegistration = function#? DEBUG CancellationTokenRegistration##(cts, o) {
	this._cts = cts;
	this._o = o;
};

ss_CancellationTokenRegistration.__typeName = 'ss.CancellationTokenRegistration';
ss.CancellationTokenRegistration = ss_CancellationTokenRegistration;
ss.initClass(ss_CancellationTokenRegistration, ss, {
	dispose: function#? DEBUG CancellationTokenRegistration$dispose##() {
		if (this._cts) {
			this._cts._deregister(this._o);
			this._cts = this._o = null;
		}
	},
	equalsT: function#? DEBUG CancellationTokenRegistration$equalsT##(o) {
		return this === o;
	}
}, Object, [ ss_IDisposable, ss_IEquatable ]);
ss_CancellationTokenRegistration.__class = false;
ss_CancellationTokenRegistration.getDefaultValue = function#? DEBUG CancellationTokenRegistration$default##() { return new ss_CancellationTokenRegistration(); };
