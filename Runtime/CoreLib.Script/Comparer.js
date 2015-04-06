///////////////////////////////////////////////////////////////////////////////
// Comparer

var ss_Comparer = ss.Comparer = ss.mkType(ss, 'ss.Comparer',
	function#? DEBUG Comparer$##(f) {
		this.f = f;
	},
	{
		compare: function#? DEBUG Comparer$compare##(x, y) {
			return this.f(x, y);
		}
	}
);

ss.initClass(ss_Comparer, null, [ss_IComparer]);

ss_Comparer.def = new ss_Comparer(function#? DEBUG Comparer$defaultCompare##(a, b) {
	if (!ss.isValue(a))
		return !ss.isValue(b)? 0 : -1;
	else if (!ss.isValue(b))
		return 1;
	else
		return ss.compare(a, b);
});
