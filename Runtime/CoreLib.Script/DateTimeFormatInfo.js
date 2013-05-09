///////////////////////////////////////////////////////////////////////////////
// DateTimeFormatInfo

var ss_DateTimeFormatInfo = function#? DEBUG DateTimeFormatInfo$##() {
    this.amDesignator ='AM';
	this.pmDesignator = 'PM';

	this.dateSeparator = '/';
	this.timeSeparator = ':';

	this.gmtDateTimePattern = 'ddd, dd MMM yyyy HH:mm:ss \'GMT\'';
	this.universalDateTimePattern = 'yyyy-MM-dd HH:mm:ssZ';
	this.sortableDateTimePattern = 'yyyy-MM-ddTHH:mm:ss';
	this.dateTimePattern = 'dddd, MMMM dd, yyyy h:mm:ss tt';

	this.longDatePattern = 'dddd, MMMM dd, yyyy';
	this.shortDatePattern = 'M/d/yyyy';

	this.longTimePattern = 'h:mm:ss tt';
	this.shortTimePattern = 'h:mm tt';

	this.firstDayOfWeek = 0;
	this.dayNames = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];
	this.shortDayNames = ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'];
	this.minimizedDayNames = ['Su','Mo','Tu','We','Th','Fr','Sa'];

	this.monthNames = ['January','February','March','April','May','June','July','August','September','October','November','December',''];
	this.shortMonthNames = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec',''];
};
ss_DateTimeFormatInfo.prototype = {
    getFormat:  function#? DEBUG DateTimeFormatInfo$getFormat##(type) {
        return (ss.getTypeFullName(type) === 'ss.DateTimeFormatInfo') ? this : null;
    }
};

ss_DateTimeFormatInfo.__typeName = 'ss.DateTimeFormatInfo';
ss.DateTimeFormatInfo = ss_DateTimeFormatInfo;
ss.initClass(ss_DateTimeFormatInfo);

ss_DateTimeFormatInfo.InvariantInfo = new ss_DateTimeFormatInfo();
