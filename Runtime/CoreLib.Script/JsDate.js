///////////////////////////////////////////////////////////////////////////////
// MutableDateTime

var ss_JsDate = ss.JsDate = ss.mkType(ss, 'ss.JsDate',
	function#? DEBUG JsDate$##() {
	},
	null,
	{
		createInstance: function#? DEBUG JsDate$createInstance##() {
			return new Date();
		},
		isInstanceOfType: function#? DEBUG JsDate$isInstanceOfType##(instance) {
			return instance instanceof Date;
		}
	}
);

ss.initClass(ss_JsDate, null, [ ss_IEquatable, ss_IComparable ]);
