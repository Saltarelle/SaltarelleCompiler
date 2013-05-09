///////////////////////////////////////////////////////////////////////////////
// NumberFormatInfo

var ss_NumberFormatInfo = function#? DEBUG NumberFormatInfo$##() {
    this.naNSymbol = 'NaN';
    this.negativeSign = '-';
    this.positiveSign = '+';
    this.negativeInfinitySymbol = '-Infinity';
    this.positiveInfinitySymbol = 'Infinity';

    this.percentSymbol = '%';
    this.percentGroupSizes = [3];
    this.percentDecimalDigits = 2;
    this.percentDecimalSeparator = '.';
    this.percentGroupSeparator = ',';
    this.percentPositivePattern = 0;
    this.percentNegativePattern = 0;

    this.currencySymbol = '$';
    this.currencyGroupSizes = [3];
    this.currencyDecimalDigits = 2;
    this.currencyDecimalSeparator = '.';
    this.currencyGroupSeparator = ',';
    this.currencyNegativePattern = 0;
    this.currencyPositivePattern = 0;

    this.numberGroupSizes = [3];
    this.numberDecimalDigits = 2;
    this.numberDecimalSeparator = '.';
    this.numberGroupSeparator = ',';
};
ss_NumberFormatInfo.prototype = {
    getFormat:  function#? DEBUG NumberFormatInfo$getFormat##(type) {
        return (ss.getTypeFullName(type) === 'ss.NumberFormatInfo') ? this : null;
    }
};

ss_NumberFormatInfo.__typeName = 'ss.NumberFormatInfo';
ss.NumberFormatInfo = ss_NumberFormatInfo;
ss.initClass(ss_NumberFormatInfo);

ss_NumberFormatInfo.InvariantInfo = new ss_NumberFormatInfo();
