///////////////////////////////////////////////////////////////////////////////
// CultureInfo

var ss_CultureInfo = function#? DEBUG CultureInfo$##(name, numberFormat, dateTimeFormat) {
  this.name = name;
  this.numberFormat = numberFormat;
  this.dateTimeFormat = dateTimeFormat;
};
ss_CultureInfo.prototype = {
  getFormat:  function#? DEBUG CultureInfo$getFormat##(type) {
    if(ss.getTypeFullName(type) === 'ss.NumberFormatInfo')
      return this.numberFormat;
    return (ss.getTypeFullName(type) === 'ss.DateTimeFormatInfo') ? this.dateTimeFormat : null;
  }
};

ss_CultureInfo.__typeName = 'ss.CultureInfo';
ss.CultureInfo = ss_CultureInfo;
ss.initClass(ss_CultureInfo);

ss_CultureInfo.InvariantCulture = new ss_CultureInfo('en-US', ss_NumberFormatInfo.InvariantInfo, ss_DateTimeFormatInfo.InvariantInfo);
ss_CultureInfo.CurrentCulture = ss_CultureInfo.InvariantCulture;
