///////////////////////////////////////////////////////////////////////////////
// CultureInfo

var ss_CultureInfo = function#? DEBUG CultureInfo$##(name, numberFormat, dateTimeFormat) {
	this.name = name;
	this.numberFormat = numberFormat;
	this.dateTimeFormat = dateTimeFormat;
};
ss_CultureInfo.prototype = {
	getFormat:  function#? DEBUG CultureInfo$getFormat##(type) {
		switch (type) {
			case ss_NumberFormatInfo: return this.numberFormat;
			case ss_DateTimeFormatInfo: return this.dateTimeFormat;
			default: return null;
		}
	}
};

ss_CultureInfo.__typeName = 'ss.CultureInfo';
ss.CultureInfo = ss_CultureInfo;
ss.initClass(ss_CultureInfo, null, [ss_IFormatProvider]);

ss_CultureInfo.invariantCulture = new ss_CultureInfo('en-US', ss_NumberFormatInfo.invariantInfo, ss_DateTimeFormatInfo.invariantInfo);
ss_CultureInfo.currentCulture = ss_CultureInfo.invariantCulture;
