////////////////////////////////////////////////////////////////////////////////
// AggregateException

var ss_ArgumentNullException = function#? DEBUG ArgumentNullException$##(message, innerException) {
    
    if (typeof message === 'undefined' && typeof innerException === 'undefined')
        ss_Exception.call(this, message, innerException);

    if (typeof message === 'string' && typeof innerException === 'undefined')
        ss_Exception.call(this, 'Argument cannot  be null : ' + message , innerException);

    if (typeof message === 'string' && typeof innerException === 'string'){
        ss_Exception.call(this,  message, innerException);
        this.argumentName = message;
    }
    if (typeof message === 'string' && typeof innerException === 'ss_Exception')
        ss_Exception.call(this,  message, innerException);
    
};


ss_ArgumentNullException.prototype = {
    get_argumentName: function#? DEBUG ArgumentNullException$get_argumentName##() {
        return this.argumentName;
	}
};


ss_ArgumentNullException.__typeName = 'ss.ArgumentNullException';
ss.ArgumentNullException = ss_ArgumentNullException;
ss.initClass(ss_ArgumentNullException, ss_Exception);
