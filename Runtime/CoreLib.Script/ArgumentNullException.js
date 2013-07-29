////////////////////////////////////////////////////////////////////////////////
// ArgumentNullException

var ss_ArgumentNullException = function#? DEBUG ArgumentNullException$##(paramName, message, innerException) {
	if (!message) {
		message = 'Value cannot be null.';
		if (paramName)
			message += '\nParameter name: ' + paramName;
	}

	ss_ArgumentException.call(this, message, paramName, innerException);
};

ss_ArgumentNullException.__typeName = 'ss.ArgumentNullException';
ss.ArgumentNullException = ss_ArgumentNullException;
ss.initClass(ss_ArgumentNullException, {}, ss_ArgumentException);
