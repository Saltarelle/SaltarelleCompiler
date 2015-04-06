////////////////////////////////////////////////////////////////////////////////
// ArgumentNullException

var ss_ArgumentNullException = ss.ArgumentNullException = ss.mkType(ss, 'ss.ArgumentNullException',
	function#? DEBUG ArgumentNullException$##(paramName, message, innerException) {
		if (!message) {
			message = 'Value cannot be null.';
			if (paramName)
				message += '\nParameter name: ' + paramName;
		}

		ss_ArgumentException.call(this, message, paramName, innerException);
	}
);

ss.initClass(ss_ArgumentNullException, ss_ArgumentException);
