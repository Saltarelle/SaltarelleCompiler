///////////////////////////////////////////////////////////////////////////////
// IComparer

var ss_IComparer = function#? DEBUG IComparer$##() { };

ss_IComparer.__typeName = 'ss.IComparer';
ss.IComparer = ss_IComparer;
ss.initInterface(ss_IComparer, ss, { compare: null });

ss.getComparer = function#? DEBUG ss$getComparer##(comparer) {
	return function () {
		return comparer.compare.apply(comparer, arguments);
	};
};
