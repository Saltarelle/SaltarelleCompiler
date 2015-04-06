///////////////////////////////////////////////////////////////////////////////
// CancellationTokenRegistration

var ss_CancellationTokenRegistration = ss.CancellationTokenRegistration = ss.mkType(ss, 'ss.CancellationTokenRegistration',
	function#? DEBUG CancellationTokenRegistration##(cts, o) {
		this._cts = cts;
		this._o = o;
	},
	{
		dispose: function#? DEBUG CancellationTokenRegistration$dispose##() {
			if (this._cts) {
				this._cts._deregister(this._o);
				this._cts = this._o = null;
			}
		},
		equalsT: function#? DEBUG CancellationTokenRegistration$equalsT##(o) {
			return this === o;
		}
	}
);

ss.initStruct(ss_CancellationTokenRegistration, [ ss_IDisposable, ss_IEquatable ]);
