////////////////////////////////////////////////////////////////////////////////
// ArgumentNullException

var ss_ArgumentOutOfRangeException = ss.ArgumentOutOfRangeException = ss.mkType(ss, 'ss.ArgumentOutOfRangeException',
	function#? DEBUG ArgumentOutOfRangeException$##(paramName, message, innerException, actualValue) {
		if (!message) {
			message = 'Value is out of range.';
			if (paramName)
				message += '\nParameter name: ' + paramName;
		}

		ss_ArgumentException.call(this, message, paramName, innerException);
		this.actualValue = actualValue || null;
	}
);

ss.initClass(ss_ArgumentOutOfRangeException, ss_ArgumentException);
