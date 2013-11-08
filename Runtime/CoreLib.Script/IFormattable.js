///////////////////////////////////////////////////////////////////////////////
// IFormattable

var ss_IFormattable = function#? DEBUG IFormattable$##() { };

ss_IFormattable.__typeName = 'ss.IFormattable';
ss.IFormattable = ss_IFormattable;
ss.initInterface(ss_IFormattable, ss, { format: null });

ss.format = function#? DEBUG ss$format##(obj, fmt) {
	if (typeof(obj) === 'number')
		return ss.formatNumber(obj, fmt);
	else if (ss.isDate(obj))
		return ss.formatDate(obj, fmt);
	else
		return obj.format(fmt);
};
