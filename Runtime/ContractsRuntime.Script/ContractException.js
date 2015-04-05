////////////////////////////////////////////////////////////////////////////////
// ContractException

var ss_ContractException = function#? DEBUG ContractException$##(failureKind, failureMessage, userMessage, condition, innerException) {
	ss.Exception.call(this, failureMessage, innerException);
	this._kind = failureKind;
	this._userMessage = userMessage;
	this._condition = condition;
};
ss_ContractException.__typeName = 'ss.ContractException';
ss.ContractException = ss_ContractException;
ss.initClass(ss_ContractException, ss, {
	get_kind: function#? DEBUG ContractException$get_failureKind##() {
	    return this._kind;
	},
	get_failure: function#? DEBUG ContractException$get_failure##() {
	    return this._failureMessage;
	},
	get_userMessage: function#? DEBUG ContractException$get_userMessage##() {
		return this._userMessage;
	},
	get_condition: function#? DEBUG ContractException$get_condition##() {
	    return this._condition;
	}
}, ss.Exception);