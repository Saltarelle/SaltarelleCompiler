///////////////////////////////////////////////////////////////////////////////
// Comparer

var ss_Comparer = function#? DEBUG Comparer$##(f) {
	this.f = f;
};

ss_Comparer.__typeName = 'ss.Comparer';
ss.Comparer = ss_Comparer;
ss.initClass(ss_Comparer, ss, {
	compare: function#? DEBUG Comparer$compare##(x, y) {
		return this.f(x, y);
	}
}, null, [ss_IComparer]);
ss_Comparer.def = new ss_Comparer(function#? DEBUG Comparer$defaultCompare##(a, b) {
	if (!ss.isValue(a))
		return !ss.isValue(b)? 0 : -1;
	else if (!ss.isValue(b))
		return 1;
	else
		return ss.compare(a, b);
});
