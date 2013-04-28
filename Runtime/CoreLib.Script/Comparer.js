///////////////////////////////////////////////////////////////////////////////
// Comparer

var ss_Comparer = function#? DEBUG Comparer$##(f) {
	this.f = f;
};
ss_Comparer.prototype.compare = function#? DEBUG Comparer$compare##(x, y) {
	return this.f(x, y);
};
ss_Comparer.create = function#? DEBUG Comparer$create##(f) {
	return new ss_Comparer(f);
};

ss_Comparer.__typeName = 'ss.Comparer';
ss.registerClass(global, 'ss.Comparer', ss_Comparer, null, [ss_IComparer]);
ss_Comparer.def = new ss_Comparer(ss.compare);
