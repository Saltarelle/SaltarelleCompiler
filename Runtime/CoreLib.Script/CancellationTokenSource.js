///////////////////////////////////////////////////////////////////////////////
// CancellationTokenSource

var ss_CancellationTokenSource = ss.CancellationTokenSource = ss.mkType(ss, 'ss.CancellationTokenSource',
	function#? DEBUG CancellationTokenSource$##(delay) {
		this._timeout = typeof delay === 'number' && delay >= 0 ? setTimeout(ss.mkdel(this, 'cancel'), delay, -1) : null;
		this.isCancellationRequested = false;
		this.token = new ss_CancellationToken(this);
		this._handlers = [];
	},
	{
		cancel: function#? DEBUG TaskCompletionSource$cancel##(throwFirst) {
			if (this.isCancellationRequested)
				return ;

			this.isCancellationRequested = true;
			var x = [];
			var h = this._handlers;

			this._clean();

			for (var i = 0; i < h.length; i++) {
				try {
					h[i].f(h[i].s);
				}
				catch (ex) {
					if (throwFirst && throwFirst !== -1)
						throw ex;
					x.push(ex);
				}
			}
			if (x.length > 0 && throwFirst !== -1)
				throw new ss_AggregateException(null, x);
		},
		cancelAfter: function#? DEBUG TaskCompletionSource$cancelAfter##(delay) {
			if (this.isCancellationRequested)
				return;
			if (this._timeout)
				clearTimeout(this._timeout);
			this._timeout = setTimeout(ss.mkdel(this, 'cancel'), delay, -1);
		},
		_register: function#? DEBUG TaskCompletionSource$_register##(f, s) {
			if (this.isCancellationRequested) {
				f(s);
				return new ss_CancellationTokenRegistration();
			}
			else {
				var o = {f: f, s: s };
				this._handlers.push(o);
				return new ss_CancellationTokenRegistration(this, o);
			}
		},
		_deregister: function#? DEBUG TaskCompletionSource$_deregister##(o) {
			var ix = this._handlers.indexOf(o);
			if (ix >= 0)
				this._handlers.splice(ix, 1);
		},
		dispose: function#? DEBUG TaskCompletionSource$dispose##(delay) {
			this._clean();
		},
		_clean: function#? DEBUG TaskCompletionSource$_clean##(delay) {
			if (this._timeout)
				clearTimeout(this._timeout);
			this._timeout = null;
			this._handlers = [];
			if (this._links) {
				for (var i = 0; i < this._links.length; i++)
					this._links[i].dispose();
				this._links = null;
			}
		}
	},
	{
		createLinked: function#? DEBUG CancellationTokenSource$createLinked##() {
			var cts = new ss_CancellationTokenSource();
			cts._links = [];
			var d = ss.mkdel(cts, 'cancel');
			for (var i = 0; i < arguments.length; i++) {
				cts._links.push(arguments[i].register(d));
			}
			return cts;
		}
	}
);

ss.initClass(ss_CancellationTokenSource, null, [ss_IDisposable]);
