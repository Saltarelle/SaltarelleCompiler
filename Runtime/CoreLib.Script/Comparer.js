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
ss_Comparer.def = new ss_Comparer(ss.compare);
