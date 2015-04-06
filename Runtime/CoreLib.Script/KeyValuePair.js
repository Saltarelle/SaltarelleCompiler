///////////////////////////////////////////////////////////////////////////////
// KeyValuePair

var ss_KeyValuePair = ss.KeyValuePair = ss.mkType(ss, 'ss.KeyValuePair',
	function#? DEBUG KeyValuePair$##() {
	},
	null,
	{
		createInstance: function#? DEBUG KeyValuePair$createInstance##() {
			return { key: null, value: null };
		},
		isInstanceOfType: function#? DEBUG KeyValuePair$isInstanceOfType##(o) {
			return typeof o === 'object' && 'key' in o && 'value' in o;
		}
	}
);

ss.initStruct(ss_KeyValuePair);
