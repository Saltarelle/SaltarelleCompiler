///////////////////////////////////////////////////////////////////////////////
// CancellationToken

var ss_CancellationToken = ss.CancellationToken = ss.mkType(ss, 'ss.CancellationToken',
	function#? DEBUG CancellationToken##(source) {
		if (!(source instanceof ss_CancellationTokenSource))
			source = source ? ss_CancellationToken._sourceTrue : ss_CancellationToken._sourceFalse;
		this._source = source;
	},
	{
		get_canBeCanceled: function#? DEBUG CancellationToken$get_canBeCanceled##() {
			return !this._source._uncancellable;
		},
		get_isCancellationRequested: function#? DEBUG CancellationToken$get_isCancellationRequested##() {
			return this._source.isCancellationRequested;
		},
		throwIfCancellationRequested: function#? DEBUG CancellationToken$throwIfCancellationRequested##() {
			if (this._source.isCancellationRequested)
				throw new ss_OperationCanceledException(this);
		},
		register: function#? DEBUG CancellationToken$register##(cb, s) {
			return this._source._register(cb, s);
		}
	},
	{
		_sourceTrue:  { isCancellationRequested: true, _register: function #? DEBUG CancellationToken_sourceTrue$_register##(f, s) { f(s); return new ss_CancellationTokenRegistration(); } },
		_sourceFalse: { _uncancellable: true, isCancellationRequested: false, _register: function #? DEBUG CancellationToken_sourceFalse$_register##() { return new ss_CancellationTokenRegistration(); } }
	}
);
ss_CancellationToken.none = new ss_CancellationToken();

ss.initStruct(ss_CancellationToken);
