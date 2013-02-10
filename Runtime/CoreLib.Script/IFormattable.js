///////////////////////////////////////////////////////////////////////////////
// IFormattable

var ss_IFormattable = function#? DEBUG IFormattable$##() { };
ss_IFormattable.prototype = {
	format: null
};

ss.registerInterface(global, 'ss.IFormattable', ss_IFormattable);

ss.format = function#? DEBUG ss$format##(obj, fmt) {
	if (typeof(obj) === 'number')
		return ss.formatNumber(obj, fmt);
	else if (ss.isDate(obj))
		return ss.formatDate(obj, fmt);
	else
		return obj.format(fmt);
};
