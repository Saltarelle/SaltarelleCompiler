///////////////////////////////////////////////////////////////////////////////
// CancellationToken

var ss_CancellationToken = function#? DEBUG CancellationToken##(source) {
	if (!(source instanceof ss_CancellationTokenSource))
		source = source ? ss_CancellationToken._sourceTrue : ss_CancellationToken._sourceFalse;
	this._source = source;
};

ss_CancellationToken.__typeName = 'ss.CancellationToken';
ss.CancellationToken = ss_CancellationToken;
ss.initClass(ss_CancellationToken, ss, {
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
});
ss_CancellationToken.__class = false;
ss_CancellationToken.getDefaultValue = function#? DEBUG CancellationToken$default##() { return new ss_CancellationToken(); };

ss_CancellationToken._sourceTrue  = { isCancellationRequested: true, _register: function(f, s) { f(s); return new ss_CancellationTokenRegistration(); } };
ss_CancellationToken._sourceFalse = { _uncancellable: true, isCancellationRequested: false, _register: function() { return new ss_CancellationTokenRegistration(); } };

ss_CancellationToken.none = new ss_CancellationToken();
