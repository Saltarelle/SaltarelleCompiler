////////////////////////////////////////////////////////////////////////////////
// ContractException

var ss_ContractException = ss.ContractException = ss.mkType(ss, 'ss.ContractException',
	function#? DEBUG ContractException$##(failureKind, failureMessage, userMessage, condition, innerException) {
		ss.Exception.call(this, failureMessage, innerException);
		this._kind = failureKind;
		this._userMessage = userMessage;
		this._condition = condition;
	},
	{
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
	}
);
ss.initClass(ss_ContractException, ss.Exception);