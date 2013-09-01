///////////////////////////////////////////////////////////////////////////////
// DateTimeFormatInfo

var ss_DateTimeFormatInfo = function#? DEBUG DateTimeFormatInfo$##() {
};

ss_DateTimeFormatInfo.__typeName = 'ss.DateTimeFormatInfo';
ss.DateTimeFormatInfo = ss_DateTimeFormatInfo;
ss.initClass(ss_DateTimeFormatInfo, ss, {
	getFormat: function#? DEBUG DateTimeFormatInfo$getFormat##(type) {
		return type === ss_DateTimeFormatInfo ? this : null;
	}
}, null, [ss_IFormatProvider]);

ss_DateTimeFormatInfo.invariantInfo = new ss_DateTimeFormatInfo();
ss.shallowCopy({
	amDesignator: 'AM',
	pmDesignator: 'PM',

	dateSeparator: '/',
	timeSeparator: ':',

	gmtDateTimePattern: 'ddd, dd MMM yyyy HH:mm:ss \'GMT\'',
	universalDateTimePattern: 'yyyy-MM-dd HH:mm:ssZ',
	sortableDateTimePattern: 'yyyy-MM-ddTHH:mm:ss',
	dateTimePattern: 'dddd, MMMM dd, yyyy h:mm:ss tt',

	longDatePattern: 'dddd, MMMM dd, yyyy',
	shortDatePattern: 'M/d/yyyy',

	longTimePattern: 'h:mm:ss tt',
	shortTimePattern: 'h:mm tt',

	firstDayOfWeek: 0,
	dayNames: ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],
	shortDayNames: ['Sun','Mon','Tue','Wed','Thu','Fri','Sat'],
	minimizedDayNames: ['Su','Mo','Tu','We','Th','Fr','Sa'],

	monthNames: ['January','February','March','April','May','June','July','August','September','October','November','December',''],
	shortMonthNames: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec','']
}, ss_DateTimeFormatInfo.invariantInfo);
