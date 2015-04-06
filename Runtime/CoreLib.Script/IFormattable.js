///////////////////////////////////////////////////////////////////////////////
// IFormattable

var ss_IFormattable = ss.IFormattable = ss.mkType(ss, 'ss.IFormattable');

ss.initInterface(ss_IFormattable);

ss.format = function#? DEBUG ss$format##(obj, fmt) {
	if (typeof(obj) === 'number')
		return ss.formatNumber(obj, fmt);
	else if (ss.isDate(obj))
		return ss.formatDate(obj, fmt);
	else
		return obj.format(fmt);
};
