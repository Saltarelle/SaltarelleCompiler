///////////////////////////////////////////////////////////////////////////////
// EqualityComparer

var ss_EqualityComparer = ss.EqualityComparer = ss.mkType(ss, 'ss.EqualityComparer',
	function#? DEBUG EqualityComparer$##() {
	},
	{
		areEqual: function#? DEBUG EqualityComparer$areEqual##(x, y) {
			return ss.staticEquals(x, y);
		},
		getObjectHashCode: function#? DEBUG EqualityComparer$getObjectHashCode##(obj) {
			return ss.isValue(obj) ? ss.getHashCode(obj) : 0;
		}
	}
);

ss.initClass(ss_EqualityComparer, null, [ss_IEqualityComparer]);
ss_EqualityComparer.def = new ss_EqualityComparer();
