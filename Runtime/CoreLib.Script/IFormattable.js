///////////////////////////////////////////////////////////////////////////////
// IFormattable

var ss_IFormattable = function#? DEBUG IFormattable$##() { };

ss_IFormattable.__typeName = 'ss.IFormattable';
ss.IFormattable = ss_IFormattable;
ss.initInterface(ss_IFormattable, ss, { format: null });

var ss_ILocaleFormattable = function#? DEBUG ILocaleFormattable$##() { };

ss_ILocaleFormattable.__typeName = 'ss.ILocaleFormattable';
ss.ILocaleFormattable = ss_ILocaleFormattable;
ss.initInterface(ss_ILocaleFormattable, ss, { formatLocale: null }, [ss_IFormattable]);

ss.format = function#? DEBUG ss$format##(obj, fmt, useLocale) {
	if (typeof(obj) === 'number')
	    return useLocale ? ss.localeFormatNumber(obj, fmt) : ss.formatNumber(obj, fmt);
	else if (ss.isDate(obj))
	    return useLocale ? ss.localeFormatDate(obj, fmt) : ss.formatDate(obj, fmt);
	else
		return useLocale ? obj.localeFormat(fmt) : obj.format(fmt);
};
